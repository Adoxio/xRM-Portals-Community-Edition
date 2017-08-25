<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ResetPasswordConfirmation/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="form-horizontal">
		<fieldset>
			<legend><%: Html.TextSnippet("Account/PasswordReset/ResetPasswordFormHeading", defaultValue: ResourceManager.GetString("Reset_Password_Defaulttext"), tagName: "span") %></legend>
			<div class="alert alert-success">
				<%: Html.HtmlSnippet("Account/PasswordReset/ResetPasswordSuccessText", defaultValue: ResourceManager.GetString("Your_Password_Has_Been_Reset")) %>
			</div>
			<% Html.RenderPartial("SignInLink", new ViewDataDictionary(ViewData) { { "class", "btn btn-primary" }, { "ReturnUrl", string.Empty } }); %>
		</fieldset>
	</div>
</asp:Content>
