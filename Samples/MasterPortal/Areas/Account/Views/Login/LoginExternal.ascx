<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<Microsoft.Owin.Security.AuthenticationDescription>>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<% using (Html.BeginForm("ExternalLogin", "Login", new { ReturnUrl = ViewBag.ReturnUrl, InvitationCode = ViewBag.InvitationCode })) { %>
<%: Html.AntiForgeryToken() %>
<div class="form-horizontal">
	<fieldset>
		<legend>
			<%: Html.TextSnippet("Account/SignIn/SignInExternalFormHeading", defaultValue: ResourceManager.GetString("Signin_With_External_Account"), tagName: "span") %>
		</legend>
		<div class="messages">
			<% if (ViewBag.ExternalRegistrationFailure ?? false) { %>
				<%: Html.HtmlSnippet("Account/Register/RegistrationDisabledMessage", defaultValue: ResourceManager.GetString("Account_Register_Registration_Disabled_Message")) %>
			<% } %>
		</div>
		<% Html.RenderPartial("LoginProvider", Model); %>
	</fieldset>
</div>
<% } %>
