<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Home.facebook.aspx.cs" Inherits="Site.Areas.Community.Pages.Home_facebook" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register src="~/Areas/HelpDesk/Controls/CaseDeflection.ascx" tagname="CaseDeflection" tagprefix="adx" %>
<%@ Register tagPrefix="site" tagName="BlogsPanel" src="~/Controls/BlogsPanel.ascx" %>
<%@ Register tagPrefix="site" tagName="ForumsPanel" src="~/Controls/ForumsPanel.ascx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/HelpDesk/css/helpdesk.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server" />

<asp:Content ContentPlaceHolderID="MainContent" runat="server" ViewStateMode="Enabled">
	<%: Html.HtmlSnippet("Facebook/Home/Content") %>
	<adx:CaseDeflection ID="CaseDeflection" runat="server"/>
	<div class="row">
		<div class="col-md-8">
			<site:BlogsPanel runat="server" />
			<site:ForumsPanel runat="server" />
		</div>
		<div class="col-md-4">
			<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Home", __portalScopeId__ = Website.Id}); %>
		</div>
	</div>
</asp:Content>