using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.CommunityMetrics
{
    public class DatabaseActivity : IActivity
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
                name = "Query",
                help_text = "SELECT [USERID] AS user_id, [DATE] AS date, count(*) as count FROM [Table] GROUP BY [USERID], [DATE]"
            });
            return settings;
        }

        public List<UserActivityDTO> GetUserActivity(ActivityDTO activity)
        {
            List<UserActivityDTO> user_activities = new List<UserActivityDTO>();

            var activity_records = dc.ExecuteQuery<UserActivityDTO>(activity.settings["Query"].ToString()).ToList();
            foreach (var activity_record in activity_records)
            {
                UserActivityDTO user_activity = user_activities.Where(i => i.user_id == activity_record.user_id && i.date == activity_record.date.Date).SingleOrDefault();
                if (user_activity == null)
                {
                    user_activity = new UserActivityDTO()
                    {
                        user_id = activity_record.user_id,
                        activity_id = activity_record.activity_id,
                        count = 0,
                        created_on_date = DateTime.Now,
                        date = activity_record.date.Date
                    };
                    user_activities.Add(user_activity);
                }
                user_activity.count++;
            }
            return user_activities.ToList();
        }
    }
}