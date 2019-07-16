using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace Dnn.CommunityMetrics
{
    public class StackOverflowActivity : IActivity
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
                name = "Tag",
                help_text = "StackOverflow Tag ( ie. asp.net )"
            });

            settings.Add(new ActivitySettingDTO()
            {
                name = "Profile",
                help_text = "The Name Of The User Profile Field For StackOverflow User Ids In Your Site"
            });

            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public List<UserActivityDTO> GetUserActivity(ActivityDTO activity)
        {
            Dictionary<int, int> arrUsers = new Dictionary<int, int>();

            // returns the top 20 users in the past month who have used a specified tag in their answers on StackOverflow 
            var apiUrl = ("http://api.stackexchange.com/2.2/tags/" + activity.settings["Tag"].ToString() + "/top-answerers/month?site=stackoverflow");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiUrl);
            httpWebRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            httpWebRequest.Method = "GET";
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string responseText;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseText = streamReader.ReadToEnd();
            }
            var result = (Result)new JavaScriptSerializer().Deserialize(responseText, typeof(Result));
            foreach (TagScore item in result.items)
            {
                var user_profile = dc.UserProfiles.Where(i => i.ProfilePropertyDefinition.PropertyName == activity.settings["Profile"].ToString() && i.PropertyValue == item.user.user_id.ToString()).SingleOrDefault();

                if (user_profile != null)
                {
                    arrUsers.Add(user_profile.UserID, item.post_count);
                }
            }

            List<UserActivityDTO> user_activities = new List<UserActivityDTO>();
            foreach (KeyValuePair<int, int> kvp in arrUsers)
            {
                UserActivityDTO user_activity = new UserActivityDTO()
                {
                    user_id = kvp.Key,
                    count = kvp.Value,
                    activity_id = activity.id,
                    date = DateTime.Now
                };
                user_activities.Add(user_activity);
            }
            return user_activities;
        }
    }

    class Result
    {
        public List<TagScore> items { get; set; }
    }
    class TagScore
    {
        public ShallowUser user { get; set; }
        public int post_count { get; set; }
        public int score { get; set; }
    }
    class ShallowUser
    {
        public int reputation { get; set; }
        public int user_id { get; set; }
        public string user_type { get; set; }
        public string profile_image { get; set; }
        public string display_name { get; set; }
        public string link { get; set; }
    }
}
