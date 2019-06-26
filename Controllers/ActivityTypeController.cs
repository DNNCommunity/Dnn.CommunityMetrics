using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace Dnn.CommunityMetrics
{
    //[SupportedModules("Dnn.CommunityMetrics")]
    public class ActivityTypeController : DnnApiController
    {
        DataContext dc = new DataContext();


        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get()
        {
            List<ActivityTypeDTO> activityTypes = new List<ActivityTypeDTO>();

            foreach (Assembly objAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] objLoadableTypes = null;
                try
                {
                    objLoadableTypes = objAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    objLoadableTypes = e.Types;
                }
                foreach (Type objType in objLoadableTypes.Where(t => t != null))
                {
                    if (!objType.IsInterface & typeof(IActivity).IsAssignableFrom(objType))
                    {
                        ActivityTypeDTO dto = new ActivityTypeDTO()
                        {
                            name = objType.Name,
                            full_name = objType.FullName
                        };
                        activityTypes.Add(dto);
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, activityTypes);
        }


    }
}