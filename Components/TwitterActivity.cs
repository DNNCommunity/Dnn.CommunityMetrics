using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dnn.CommunityMetrics
{
    public class TwitterActivity : IActivity
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
                name = "Consumer Key",
                help_text = "Consumer Key From Twitter http://apps.twitter.com"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Consumer Secret",
                help_text = "Consumer Secret From Twitter http://apps.twitter.com"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Access Token",
                help_text = "Access Token From Twitter http://apps.twitter.com"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Access Secret",
                help_text = "Access Secret From Twitter http://apps.twitter.com"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Query",
                help_text = "Twitter Search Query ( ie. #twitter OR @twitter )"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Profile",
                help_text = "The Name Of The User Profile Field For Twitter Accounts In Your Site"
            });

            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objActivity"></param>
        /// <param name="ExecutionDate"></param>
        /// <returns></returns>
        public List<UserActivityDTO> GetUserActivity(ActivityDTO activity)
        {
            List<UserActivityDTO> user_activities = new List<UserActivityDTO>();

            TwitterAPI api = new TwitterAPI(
                activity.settings["Access Token"].ToString(),
                activity.settings["Access Secret"].ToString(),
                activity.settings["Consumer Key"].ToString(),
                activity.settings["Consumer Secret"].ToString());

            foreach (JSONObject json in api.Get("search/tweets.json", new Parameters { { "q", activity.settings["Query"].ToString() } }))
            {
                foreach (JSONObject status in json.GetList<JSONObject>("statuses"))
                {
                    string ScreenName = status.Get("user.screen_name").ToString();
                    var user_profile = dc.UserProfiles.Where(i => i.ProfilePropertyDefinition.PropertyName == activity.settings["Profile"].ToString() && i.PropertyValue == ScreenName).SingleOrDefault();

                    if (user_profile != null) // tweet was made by a community member
                    {
                        Nullable<DateTime> last_activity_date = dc.CommunityMetrics_UserActivities.Where(i => i.user_id == user_profile.UserID && i.activity_id == activity.id).OrderByDescending(i => i.date).Select(i => i.date).FirstOrDefault();

                        DateTime CreatedDate = DateTime.ParseExact(status.Get("created_at").ToString(), "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture).Date;

                        if (CreatedDate > last_activity_date.GetValueOrDefault() && CreatedDate < DateTime.Now.Date)
                        {
                            // record the points

                            var user_activity = user_activities.Where(i => i.user_id == user_profile.UserID && i.date == CreatedDate).SingleOrDefault();

                            if (user_activity == null)
                            {
                                user_activity = new UserActivityDTO()
                                {
                                    user_id = user_profile.UserID,
                                    activity_id = activity.id,
                                    count = 0,
                                    date = CreatedDate
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
