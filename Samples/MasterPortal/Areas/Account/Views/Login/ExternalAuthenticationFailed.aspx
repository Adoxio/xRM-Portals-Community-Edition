<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ExternalLoginFailure/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="alert alert-danger">
	    <% if (ViewBag.AccessDeniedError ?? false) { %>
	        <%: Html.HtmlSnippet("Account/Register/ExternalAuthenticationFailed/AccessDenied", defaultValue: ResourceManager.GetString("External_Authentication_Failed_Access_Denied")) %>
	    <% } else { %>
	        <%: Html.HtmlSnippet("Account/Register/ExternalAuthenticationFailed", defaultValue: ResourceManager.GetString("External_Authentication_Failed")) %>
	    <% } %>
	</div>
	<% Html.RenderPartial("SignInLink", new ViewDataDictionary(ViewData) { { "class", "btn btn-primary" } }); %>
</asp:Content>