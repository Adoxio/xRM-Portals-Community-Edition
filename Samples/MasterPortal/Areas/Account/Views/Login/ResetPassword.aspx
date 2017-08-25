<%@ Page Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ResetPasswordViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ResetPassword/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ResetPassword", "Login")) { %>
		<%: Html.AntiForgeryToken() %>
		<%: Html.HiddenFor(model => model.UserId) %>
		<%: Html.HiddenFor(model => model.Code) %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/PasswordReset/ResetPasswordFormHeading", defaultValue: ResourceManager.GetString("Reset_Password_Defaulttext"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="Password"><%: Html.TextSnippet("Account/PasswordReset/PasswordLabel", defaultValue: ResourceManager.GetString("New_Password_DefaultText"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.PasswordFor(model => model.Password, new { @class = "form-control" }) %>
					</div>
				</div>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="ConfirmPassword"><%: Html.TextSnippet("Account/PasswordReset/ConfirmPasswordLabel", defaultValue: ResourceManager.GetString("New_Password_Confirmation_Message"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.PasswordFor(model => model.ConfirmPassword, new { @class = "form-control" }) %>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-4 col-sm-8">
						<button id="submit-reset-password" class="btn btn-primary"><%: Html.SnippetLiteral("Account/PasswordReset/ResetPasswordButtonText", ResourceManager.GetString("Reset")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
	<script type="text/javascript">
		$(function() {
			$("#submit-reset-password").click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
		});
	</script>
</asp:Content>
