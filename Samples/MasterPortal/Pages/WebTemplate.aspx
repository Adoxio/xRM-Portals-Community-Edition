<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="true" CodeBehind="WebTemplate.aspx.cs" Inherits="Site.Pages.WebTemplate" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="ContentContainer" runat="server">
	<adx:LiquidServerControl runat="server" ID="Liquid" />
</asp:Content>
