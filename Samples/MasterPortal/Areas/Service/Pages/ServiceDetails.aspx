<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/WebForms.master" CodeBehind="ServiceDetails.aspx.cs" Inherits="Site.Areas.Service.Pages.ServiceDetails" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="~/Areas/Service/css/service.css" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<crm:CrmHyperLink runat="server" SiteMarkerName="View Scheduled Services" CssClass="btn btn-default">
				<span class="fa fa-list" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="Services/ServiceDetails/ServicesLink" DefaultText="<%$ ResourceManager:View_Scheduled_Services %>" />
			</crm:CrmHyperLink>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ContentBottom">
	<div class="service-schedule">
		<div class="panel panel-default">
			<div class="panel-heading">
				<div class="panel-title">
					<span class="fa fa-wrench" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Services/ServiceDetails/Service" DefaultText="<%$ ResourceManager:Service_DefaultText %>" EditType="text" />
				</div>
			</div>
			<div class="panel-body">
				<asp:Label runat="server" ID="serviceType" />
			</div>
		</div>
		<div class="panel panel-default">
			<div class="panel-heading">
				<div class="panel-title">
					<span class="fa fa-clock-o" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Services/ServiceDetails/StartTime" DefaultText="<%$ ResourceManager:Start_Time_DefaultText %>" EditType="text" />
				</div>
			</div>
			<div class="panel-body">
				<asp:Label runat="server" ID="startTime" />
			</div>
		</div>
		<div class="panel panel-default">
			<div class="panel-heading">
				<div class="panel-title">
					<span class="fa fa-clock-o" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Services/ServiceDetails/EndTime" DefaultText="<%$ ResourceManager:End_Time_DefaultText %>" EditType="text" />
				</div>
			</div>
			<div class="panel-body">
				<asp:Label runat="server" ID="endTime" />
			</div>
		</div>
	</div>
</asp:Content>