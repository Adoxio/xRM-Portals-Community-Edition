<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Site.Areas.Account.ViewModels.LoginViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<% var userName = ResourceManager.GetString("UserName_Exception"); %>
<% var password = ResourceManager.GetString("Password_Defaulttext"); %>
<% var requiredFieldError = ResourceManager.GetString("Required_Field_Error"); %>

<% using (Html.BeginForm("Login", "Login", new { area = "Account", ReturnUrl = ViewBag.ReturnUrl, InvitationCode = ViewBag.InvitationCode })) { %>
	<%: Html.AntiForgeryToken() %>
	<div class="form-horizontal">
		<fieldset>
			<legend>
				<%: Html.TextSnippet("Account/SignIn/SignInLocalFormHeading", defaultValue: ResourceManager.GetString("Signin_With_Local_Account"), tagName: "span") %>
			</legend>
		<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger",@tabindex="0",@id="loginValidationSummary"}) %>
			<% if (ViewBag.Settings.LocalLoginByEmail) { %>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="Email"><%: Html.TextSnippet("Account/SignIn/EmailLabel", defaultValue: ResourceManager.GetString("Email_DefaultText"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control" }) %>
					</div>
				</div>
			<% } else { %>
				<div class="form-group">
					<label class="col-sm-4 control-label required" for="Username"><%: Html.TextSnippet("Account/SignIn/UsernameLabel", defaultValue: userName, tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.TextBoxFor(model => model.Username, new { @class = "form-control", aria_label = string.Format(requiredFieldError, userName) }) %>
					</div>
				</div>
			<% } %>
			<div class="form-group">
				<label class="col-sm-4 control-label required" for="Password"><%: Html.TextSnippet("Account/SignIn/PasswordLabel", defaultValue: password, tagName: "span") %></label>
				<div class="col-sm-8">
					<%: Html.PasswordFor(model => model.Password, new { @class = "form-control", autocomplete = "off", aria_label = string.Format(requiredFieldError, password) }) %>
				</div>
			</div>
			<% if (ViewBag.Settings.RememberMeEnabled) { %>
				<div class="form-group">
					<div class="col-sm-offset-4 col-sm-8">
						<div class="checkbox">
							<label>
								<%: Html.CheckBoxFor(model => model.RememberMe) %>
								<%: Html.TextSnippet("Account/SignIn/RememberMeLabel", defaultValue: ResourceManager.GetString("Remember_Me_Defaulttext"), tagName: "span") %>
							</label>
						</div>
					</div>
				</div>
			<% } %>
			<div class="form-group">
				<div class="col-sm-offset-4 col-sm-8">
					<button id="submit-signin-local" class="btn btn-primary" title="<%: Html.SnippetLiteral("Account/SignIn/SignInLocalButtonText", ResourceManager.GetString("Sign_In")) %>"><%: Html.SnippetLiteral("Account/SignIn/SignInLocalButtonText", ResourceManager.GetString("Sign_In")) %></button>
					<% if (ViewBag.Settings.ResetPasswordEnabled) { %>
						<a class="btn btn-default" role="button" href="<%: Url.Action("ForgotPassword") %>" title="<%: Html.SnippetLiteral("Account/SignIn/PasswordResetLabel", ResourceManager.GetString("Forgot_Your_Password")) %>"><%: Html.SnippetLiteral("Account/SignIn/PasswordResetLabel", ResourceManager.GetString("Forgot_Your_Password")) %></a>
					<% } %>
				</div>
			</div>
		</fieldset>
	</div>
<% } %>
<script type="text/javascript">
	$(function() {
		$("#submit-signin-local").click(function () {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		});
	});
</script>
