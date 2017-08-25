<%@ Page Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ChangePasswordViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ChangePassword/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
  <%: Html.TextSnippet("Profile/SecurityNav/ChangePassword", defaultValue: ResourceManager.GetString("Change_Password_DefaultText"), tagName: "span") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
    <% Html.RenderPartial("ProfileBreadcrumbs"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ChangePassword", "Manage", new { area = "Account" })) { %>
		<%: Html.AntiForgeryToken() %>
		<% var requiredFieldError = ResourceManager.GetString("Editable_Required_Label"); %>
		<% var oldPassword = ResourceManager.GetString("Old_Password_Defaulttext"); %>
		<% var newPassword = ResourceManager.GetString("New_Password_DefaultText"); %>
		<% var confirmPassword = ResourceManager.GetString("Confirm_Password"); %>
		<div class="form-horizontal">
			<fieldset>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger", @tabindex="0", @id="login-validation-summary" }) %>
				<% if (ViewBag.Settings.LocalLoginByEmail) { %>
					<div class="form-group">
						<label class="col-sm-4 control-label" for="Email"><%: Html.TextSnippet("Account/ChangePassword/Email", defaultValue: ResourceManager.GetString("Email_DefaultText"), tagName: "span") %></label>
						<div class="col-sm-8">
							<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control", @readonly = "readonly" }) %>
						</div>
					</div>
				<% } else { %>
					<div class="form-group">
						<label class="col-sm-4 control-label" for="Username"><%: Html.TextSnippet("Account/ChangePassword/Username", defaultValue: ResourceManager.GetString("UserName_Exception"), tagName: "span") %></label>
						<div class="col-sm-8">
							<%: Html.TextBoxFor(model => model.Username, new { @class = "form-control", @readonly = "readonly" }) %>
						</div>
					</div>
				<% } %>
				<div class="form-group">
					<label class="col-sm-4 control-label required" for="OldPassword"><%: Html.TextSnippet("Account/ChangePassword/OldPassword", defaultValue: ResourceManager.GetString("Old_Password_Defaulttext"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.PasswordFor(model => model.OldPassword, new { @class = "form-control", autocomplete = "off", aria_label = oldPassword + requiredFieldError }) %>
					</div>
				</div>
				<div class="form-group">
					<label class="col-sm-4 control-label required" for="NewPassword"><%: Html.TextSnippet("Account/ChangePassword/NewPassword", defaultValue: ResourceManager.GetString("New_Password_DefaultText"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.PasswordFor(model => model.NewPassword, new { @class = "form-control", autocomplete = "off", aria_label = newPassword + requiredFieldError }) %>
					</div>
				</div>
				<div class="form-group">
					<label class="col-sm-4 control-label required" for="ConfirmPassword"><%: Html.TextSnippet("Account/ChangePassword/ConfirmPassword", defaultValue: ResourceManager.GetString("Confirm_Password"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.PasswordFor(model => model.ConfirmPassword, new { @class = "form-control", autocomplete = "off", aria_label = confirmPassword + requiredFieldError }) %>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-4 col-sm-8">
						<button class="btn btn-primary"><%: Html.SnippetLiteral("Account/ChangePassword/ChangePasswordButtonText", ResourceManager.GetString("Change_Password_DefaultText")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
<script>
	$(document).ready(function () {
		if ($('#login-validation-summary').length) {
			if ($('#login-validation-summary').find('li').length > 0)
			{
				var valMesssages = $('#login-validation-summary').find('li');
				var ariaLabel;
				ariaLabel = "";
				for (i = 0; i < valMesssages.length; i++) {
					ariaLabel = ariaLabel + valMesssages[i].innerHTML;
				}
				$('#login-validation-summary').attr("aria-label", ariaLabel);
				$('#login-validation-summary').focus();
			}
		}
	});
</script>
</asp:Content>
