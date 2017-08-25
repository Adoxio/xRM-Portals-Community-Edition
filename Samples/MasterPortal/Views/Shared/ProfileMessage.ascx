<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Import Namespace="Site.Areas.Account.Controllers" %>

<% var alerts = new[] {
	new { Id = ManageController.ManageMessageId.SetPasswordSuccess, SnippetName = "Profile/Message/SetPasswordSuccess", DefaultText = ResourceManager.GetString("Password_Set_Successfully"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.ChangePasswordSuccess, SnippetName = "Profile/Message/ChangePasswordSuccess", DefaultText = ResourceManager.GetString("Password_Changed_Successfully"), Type = "success", Icon = "fa-check-circle" },

	new { Id = ManageController.ManageMessageId.ChangeEmailSuccess, SnippetName = "Profile/Message/ChangeEmailSuccess", DefaultText = ResourceManager.GetString("Email_Changed_Successfully"), Type = "success", Icon = "fa-check-circle" },

	new { Id = ManageController.ManageMessageId.ConfirmEmailSuccess, SnippetName = "Profile/Message/ConfirmEmailSuccess", DefaultText = ResourceManager.GetString("Email_Confirmed_Successfully"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.ConfirmEmailFailure, SnippetName = "Profile/Message/ConfirmEmailFailure", DefaultText = ResourceManager.GetString("Confirm_Email_Failed"), Type = "danger", Icon = "fa-times-circle" },

	new { Id = ManageController.ManageMessageId.ChangePhoneNumberSuccess, SnippetName = "Profile/Message/ChangePhoneNumberSuccess", DefaultText = ResourceManager.GetString("Mobile_Phone_Number_Changed_Successfully"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.ChangePhoneNumberFailure, SnippetName = "Profile/Message/ChangePhoneNumberFailure", DefaultText = ResourceManager.GetString("Change_Mobile_Phone_Number_Failed"), Type = "danger", Icon = "fa-times-circle" },
	new { Id = ManageController.ManageMessageId.RemovePhoneNumberSuccess, SnippetName = "Profile/Message/RemovePhoneNumberSuccess", DefaultText = ResourceManager.GetString("Mobile_Phone_Number_Removed_Successfully"), Type = "success", Icon = "fa-check-circle" },

	new { Id = ManageController.ManageMessageId.ForgetBrowserSuccess, SnippetName = "Profile/Message/ForgetBrowserSuccess", DefaultText = ResourceManager.GetString("Tow_Factor_signIn_Required_For_Browser"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.RememberBrowserSuccess, SnippetName = "Profile/Message/RememberBrowserSuccess", DefaultText = ResourceManager.GetString("Remember_Browser_Success_Message"), Type = "success", Icon = "fa-check-circle" },

	new { Id = ManageController.ManageMessageId.DisableTwoFactorSuccess, SnippetName = "Profile/Message/DisableTwoFactorSuccess", DefaultText = ResourceManager.GetString("Twofactor_Authentication_Disabled_Successfully_Message"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.EnableTwoFactorSuccess, SnippetName = "Profile/Message/EnableTwoFactorSuccess", DefaultText = ResourceManager.GetString("Twofactor_Authentication_Enabled_Successfully_Message"), Type = "success", Icon = "fa-check-circle" },
	
	new { Id = ManageController.ManageMessageId.RemoveLoginSuccess, SnippetName = "Profile/Message/RemoveLoginSuccessText", DefaultText = ResourceManager.GetString("External_Account_Removed_sucessfully"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.RemoveLoginFailure, SnippetName = "Profile/Message/RemoveLoginFailureText", DefaultText = ResourceManager.GetString("Remove_External_Account_Failed"), Type = "danger", Icon = "fa-times-circle" },
	new { Id = ManageController.ManageMessageId.LinkLoginSuccess, SnippetName = "Profile/Message/LinkLoginSuccessText", DefaultText = ResourceManager.GetString("External_Account_Added_sucessfully"), Type = "success", Icon = "fa-check-circle" },
	new { Id = ManageController.ManageMessageId.LinkLoginFailure, SnippetName = "Profile/Message/LinkLoginFailureText", DefaultText = ResourceManager.GetString("Addition_Of_External_Account_Failed"), Type = "danger", Icon = "fa-times-circle" },
}; %>
<% if (!string.IsNullOrWhiteSpace(Model)) {
		foreach (var alert in alerts) {
			if (Model == alert.Id.ToString()) { %>
				<div class="alert alert-<%: alert.Type %>">
					<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
					<span class="fa <%: alert.Icon %>" aria-hidden="true"></span> <%: Html.TextSnippet(alert.SnippetName, defaultValue: alert.DefaultText, tagName: "span") %>
				</div>
			<% }
		}
} %>