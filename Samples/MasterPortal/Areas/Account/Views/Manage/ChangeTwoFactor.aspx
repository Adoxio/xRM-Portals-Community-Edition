<%@ Page Title="" Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ChangeTwoFactor/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="form-horizontal">
		<fieldset>
			<legend><%: Html.TextSnippet("Account/ChangeTwoFactor/ChangeTwoFactorFormHeading", defaultValue: ResourceManager.GetString("Change_Two_Factor_Authentication"), tagName: "span") %></legend>
			<%  if (ViewBag.HasEmail && !ViewBag.IsEmailConfirmed) { %>
				<div class="alert alert-warning clearfix">
					<a class="btn btn-warning btn-xs pull-right" href="<%: Url.Action("ConfirmEmailRequest", "Manage") %>" title="<%: Html.SnippetLiteral("Account/ChangeTwoFactor/ConfirmEmailButtonText", ResourceManager.GetString("Confirm_Email_Defaulttext")) %>">
						<span class="fa fa-envelope-o" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangeTwoFactor/ConfirmEmailButtonText", ResourceManager.GetString("Confirm_Email_Defaulttext")) %>
					</a>
					<span class="fa fa-exclamation-circle" aria-hidden="true"></span> <%: Html.TextSnippet("Account/ChangeTwoFactor/ConfirmEmailInstructionsText", defaultValue: ResourceManager.GetString("Confirmed_Email_Required_Message"), tagName: "span") %>
				</div>
			<% } else if (!ViewBag.HasEmail) { %>
				<div class="alert alert-warning clearfix">
					<a class="btn btn-warning btn-xs pull-right" href="<%: Url.Action("ChangeEmail", "Manage") %>" title="<%: Html.SnippetLiteral("Account/ChangeTwoFactor/ChangeEmailButtonText", ResourceManager.GetString("Change_Email_Defaulttext")) %>">
						<%: Html.TextSnippet("Account/ChangeTwoFactor/ChangeEmailButtonText", defaultValue: ResourceManager.GetString("Change_Email_Defaulttext")) %>
					</a>
					<span class="fa fa-exclamation-circle" aria-hidden="true"></span> <%: Html.HtmlSnippet("Account/ChangeTwoFactor/ChangeEmailInstructionsText", defaultValue: ResourceManager.GetString("Confirmed_Email_Required_Message")) %>
				</div>
			<% } %>
			<% if (ViewBag.IsTwoFactorEnabled) { %>
				<div class="alert alert-info clearfix">
					<% using (Html.BeginForm("DisableTFA", "Manage")) { %>
						<%: Html.AntiForgeryToken() %>
						<button class="btn btn-primary btn-xs pull-right"><span class="fa fa-times-circle" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangeTwoFactor/DisableTwoFactorButtonText", ResourceManager.GetString("Disable")) %></button>
					<% } %>
					<%  if (ViewBag.HasEmail && ViewBag.IsEmailConfirmed) { %>
						<%: Html.TextSnippet("Account/ChangeTwoFactor/TwoFactorEnabledText", defaultValue: ResourceManager.GetString("Twofactor_Authentication_Currently_Enabled"), tagName: "span") %>
					<% } else { %>
						<%: Html.TextSnippet("Account/ChangeTwoFactor/TwoFactorWaitingText", defaultValue: ResourceManager.GetString("Twofactor_Authentication_Waiting_For_Email_Confirmation"), tagName: "span") %>
					<% } %>
				</div>
				<% if (ViewBag.TwoFactorBrowserRemembered) { %>
					<div class="alert alert-warning clearfix">
						<% using (Html.BeginForm("ForgetBrowser", "Manage")) { %>
							<%: Html.AntiForgeryToken() %>
							<button class="btn btn-warning btn-xs pull-right"><span class="fa fa-trash-o" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangeTwoFactor/ForgetBrowserButtonText", ResourceManager.GetString("Forget_Browser")) %></button>
						<% } %>
						<%: Html.TextSnippet("Account/ChangeTwoFactor/TwoFactorBrowserRememberedText", defaultValue: ResourceManager.GetString("Twofactor_Authentication_Session_Remembered_For_This_Browser"), tagName: "span") %>
					</div>
				<% } %>
			<% } else if (ViewBag.HasEmail && ViewBag.IsEmailConfirmed) { %>
				<div class="alert alert-warning clearfix">
					<% using (Html.BeginForm("EnableTFA", "Manage")) { %>
						<%: Html.AntiForgeryToken() %>
						<button class="btn btn-warning btn-xs pull-right"><span class="fa fa-check-circle" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangeTwoFactor/EnableTwoFactorButtonText", ResourceManager.GetString("Enable")) %></button>
					<% } %>
					<span class="fa fa-exclamation-circle" aria-hidden="true"></span> <%: Html.TextSnippet("Account/ChangeTwoFactor/TwoFactorDisabledText", defaultValue: ResourceManager.GetString("Twofactor_Authentication_Currently_Disabled"), tagName: "span") %>
				</div>
			<% } %>
		</fieldset>
	</div>
</asp:Content>
