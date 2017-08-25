<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="True" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Community.Pages.Home" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Register tagPrefix="site" tagName="BlogsPanel" src="~/Controls/BlogsPanel.ascx" %>
<%@ Register tagPrefix="site" tagName="EventsPanel" src="~/Controls/EventsPanel.ascx" %>
<%@ Register tagPrefix="site" tagName="ForumsPanel" src="~/Controls/ForumsPanel.ascx" %>
<%@ Register tagPrefix="site" tagName="NewsPanel" src="~/Controls/NewsPanel.ascx" %>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentPage" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<adx:Property DataSourceID="CurrentPage" PropertyName="adx_copy" EditType="html" CssClass="page-copy home-copy" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ViewStateMode="Enabled" runat="server">
	<div class="row">
		<div class="col-md-8">
			<site:NewsPanel runat="server" />
			<site:BlogsPanel runat="server" />
			<site:ForumsPanel runat="server" />
		</div>
		<div class="col-md-4">
			<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Home", __portalScopeId__ = Website.Id}); %>
			<site:EventsPanel runat="server" />
		</div>
	</div>
</asp:Content>