<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Admin.ascx.cs" Inherits="Dnn.CommunityMetrics.Admin" %>

<div ng-app="DNN_CommunityMetrics" ng-cloak>
    <div admin></div>
</div>

<script>
    var module_id = <%= ModuleId %>;
    var user_id = <%= UserId %>;
    var sf = $.ServicesFramework(module_id);
</script>

