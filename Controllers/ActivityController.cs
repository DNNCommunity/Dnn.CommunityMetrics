using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using System;
using System.Collections;
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
    public class ActivityController : DnnApiController
    {
        DataContext dc = new DataContext();

        [NonAction]
        public ActivityDTO ConvertItemToDto(CommunityMetrics_Activity item)
        {
            ActivityDTO dto = new ActivityDTO();

            dto.id = item.id;
            dto.name = item.name;
            dto.description = item.description;
            dto.type_name = item.type_name;
            dto.factor = item.factor;
            dto.active = item.active;
            dto.metric_type = (MetricTypeEnum)item.metric_type;
            dto.user_filter = item.user_filter;
            dto.min_daily = item.min_daily;
            dto.max_daily = item.max_daily;

            Hashtable settings = new Hashtable();

            foreach (CommunityMetrics_ActivitySetting activity_setting in item.CommunityMetrics_ActivitySettings)
            {
                settings.Add(activity_setting.name, activity_setting.value);
            }
            dto.settings = settings;

            return dto;
        }
        [NonAction]
        public CommunityMetrics_Activity ConvertDtoToItem(CommunityMetrics_Activity item, ActivityDTO dto)
        {
            if (item == null)
            {
                item = new CommunityMetrics_Activity();
            }

            if (dto == null)
            {
                return item;
            }

            item.id = dto.id;
            item.name = dto.name;
            item.description = dto.description;
            item.type_name = dto.type_name;
            item.factor = dto.factor;
            item.active = dto.active;            
            item.min_daily = dto.min_daily;
            item.max_daily = dto.max_daily;

            return item;
        }


        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get()
        {
            try
            {
                List<ActivityDTO> dtos = GetActivities();
                return Request.CreateResponse(HttpStatusCode.OK, dtos);
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
                CommunityMetrics_Activity item = dc.CommunityMetrics_Activities.Where(i => i.id == id).SingleOrDefault();

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
        public HttpResponseMessage Post(ActivityDTO dto)
        {
            try
            {
                dto = SaveActivity(dto);

                return Request.CreateResponse(HttpStatusCode.OK, dto);
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
        public HttpResponseMessage Put(ActivityDTO dto)
        {
            try
            {
                dto = SaveActivity(dto);

                return Request.CreateResponse(HttpStatusCode.OK, dto);
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
                CommunityMetrics_Activity item = dc.CommunityMetrics_Activities.Where(i => i.id == id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                dc.CommunityMetrics_Activities.DeleteOnSubmit(item);
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
        public ActivityDTO SaveActivity(ActivityDTO dto)
        {
            CommunityMetrics_Activity activity = dc.CommunityMetrics_Activities.Where(i => i.id == dto.id).SingleOrDefault();

            if (activity == null)
            {
                activity = ConvertDtoToItem(null, dto);
                activity.created_by_user_id = UserController.Instance.GetCurrentUserInfo().UserID;
                activity.created_on_date = DateTime.Now;
                activity.user_filter = string.Empty;

                IActivity objIActivity = (IActivity)Activator.CreateInstance(BuildManager.GetType(activity.type_name, true));
                activity.metric_type = (int)objIActivity.MetricType;

                dc.CommunityMetrics_Activities.InsertOnSubmit(activity);
            }

            activity = ConvertDtoToItem(activity, dto);

            activity.last_modified_by_user_id = UserController.Instance.GetCurrentUserInfo().UserID;
            activity.last_modified_on_date = DateTime.Now;

            dc.SubmitChanges();

            return ConvertItemToDto(activity);
        }

        [NonAction]
        public List<ActivityDTO> GetActivities()
        {
            try
            {
                var query = dc.CommunityMetrics_Activities.AsQueryable();

                List<ActivityDTO> dtos = new List<ActivityDTO>();
                foreach (CommunityMetrics_Activity item in query)
                {
                    ActivityDTO dto = ConvertItemToDto(item);
                    dtos.Add(dto);
                }

                return dtos;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw ex;
            }
        }

    }
}