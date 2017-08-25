<%@ Page Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ForgotPasswordConfirmation/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="form-horizontal">
		<fieldset>
			<legend><%: Html.TextSnippet("Account/PasswordReset/ForgotPasswordFormHeading", defaultValue: ResourceManager.GetString("Form_Heading_Forgot_Password")) %></legend>
			<div class="alert alert-info">
				<%: Html.HtmlSnippet("Account/PasswordReset/ForgotPasswordConfirmationSuccessText", defaultValue: ResourceManager.GetString("Please_Check_Email_To_Reset_Your_Password")) %>
			</div>
		</fieldset>
	</div>
	<% if (ViewBag.Settings.IsDemoMode && ViewBag.DemoModeLink != null) { %>
		<div class="panel panel-warning">
			<div class="panel-heading">
				<h3 class="panel-title"><span class="fa fa-wrench" aria-hidden="true"></span> DEMO MODE <small>LOCAL ONLY</small></h3>
			</div>
			<div class="panel-body">
				<a class="btn btn-default" href="<%: ViewBag.DemoModeLink %>" title="<%: Html.SnippetLiteral("Account/SignIn/ForgotPasswordConfirmationButtonText", ResourceManager.GetString("Reset_Password_Defaulttext")) %>"><%: Html.SnippetLiteral("Account/SignIn/ForgotPasswordConfirmationButtonText", ResourceManager.GetString("Reset_Password_Defaulttext")) %></a>
			</div>
		</div>
	<% } %>
</asp:Content>
