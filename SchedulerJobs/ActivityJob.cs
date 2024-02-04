using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;

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
                                    this.SaveUserActivity(user_activity);
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

        private void SaveUserActivity(UserActivityDTO dto)
        {
            DataContext dc = new DataContext();

            // prevents saving of duplicate activity
            CommunityMetrics_UserActivity user_activity = dc.CommunityMetrics_UserActivities.Where(i => i.activity_id == dto.activity_id && i.user_id == dto.user_id && i.date == dto.date).SingleOrDefault();
            if (user_activity == null)
            {
                user_activity = new CommunityMetrics_UserActivity()
                {
                    activity_id = dto.activity_id,
                    user_id = dto.user_id
                };
                var userExists = false;
                var portals = PortalController.Instance.GetPortals();
                foreach (IPortalInfo portal in portals)
                {
                    var user = UserController.Instance.GetUser(portal.PortalId, dto.user_id);
                    if (user != null)
                    {
                        userExists = true;
                        break;
                    }
                }
                if (!userExists)
                {
                    return;
                }
                dc.CommunityMetrics_UserActivities.InsertOnSubmit(user_activity);
            }

            user_activity.count = dto.count;
            user_activity.notes = dto.notes;
            dto.links.ToList().ForEach(i =>
            {
                CommunityMetrics_UserActivityLink link = new CommunityMetrics_UserActivityLink()
                {
                    text = i.text,
                    href = i.href
                };
                user_activity.CommunityMetrics_UserActivityLinks.Add(link);
            });
            user_activity.date = dto.date;
            user_activity.created_on_date = DateTime.Now;

            dc.SubmitChanges();
        }

    }
}