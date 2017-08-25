<%@ Page Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.SetPasswordViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/SetPassword/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="form-horizontal">
		<fieldset>
			<legend><%: Html.TextSnippet("Account/SetPassword/SetPasswordFormHeading", defaultValue: ResourceManager.GetString("Set_Password_Defaulttext"), tagName: "span") %></legend>
			<% if (ViewBag.Settings.LocalLoginByEmail && !ViewBag.HasEmail) { %>
				<div class="alert alert-warning clearfix">
					<a class="btn btn-warning btn-xs pull-right" href="<%: Url.Action("ChangeEmail", "Manage") %>" title="<%: Html.SnippetLiteral("Account/SetPassword/ChangeEmailButtonText", ResourceManager.GetString("Set_Email_Defaulttext")) %>">
						<%: Html.TextSnippet("Account/SetPassword/ChangeEmailButtonText", defaultValue: ResourceManager.GetString("Set_Email_Defaulttext"), tagName: "span") %>
					</a>
					<span class="fa fa-exclamation-circle" aria-hidden="true"></span> <%: Html.HtmlSnippet("Account/SetPassword/ChangeEmailInstructionsText", defaultValue: ResourceManager.GetString("An_Email_Required_To_Set_Local_Account_Password")) %>
				</div>
			<% } else { %>
				<% using (Html.BeginForm("SetPassword", "Manage", new { area = "Account" })) { %>
					<%: Html.AntiForgeryToken() %>
					<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
					<% if (ViewBag.Settings.LocalLoginByEmail) { %>
						<div class="form-group">
							<label class="col-sm-4 control-label" for="Email"><%: Html.TextSnippet("Account/SetPassword/Email", defaultValue: ResourceManager.GetString("Email_DefaultText"), tagName: "span") %></label>
							<div class="col-sm-8">
								<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control", @readonly = "readonly" }) %>
							</div>
						</div>
					<% } else { %>
						<div class="form-group">
							<label class="col-sm-4 control-label" for="Username"><%: Html.TextSnippet("Account/SetPassword/Username", defaultValue: ResourceManager.GetString("UserName_Exception"), tagName: "span") %></label>
							<div class="col-sm-8">
								<%: Html.TextBoxFor(model => model.Username, new { @class = "form-control" }) %>
							</div>
						</div>
					<% } %>
					<div class="form-group">
						<label class="col-sm-4 control-label" for="NewPassword"><%: Html.TextSnippet("Account/SetPassword/NewPassword", defaultValue: ResourceManager.GetString("New_Password_DefaultText"), tagName: "span") %></label>
						<div class="col-sm-8">
							<%: Html.PasswordFor(model => model.NewPassword, new { @class = "form-control" }) %>
						</div>
					</div>
					<div class="form-group">
						<label class="col-sm-4 control-label" for="ConfirmPassword"><%: Html.TextSnippet("Account/SetPassword/ConfirmPassword", defaultValue: ResourceManager.GetString("Confirm_Password"), tagName: "span") %></label>
						<div class="col-sm-8">
							<%: Html.PasswordFor(model => model.ConfirmPassword, new { @class = "form-control" }) %>
						</div>
					</div>
					<div class="form-group">
						<div class="col-sm-offset-4 col-sm-8">
							<button class="btn btn-primary"><%: Html.SnippetLiteral("Account/SetPassword/SetPasswordButtonText", ResourceManager.GetString("Set_Password_Defaulttext")) %></button>
						</div>
					</div>
				<% } %>
			<% } %>
		</fieldset>
	</div>
</asp:Content>
