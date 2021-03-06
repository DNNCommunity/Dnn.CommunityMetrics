﻿using DotNetNuke.Services.Exceptions;
using System;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Modules;

namespace Dnn.CommunityMetrics
{
    partial class Dashboard : ModuleBase, IActionable
    {
        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection Actions = new ModuleActionCollection();
                if (IsEditable)
                {
                    Actions.Add(GetNextActionID(), "Admin", ModuleActionType.AddContent, "", "", EditUrl("Admin"), false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                }
                return Actions;
            }
        }

        protected new void Page_Load(Object sender, EventArgs e)
        {
            try
            {
                base.Page_Load(sender, e);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

    }
}
