<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="Listing.aspx.cs" Inherits="Site.Pages.Listing" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Register src="~/Controls/ChildNavigation.ascx" tagname="ChildNavigation" tagprefix="site" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<site:ChildNavigation ShowShortcuts="False" ShowDescriptions="True" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<site:ChildNavigation ShowChildren="False" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarBottom" runat="server">
	<%: Html.Rating(Entity != null ? Entity.ToEntityReference() : null, panel: true) %>
	<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Sidebar", __portalScopeId__ = Website.Id}); %>
	<% Html.RenderAction("AdPlacement", "Ad", new { Area = "Cms", id = "Sidebar Bottom", __portalScopeId__ = Website.Id }); %>
</asp:Content>