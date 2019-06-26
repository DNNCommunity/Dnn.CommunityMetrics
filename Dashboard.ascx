<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="Dnn.CommunityMetrics.Dashboard" %>

<div ng-app="DNN_CommunityMetrics" ng-cloak>
    <div dashboard></div>
</div>

<script>
    var module_id = <%= ModuleId %>;
    var user_id = <%= UserId %>;
    var sf = $.ServicesFramework(module_id);
</script>