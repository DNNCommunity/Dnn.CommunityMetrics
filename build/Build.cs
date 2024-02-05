using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.UbuntuLatest,
    ImportSecrets = new[] { nameof(GitHubToken) },
    OnPullRequestBranches = new[] { "develop", "main", "master", "release/*" },
    OnPushBranches = new[] { "main", "master", "develop", "release/*" },
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

    [Parameter("Github Token")] readonly string GitHubToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath DnnBinDirectory => RootDirectory.Parent.Parent / "bin";

    private string ProjectName = "Dnn.CommunityMetrics";
    private string releaseNotes;

    GitHubClient gitHubClient;
    Release release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            MSBuild(s => s
                .SetProjectFile(Solution.GetProject(ProjectName))
                .SetTargets("Restore"));
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
        .Before(Release)
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
        .DependsOn(Release)
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

    Target GenerateReleaseNotes => _ => _
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(SetupGitHubClient)
        .Executes(() =>
        {
            // Get the milestone
            var milestone = gitHubClient.Issue.Milestone.GetAllForRepository(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName()).Result
                .Where(m => m.Title == GitVersion.MajorMinorPatch).FirstOrDefault();
            Serilog.Log.Information(milestone.ToJson());
            if (milestone == null)
            {
                Serilog.Log.Warning("Milestone not found for this version");
                releaseNotes = "No release notes for this version.";
                return;
            }

            try
            {
                // Get the PRs
                var prRequest = new PullRequestRequest()
                {
                    State = ItemStateFilter.All
                };
                var allPrs = Task.Run(() =>
                    gitHubClient.Repository.PullRequest.GetAllForRepository(
                            GitRepository.GetGitHubOwner(),
                        GitRepository.GetGitHubName(), prRequest)
                ).Result;

                var pullRequests = allPrs.Where(p =>
                    p.Milestone?.Title == milestone.Title &&
                    p.Merged == true &&
                    p.Milestone?.Title == GitVersion.MajorMinorPatch);
                Serilog.Log.Information(pullRequests.ToJson());

                // Build release notes
                var releaseNotesBuilder = new StringBuilder();
                releaseNotesBuilder
                    .AppendLine($"# {GitRepository.GetGitHubName()} {milestone.Title}")
                    .AppendLine()
                    .AppendLine($"A total of {pullRequests.Count()} pull requests where merged in this release.")
                    .AppendLine();

                foreach (var group in pullRequests.GroupBy(p => p.Labels[0]?.Name, (label, prs) => new { label, prs }))
                {
                    Serilog.Log.Information(group.ToJson());
                    releaseNotesBuilder.AppendLine($"## {group.label}");
                    foreach (var pr in group.prs)
                    {
                        Serilog.Log.Information(pr.ToJson());
                        releaseNotesBuilder.AppendLine($"- #{pr.Number} {pr.Title}. Thanks @{pr.User.Login}");
                    }
                }

                releaseNotes = releaseNotesBuilder.ToString();
                Serilog.Log.Information(releaseNotes);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Something went wrong with the github api call.");
                throw;
            }
        });

    Target Release => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch()))
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(SetupGitHubClient)
        .DependsOn(GenerateReleaseNotes)
        .DependsOn(TagRelease)
        .DependsOn(Package)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .Executes(() =>
        {
            var newRelease = new NewRelease(GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}")
            {
                Body = releaseNotes,
                Draft = true,
                Name = GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}",
                TargetCommitish = GitVersion.Sha,
                Prerelease = GitRepository.IsOnReleaseBranch(),
            };
            release = gitHubClient.Repository.Release.Create(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName(),
                newRelease).Result;
            Serilog.Log.Information($"{release.Name} released !");

            var artifactFile = RootDirectory.GlobFiles("artifacts/**/*.zip").FirstOrDefault();
            var artifact = File.OpenRead(artifactFile);
            var artifactInfo = new FileInfo(artifactFile);
            var assetUpload = new ReleaseAssetUpload()
            {
                FileName = artifactInfo.Name,
                ContentType = "application/zip",
                RawData = artifact
            };
            var asset = gitHubClient.Repository.Release.UploadAsset(release, assetUpload).Result;
            Serilog.Log.Information($"Asset {asset.Name} published at {asset.BrowserDownloadUrl}");
        });

    Target TagRelease => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch()))
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(SetupGitHubClient)
        .Before(Compile)
        .Executes(() =>
        {
            Git($"remote set-url origin https://{GitRepository.GetGitHubOwner()}:{GitHubToken}@github.com/{GitRepository.GetGitHubOwner()}/{GitRepository.GetGitHubName()}.git");
            var version = GitRepository.IsOnMainOrMasterBranch() ? GitVersion.MajorMinorPatch : GitVersion.SemVer;
            GitLogger = (type, output) => Serilog.Log.Information(output);
            Git($"tag v{version}");
            Git($"push --tags");
        });

    Target SetupGitHubClient => _ => _
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .OnlyWhenDynamic(() => GitRepository != null)
        .Executes(() =>
        {
            Serilog.Log.Information($"We are on branch {GitRepository.Branch}");
            if (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
            {
                gitHubClient = new GitHubClient(new ProductHeaderValue("Nuke"));
                var tokenAuth = new Credentials(GitHubToken);
                gitHubClient.Credentials = tokenAuth;
            }
        });
}
