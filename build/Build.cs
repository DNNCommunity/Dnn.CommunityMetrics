using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.WindowsLatest,
    ImportSecrets = new[] { nameof(GitHubToken) },
    OnPullRequestBranches = new[] { "develop", "main", "master", "release/" },
    OnPushBranches = new[] { "main", "master", "develop", "release/" },
    InvokedTargets = new[] { nameof(CI) },
    FetchDepth = 0
    )]

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Package);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    readonly string GitHubToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath DnnBinDirectory => RootDirectory.Parent.Parent / "bin";

    private string ProjectName = "Dnn.CommunityMetrics";
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution.GetProject(ProjectName)));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(SetManifestVersions)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetProjectFile(Solution.GetProject(ProjectName))
                .SetFileVersion(GitVersion.SemVer)
                .SetInformationalVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.SemVer)
                .SetConfiguration(Configuration));
        });

    Target Deploy => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var assembly = RootDirectory / "bin" / Configuration / $"{ProjectName}.dll";
            CopyFileToDirectory(assembly, DnnBinDirectory, policy: FileExistsPolicy.Overwrite);
        });

    Target Package => _ => _
        .DependsOn(Clean)
        .DependsOn(Compile)
        .Executes(() =>
        {
            var stagingDirectory = RootDirectory / "staging";
            var rootFiles = RootDirectory.GlobFiles("*.dnn", "LICENSE");
            var rootResourceFiles = RootDirectory.GlobFiles("*.ascx", "*.css");
            var assembly = RootDirectory / "bin" / Configuration / $"{ProjectName}.dll";
            var appFolder = RootDirectory / "app";
            var pluginsFolder = RootDirectory / "plugins";
            var scriptsFolder = RootDirectory / "Scripts";
            var resourcesDirectory = stagingDirectory / "resources";

            rootFiles.ForEach(file => CopyFileToDirectory(file, stagingDirectory, policy: FileExistsPolicy.Overwrite, createDirectories: true));
            rootResourceFiles.ForEach(file => CopyFileToDirectory(file, resourcesDirectory, policy: FileExistsPolicy.Overwrite, createDirectories: true));
            CopyDirectoryRecursively(scriptsFolder, stagingDirectory / "Scripts", directoryPolicy: DirectoryExistsPolicy.Merge, filePolicy: FileExistsPolicy.Overwrite);
            CopyDirectoryRecursively(appFolder, resourcesDirectory / "app", directoryPolicy: DirectoryExistsPolicy.Merge, filePolicy: FileExistsPolicy.Overwrite);
            CopyDirectoryRecursively(pluginsFolder, resourcesDirectory / "plugins", directoryPolicy: DirectoryExistsPolicy.Merge, filePolicy: FileExistsPolicy.Overwrite);
            CopyFileToDirectory(assembly, stagingDirectory / "bin", policy: FileExistsPolicy.Overwrite, createDirectories: true);

            resourcesDirectory.ZipTo(stagingDirectory / "resources.zip");
            resourcesDirectory.DeleteDirectory();
            stagingDirectory.ZipTo(ArtifactsDirectory / $"{ProjectName}_{GitVersion.SemVer}_Install.zip");

            stagingDirectory.DeleteDirectory();
        });

    Target CI => _ => _
        .DependsOn(Package)
        .Produces(ArtifactsDirectory)
        .Executes(() =>
        {
        });

    Target SetManifestVersions => _ => _
        .Executes(() =>
        {
            var manifests = RootDirectory.GlobFiles("*.dnn");
            foreach (var manifest in manifests)
            {
                var doc = new XmlDocument();
                doc.Load(manifest);
                var packages = doc.SelectNodes("dotnetnuke/packages/package");
                foreach (XmlNode package in packages)
                {
                    var version = package.Attributes["version"];
                    if (version != null)
                    {
                        Serilog.Log.Information($"Found package {package.Attributes["name"].Value} with version {version.Value}");
                        version.Value =
                            GitVersion != null
                            ? $"{GitVersion.Major.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Minor.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Patch.ToString("00", CultureInfo.InvariantCulture)}"
                            : "00.01.00";
                        Serilog.Log.Information($"Updated package {package.Attributes["name"].Value} to version {version.Value}");

                        var components = package.SelectNodes("components/component");
                        foreach (XmlNode component in components)
                        {
                            if (component.Attributes["type"].Value == "Cleanup")
                            {
                                var cleanupVersion = component.Attributes["version"];
                                cleanupVersion.Value =
                                    GitVersion != null
                                    ? $"{GitVersion.Major.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Minor.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Patch.ToString("00", CultureInfo.InvariantCulture)}"
                                    : "00.01.00";
                            }
                        }
                    }
                }
                doc.Save(manifest);
                Serilog.Log.Information($"Saved {manifest}");
            }
        });

}
