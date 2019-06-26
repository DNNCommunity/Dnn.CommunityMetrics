using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;

namespace Dnn.CommunityMetrics
{
    public class ModuleBase : PortalModuleBase
    {
        protected void Page_Load(Object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);

            ClientResourceManager.RegisterStyleSheet(this.Page, ResolveUrl("https://use.fontawesome.com/releases/v5.7.2/css/all.css"), 1);

            ClientResourceManager.RegisterStyleSheet(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/plugins/angular-toastr/angular-toastr.min.css"), 1);

            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular.min.js"), 2);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular-messages.min.js"), 3);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular-animate.min.js"), 3);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular-sanitize.min.js"), 4);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular-cookies.min.js"), 4);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("https://ajax.googleapis.com/ajax/libs/angularjs/1.7.8/angular-route.min.js"), 4);

            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/plugins/angular-toastr/angular-toastr.tpls.min.js"), 5);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/plugins/ui.bootstrap/ui-bootstrap-tpls-2.5.0.min.js"), 6);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/plugins/datetime-picker/datetime-picker.min.js"), 7);


            // app
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/app.js"), 7);

            // services            
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/services/activity.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/services/activity-setting.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/services/user-activity.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/services/activity-type.js"), 15);

            // directives
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/directives/dashboard.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/directives/admin.js"), 15);

            // controllers
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/dashboard.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/admin.js"), 15);


            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/activity/activity-delete.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/activity/activity-edit.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/activity/activity-list.js"), 15);

            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/user-activity/user-activity-delete.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/user-activity/user-activity-edit.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/user-activity/user-activity-list.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/user-activity/user-activity-detail.js"), 15);
            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/user-activity/user-activity-detail2.js"), 15);

            ClientResourceManager.RegisterScript(this.Page, ResolveUrl("/DesktopModules/Dnn.CommunityMetrics/app/controllers/activity-setting/activity-setting-edit.js"), 15);
        }
    }
}