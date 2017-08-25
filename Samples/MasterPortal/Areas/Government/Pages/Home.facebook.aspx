<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Home.facebook.aspx.cs" Inherits="Site.Areas.Government.Pages.Home_facebook" %>
<%@ OutputCache CacheProfile="User" %>
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
	<site:NewsPanel runat="server"/>
</asp:Content>
