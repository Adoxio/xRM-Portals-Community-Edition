<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Default.master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<asp:Content ID="Title" ContentPlaceHolderID="Title" runat="server" ><%= (string) ViewBag.Title  %></asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
	<% Html.RenderPartial((string) ViewBag._ViewName, ViewData); %>
</asp:Content>
