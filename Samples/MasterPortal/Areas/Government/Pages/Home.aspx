<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="True" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Government.Pages.Home" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Register tagPrefix="site" tagName="NewsPanel" src="~/Controls/NewsPanel.ascx" %>

<asp:Content ID="Head" ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content ID="Scripts" ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/Service311/js/service311.js") %>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentPage" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<adx:Property DataSourceID="CurrentPage" PropertyName="adx_copy" EditType="html" CssClass="page-copy" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ViewStateMode="Enabled" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="row">
		<div class="col-md-8">
			<site:NewsPanel runat="server"/>
		</div>
		<div class="col-md-4">
			<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Home", __portalScopeId__ = Website.Id}); %>
			<asp:Panel ID="TwitterFeedPanel" runat="server">
				<adx:Snippet SnippetName="Home Twitter Widget" EditType="text" Literal="True" runat="server"/>
			</asp:Panel>
		</div>
	</div>
</asp:Content>