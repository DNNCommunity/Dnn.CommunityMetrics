using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Compilation;
using System.Web.Http;

namespace Dnn.CommunityMetrics
{
    //[SupportedModules("Dnn.CommunityMetrics")]
    public class UserActivityController : DnnApiController
    {
        DataContext dc = new DataContext();

        [NonAction]
        public UserActivityDTO ConvertItemToDto(CommunityMetrics_UserActivity item)
        {
            UserActivityDTO dto = new UserActivityDTO();

            dto.id = item.id;
            dto.activity_id = item.activity_id;
            dto.user_id = item.user_id;
            dto.date = item.date;
            dto.count = item.count;
            dto.notes = item.notes;
            dto.created_on_date = item.created_on_date;

            if (!string.IsNullOrWhiteSpace(item.User.DisplayName))
            {
                dto.user_name = item.User.DisplayName;
            }
            else
            {
                dto.user_name = item.User.FirstName + " " + item.User.LastName;
            }

            dto.activity_name = item.CommunityMetrics_Activity.name;

            return dto;
        }
        [NonAction]
        public CommunityMetrics_UserActivity ConvertDtoToItem(CommunityMetrics_UserActivity item, UserActivityDTO dto)
        {
            if (item == null)
            {
                item = new CommunityMetrics_UserActivity();
            }

            if (dto == null)
            {
                return item;
            }

            item.id = dto.id;
            item.activity_id = dto.activity_id;
            item.user_id = dto.user_id;
            item.date = dto.date;
            item.count = dto.count;
            item.notes = dto.notes;
            item.created_on_date = dto.created_on_date;

            return item;
        }


        // returns a list of user activity -  filter for user, activity, or time period
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(Nullable<int> user_id, Nullable<int> activity_id, Nullable<DateTime> period_start, Nullable<DateTime> period_end, Nullable<int> skip = null, Nullable<int> take = null)
        {
            try
            {
                var query = dc.CommunityMetrics_UserActivities.Where(i => i.CommunityMetrics_Activity.active == true);

                // user_id?
                if (user_id.HasValue)
                {
                    query = query.Where(i => i.user_id == user_id.GetValueOrDefault());
                }

                // activity_id?
                if (activity_id.HasValue)
                {
                    query = query.Where(i => i.activity_id == activity_id.GetValueOrDefault());
                }

                // period_start?
                if (period_start.HasValue)
                {
                    query = query.Where(i => i.date >= period_start.GetValueOrDefault());
                }

                // period_end?
                if (period_end.HasValue)
                {
                    query = query.Where(i => i.date <= period_end.GetValueOrDefault());
                }

                // skip?
                if (skip.HasValue)
                {
                    query = query.Skip(skip.GetValueOrDefault());
                }

                // take?
                if (take.HasValue)
                {
                    query = query.Take(take.GetValueOrDefault());
                }

                List<UserActivityDTO> dtos = new List<UserActivityDTO>();
                foreach (CommunityMetrics_UserActivity item in query)
                {
                    UserActivityDTO dto = ConvertItemToDto(item);
                    dtos.Add(dto);
                }

                return Request.CreateResponse(HttpStatusCode.OK, dtos);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // returns a list of point grouped by activity - can filter for user or time period
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(Nullable<int> user_id, Nullable<DateTime> period_start, Nullable<DateTime> period_end, Nullable<int> skip = null, Nullable<int> take = null)
        {
            try
            {
                var query = dc.CommunityMetrics_UserActivities.Where(i => i.CommunityMetrics_Activity.active == true);

                // user_id?
                if (user_id.HasValue)
                {
                    query = query.Where(i => i.user_id == user_id.GetValueOrDefault());
                }

                // period_start?
                if (period_start.HasValue)
                {
                    query = query.Where(i => i.date >= period_start.GetValueOrDefault());
                }

                // period_end?
                if (period_end.HasValue)
                {
                    query = query.Where(i => i.date <= period_end.GetValueOrDefault());
                }

                var list = query
                           .GroupBy(i => i.activity_id)
                           .Select(g => new
                           {
                               activity_id = g.Key,
                               activity_name = g.Select(i => i.CommunityMetrics_Activity.name).FirstOrDefault(),
                               count = g.Sum(i => i.count),
                               total_points = g.Sum(i => i.CommunityMetrics_Activity.factor * i.count)
                           })
                           .OrderByDescending(i => i.total_points)
                           .AsEnumerable();

                // skip?
                if (skip.HasValue)
                {
                    list = list.Skip(skip.GetValueOrDefault());
                }

                // take?
                if (take.HasValue)
                {
                    list = list.Take(take.GetValueOrDefault());
                }

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // returns a ranked list of users for a given time period
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(string user_search, Nullable<DateTime> period_start, Nullable<DateTime> period_end, Nullable<int> skip = null, Nullable<int> take = null)
        {
            try
            {
                var query = dc.CommunityMetrics_UserActivities.Where(i => i.CommunityMetrics_Activity.active == true);

                // period_start?
                if (period_start.HasValue)
                {
                    query = query.Where(i => i.date >= period_start.GetValueOrDefault());
                }

                // period_end?
                if (period_end.HasValue)
                {
                    query = query.Where(i => i.date <= period_end);
                }

                var list = query
                            .GroupBy(i => i.user_id)
                            .Select(g => new
                            {
                                user_id = g.Key,
                                user_name = g.Select(i => i.User.DisplayName).FirstOrDefault(),
                                total_points = g.Sum(i => i.CommunityMetrics_Activity.factor * i.count)
                            })
                            .OrderByDescending(i => i.total_points)
                            .AsEnumerable();

                var ranked_list = list.Select((a, b) => new
                {
                    a.user_id,
                    rank = b,
                    a.user_name,
                    a.total_points
                }).AsEnumerable();

                // user_search?
                if (!string.IsNullOrEmpty(user_search))
                {
                    ranked_list = ranked_list.Where(i => i.user_name.ToLower().Contains(user_search.ToLower()));
                }

                // skip?
                if (skip.HasValue)
                {
                    ranked_list = ranked_list.Skip(skip.GetValueOrDefault());
                }

                // take?
                if (take.HasValue)
                {
                    ranked_list = ranked_list.Take(take.GetValueOrDefault());
                }

                return Request.CreateResponse(HttpStatusCode.OK, ranked_list);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                CommunityMetrics_UserActivity item = dc.CommunityMetrics_UserActivities.Where(i => i.id == id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK, ConvertItemToDto(item));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Post(UserActivityDTO dto)
        {
            try
            {
                CommunityMetrics_UserActivity item = ConvertDtoToItem(null, dto);

                item.created_on_date = DateTime.Now;

                dc.CommunityMetrics_UserActivities.InsertOnSubmit(item);
                dc.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, ConvertItemToDto(item));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Put(UserActivityDTO dto)
        {
            try
            {
                CommunityMetrics_UserActivity item = dc.CommunityMetrics_UserActivities.Where(i => i.id == dto.id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                item = ConvertDtoToItem(item, dto);

                dc.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, ConvertItemToDto(item)); // send back the updated record
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                CommunityMetrics_UserActivity item = dc.CommunityMetrics_UserActivities.Where(i => i.id == id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                dc.CommunityMetrics_UserActivities.DeleteOnSubmit(item);
                dc.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [NonAction]
        public List<UserActivityDTO> GetUserActivity(ActivityDTO activity, DateTime date)
        {
            IActivity objIActivity = (IActivity)Activator.CreateInstance(BuildManager.GetType(activity.type_name, true));

            List<UserActivityDTO> user_activities = objIActivity.GetUserActivity(activity);

            // if this is a cumulative type metric, need to subtract out the previous count
            if ((MetricTypeEnum)activity.metric_type == MetricTypeEnum.Cumulative)
            {
                foreach (UserActivityDTO user_activity in user_activities)
                {
                    int intCount = GetHistoricalCount(activity.id, user_activity.user_id, date).GetValueOrDefault();
                    user_activity.count -= intCount;
                }
            }

            //// if this is a once type metric, need to zero out the count -- this logic doesnt work...
            //if ((MetricTypeEnum)activity.metric_type == MetricTypeEnum.Once)
            //{
            //    foreach (UserActivityDTO user_activity in user_activities)
            //    {
            //        int intCount = GetHistoricalCount(activity.id, user_activity.user_id, date);
            //        if (intCount > 0)
            //        {
            //            user_activity.count = 0;
            //        };
            //    }
            //}
            return user_activities;
        }

        [NonAction]
        private Nullable<int> GetHistoricalCount(int activity_id, int user_id, DateTime date)
        {
            var user_activities = dc.CommunityMetrics_UserActivities.Where(i => i.activity_id == activity_id && i.user_id == user_id && i.date < date).ToList();
            var count = user_activities.Sum(i => i.count);
            return count;
        }

        [NonAction]
        public void SaveUserActivity(UserActivityDTO dto)
        {
            // prevents saving of duplicate activity
            CommunityMetrics_UserActivity user_activity = dc.CommunityMetrics_UserActivities.Where(i => i.activity_id == dto.activity_id && i.user_id == dto.user_id && i.date == dto.date).SingleOrDefault();
            if (user_activity == null)
            {
                user_activity = new CommunityMetrics_UserActivity()
                {
                    activity_id = dto.activity_id,
                    user_id = dto.user_id
                };
                dc.CommunityMetrics_UserActivities.InsertOnSubmit(user_activity);
            }

            user_activity.count = dto.count;
            user_activity.notes = dto.notes;
            user_activity.date = dto.date;
            user_activity.created_on_date = DateTime.Now;

            dc.SubmitChanges();
        }
    }
}