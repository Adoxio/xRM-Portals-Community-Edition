<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ShoppingCart.aspx.cs" Inherits="Site.Areas.Commerce.Pages.ShoppingCartPage" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Register TagPrefix="site" TagName="ShoppingCart" Src="~/Areas/Commerce/Controls/ShoppingCart.ascx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Commerce/css/commerce.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/Commerce/js/commerce.js") %>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<site:ShoppingCart ID="EditableShoppingCart" runat="server" />
</asp:Content>
