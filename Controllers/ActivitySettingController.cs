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
    [SupportedModules("Dnn.CommunityMetrics")]
    [ValidateAntiForgeryToken]
    public class ActivitySettingController : DnnApiController
    {
        DataContext dc = new DataContext();

        [NonAction]
        public ActivitySettingDTO ConvertItemToDto(CommunityMetrics_ActivitySetting item)
        {
            ActivitySettingDTO dto = new ActivitySettingDTO();

            dto.id = item.id;
            dto.activity_id = item.activity_id;
            dto.name = item.name;
            dto.value = item.value;

            return dto;
        }
        [NonAction]
        public CommunityMetrics_ActivitySetting ConvertDtoToItem(CommunityMetrics_ActivitySetting item, ActivitySettingDTO dto)
        {
            if (item == null)
            {
                item = new CommunityMetrics_ActivitySetting();
            }

            if (dto == null)
            {
                return item;
            }

            item.id = dto.id.GetValueOrDefault();
            item.activity_id = dto.activity_id.GetValueOrDefault();
            item.name = dto.name;
            item.value = dto.value;

            return item;
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [AllowAnonymous]
        public HttpResponseMessage Get(string typeName, Nullable<int> activity_id)
        {
            IActivity objIActivity = (IActivity)Activator.CreateInstance(BuildManager.GetType(typeName, true));

            List<ActivitySettingDTO> settings = objIActivity.GetSettings();

            if (activity_id.HasValue) // load the setting values if they are present for activity
            {
                foreach (ActivitySettingDTO setting in settings)
                {
                    setting.activity_id = activity_id;

                    var setting_instance = dc.CommunityMetrics_ActivitySettings.Where(i => i.activity_id == activity_id && i.name == setting.name).SingleOrDefault();
                    if (setting_instance != null)
                    {
                        setting.id = setting_instance.id;

                        setting.value = setting_instance.value;
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, settings);
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [AllowAnonymous]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                CommunityMetrics_ActivitySetting item = dc.CommunityMetrics_ActivitySettings.Where(i => i.id == id).SingleOrDefault();

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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Post(ActivitySettingDTO dto)
        {
            try
            {
                CommunityMetrics_ActivitySetting item = ConvertDtoToItem(null, dto);

                dc.CommunityMetrics_ActivitySettings.InsertOnSubmit(item);
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Put(ActivitySettingDTO dto)
        {
            try
            {
                CommunityMetrics_ActivitySetting item = dc.CommunityMetrics_ActivitySettings.Where(i => i.id == dto.id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                item = ConvertDtoToItem(item, dto);

                dc.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, ConvertItemToDto(item));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpDelete]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                CommunityMetrics_ActivitySetting item = dc.CommunityMetrics_ActivitySettings.Where(i => i.id == id).SingleOrDefault();

                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                dc.CommunityMetrics_ActivitySettings.DeleteOnSubmit(item);
                dc.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}