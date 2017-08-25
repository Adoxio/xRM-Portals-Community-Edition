<%@ Page Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="true" CodeBehind="MyServiceRequests.aspx.cs" Inherits="Site.Areas.Service311.Pages.MyServiceRequests" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<crm:CrmHyperLink runat="server" SiteMarkerName="Service Requests" CssClass="btn btn-primary">
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="311 New Service Request Button Label" Literal="True" DefaultText="<%$ ResourceManager:Create_New_Service_Request_DefaultText %>" />
			</crm:CrmHyperLink>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>