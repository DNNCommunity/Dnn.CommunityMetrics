using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.CommunityMetrics
{
    public class GitHubPullRequestActivity : IActivity
    {
        DataContext dc = new DataContext();
        public MetricTypeEnum MetricType
        {
            get { return MetricTypeEnum.Daily; }
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

            var user_profiles = dc.UserProfiles.Where(i => i.ProfilePropertyDefinition.PropertyName == activity.settings["Profile"].ToString()).Select(i => new { user_id = i.UserID, gitHub_login = i.PropertyValue }).ToList();

            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("Dnn.CommunityActivity"));
            gitHubClient.Credentials = new Credentials(activity.settings["Credentials"].ToString());

            PullRequestRequest objOptions = new PullRequestRequest();
            objOptions.State = ItemStateFilter.All;

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
                IReadOnlyList<PullRequest> pull_requests = gitHubClient.PullRequest.GetAllForRepository(repository.Id, objOptions).Result;

                if (pull_requests.Any())
                {
                    foreach (var user_profile in user_profiles)
                    {
                        Nullable<DateTime> last_activity_date = dc.CommunityMetrics_UserActivities.Where(i => i.user_id == user_profile.user_id && i.activity_id == activity.id).OrderByDescending(i => i.date).Select(i => i.date).FirstOrDefault();

                        var recent_pull_requests = pull_requests
                          .Where(i =>
                          i.User != null &&
                          i.User.Login == user_profile.gitHub_login &&
                          i.CreatedAt.Date > last_activity_date.GetValueOrDefault() &&
                          i.CreatedAt.Date < DateTime.Now.Date &&
                          i.Merged == true &&
                          i.MergedAt.HasValue
                          );

                        foreach (PullRequest pull_request in recent_pull_requests)
                        {
                            // record the points
                            var user_activity = user_activities.Where(i => i.user_id == user_profile.user_id && i.date == pull_request.CreatedAt.Date).SingleOrDefault();

                            if (user_activity == null)
                            {
                                user_activity = new UserActivityDTO()
                                {
                                    user_id = user_profile.user_id,
                                    activity_id = activity.id,
                                    count = 0,
                                    date = pull_request.CreatedAt.Date
                                };
                                user_activities.Add(user_activity);
                            }
                            user_activity.count++;
                        }
                    }
                }
            }
            return user_activities;
        }
    }
}
