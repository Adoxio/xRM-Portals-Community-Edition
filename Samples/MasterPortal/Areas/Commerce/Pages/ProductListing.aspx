<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/WebFormsContent.master" CodeBehind="ProductListing.aspx.cs" Inherits="Site.Areas.Commerce.Pages.ProductListing" %>
<%@ Register src="~/Controls/ChildNavigation.ascx" tagname="ChildNavigation" tagprefix="site" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Commerce/css/commerce.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/Commerce/js/commerce.js") %>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<site:ChildNavigation ShowShortcuts="False" ShowDescriptions="True" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<site:ChildNavigation ShowChildren="False" runat="server" />
</asp:Content>
