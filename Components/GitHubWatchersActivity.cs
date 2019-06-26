using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.CommunityMetrics
{
    public class GitHubWatchersActivity : IActivity
    {
        DataContext dc = new DataContext();
        public MetricTypeEnum MetricType
        {
            get { return MetricTypeEnum.Cumulative; }
        }

        public List<ActivitySettingDTO> GetSettings()
        {
            List<ActivitySettingDTO> settings = new List<ActivitySettingDTO>();

            settings.Add(new ActivitySettingDTO()
            {
                name = "Credentials",
                help_text = "A Personal Acccess Token You Create In Your GitHub Account"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Profile",
                help_text = "The Name Of The User Profile Field You Created For GitHub Accounts In Your Site"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Query",
                help_text = "Search for GitHub repositories matching this tag"
            });

            return settings;
        }

        public List<UserActivityDTO> GetUserActivity(ActivityDTO activity)
        {
            List<UserActivityDTO> user_activities = new List<UserActivityDTO>();

            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("Dnn.CommunityActivity"));
            gitHubClient.Credentials = new Credentials(activity.settings["Credentials"].ToString());

            List<Repository> repositories = new List<Repository>();

            var totalCount = int.MaxValue;
            var page = 1;

            // get a list of all the repos matching the search criteria
            while (repositories.Count() < totalCount)
            {
                var request = new SearchRepositoriesRequest(activity.settings["Query"].ToString())
                {
                    Page = page,
                };
                var result = gitHubClient.Search.SearchRepo(request).Result;
                totalCount = result.TotalCount;
                repositories.AddRange(result.Items);
                page++;
            }

            foreach (Repository repository in repositories)
            {
                IReadOnlyList<Octokit.User> users = gitHubClient.Activity.Watching.GetAllWatchers(repository.Id).Result;
                foreach (Octokit.User user in users)
                {
                    var user_profile = dc.UserProfiles.Where(i => i.ProfilePropertyDefinition.PropertyName == activity.settings["Profile"].ToString() && i.PropertyValue == user.Login).SingleOrDefault();

                    if (user_profile != null)
                    {
                        var user_activity = user_activities.Where(i => i.user_id == user_profile.UserID && i.activity_id == activity.id).Single();
                        if (user_activity == null)
                        {
                            user_activity = new UserActivityDTO()
                            {
                                user_id = user_profile.UserID,
                                activity_id = activity.id,
                                count = 0,
                                created_on_date = DateTime.Now,
                                date = DateTime.Now
                            };
                            user_activities.Add(user_activity);
                        }
                        user_activity.count++;
                    }
                }
            }
            return user_activities;
        }
    }
}
