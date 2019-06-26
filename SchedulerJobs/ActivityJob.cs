using System;
using System.Collections.Generic;

namespace Dnn.CommunityMetrics
{
    public class ActivityJob : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public ActivityJob(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                Progressing();
                string strMessage = ProcessActivities();
                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote("Successful. " + strMessage);
            }
            catch (Exception exc)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("Failed. " + exc.ToString());
                Errored(ref exc);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
            }
        }

        public string ProcessActivities()
        {
            string strMessage = "<br/>";
            DateTime datDate = DateTime.Now;
            int intCount = 0;

            // iterate through activities
            ActivityController activityController = new ActivityController();
            UserActivityController userActivityController = new UserActivityController();

            foreach (ActivityDTO activity in activityController.GetActivities())
            {
                if (activity.active)
                {
                    strMessage += "<br/>Activity: " + activity.name + "<br/>";

                    intCount = 0;

                    try
                    {
                        List<UserActivityDTO> user_activities = userActivityController.GetUserActivity(activity, datDate);
                        foreach (UserActivityDTO user_activity in user_activities)
                        {
                            user_activity.activity_id = activity.id;

                            if (string.IsNullOrWhiteSpace(activity.user_filter) || ("," + activity.user_filter + ",").IndexOf("," + user_activity.user_id.ToString() + ",") == -1)
                            {
                                if (activity.min_daily > 0 && user_activity.count < activity.min_daily)
                                {
                                    user_activity.count = 0;
                                }
                                if (activity.max_daily > 0 && user_activity.count > activity.max_daily)
                                {
                                    user_activity.count = activity.max_daily;
                                }
                                if (user_activity.count > 0)
                                {
                                    userActivityController.SaveUserActivity(user_activity);
                                    intCount += 1;
                                }
                            }
                        }
                        strMessage += " User Activity: " + intCount.ToString() + "<br/>";
                        activityController.SaveActivity(activity);
                    }
                    catch (Exception ex)
                    {
                        strMessage += ex.ToString() + "<br/>";
                    }

                }
            }
            return strMessage;
        }
    }
}