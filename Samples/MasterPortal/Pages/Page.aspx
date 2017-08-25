<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="True" CodeBehind="Page.aspx.cs" Inherits="Site.Pages.Page" ValidateRequest="false" EnableEventValidation="false" %>
<%@ Register src="~/Controls/Comments.ascx" tagname="Comments" tagprefix="site" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Core.Flighting" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="page-metadata clearfix">
		<adx:Snippet SnippetName="Social Share Widget Code Page Bottom" EditType="text" HtmlTag="Div" DefaultText="" runat="server"/>
	</div>
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback)) 
	{ %>
		<site:Comments ViewStateMode="Enabled" EnableRatings="False" runat="server" />
	<% } %>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarBottom" runat="server">
	<%: Html.Rating(Entity != null ? Entity.ToEntityReference() : null, panel: true) %>
	<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Sidebar", __portalScopeId__ = Website.Id}); %>
	<% Html.RenderAction("AdPlacement", "Ad", new { Area = "Cms", id = "Sidebar Bottom", __portalScopeId__ = Website.Id }); %>
</asp:Content>