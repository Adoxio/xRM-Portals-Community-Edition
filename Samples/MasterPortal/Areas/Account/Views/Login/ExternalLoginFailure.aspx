<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ExternalLoginFailure/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="alert alert-danger">
		<%: Html.HtmlSnippet("Account/SignIn/ExternalLoginFailureText", defaultValue: ResourceManager.GetString("Unable_To_Authenticate_External_Account_Provider")) %>
	</div>
	<% Html.RenderPartial("SignInLink", new ViewDataDictionary(ViewData) { { "class", "btn btn-primary" } }); %>
</asp:Content>
