<%@ Page Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="true" CodeBehind="MyPermitApplications.aspx.cs" Inherits="Site.Areas.Permits.Pages.MyPermitApplications" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Permits/css/permits.css") %>" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<crm:CrmHyperLink runat="server" SiteMarkerName="Permits" CssClass="btn btn-primary">
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="New Permit Application Button Label" Literal="True" DefaultText="<%$ ResourceManager:Apply_For_Permit_Or_License_DefaultText %>" />
			</crm:CrmHyperLink>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>