<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="WebTemplateNoValidation.aspx.cs" Inherits="Site.Pages.WebTemplateNoValidation" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="ContentContainer" runat="server">
	<adx:LiquidServerControl runat="server" ID="Liquid" />
</asp:Content>