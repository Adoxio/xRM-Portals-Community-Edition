<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="True" CodeBehind="SiteMap.aspx.cs" Inherits="Site.Pages.SiteMap" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div>
		<asp:SiteMapDataSource ID="SiteMapDataSource" runat="server" />
		<asp:TreeView DataSourceID="SiteMapDataSource" runat="server" />
	</div>
</asp:Content>
