using DotNetNuke.Web.Api;
using System.Web.Http;

namespace Dnn.CommunityMetrics
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            // Default
            mapRouteManager.MapHttpRoute(
                moduleFolderName: "Dnn.CommunityMetrics",
                routeName: "default",
                url: "{controller}/{id}",
                defaults: new
                {
                    id = RouteParameter.Optional
                },
                namespaces: new[] { "Dnn.CommunityMetrics" });
        }
    }
}