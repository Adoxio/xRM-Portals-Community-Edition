<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ForgotPasswordViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ForgotPassword/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ForgotPassword", "Login")) { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/PasswordReset/ForgotPasswordFormHeading", defaultValue: ResourceManager.GetString("Form_Heading_Forgot_Password"), tagName: "span") %></legend>
                <%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger",@tabindex="-1" ,@id="loginValidationSummary"}) %>
                <div class="form-group">
					<label class="col-sm-2 control-label required" for="Email"><%: Html.TextSnippet("Account/PasswordReset/EmailLabel", defaultValue: ResourceManager.GetString("Email_DefaultText"), tagName: "span") %></label>
					<div class="col-sm-10">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control" }) %>
						<p class="help-block"><%: Html.TextSnippet("Account/PasswordReset/EmailInstructionsText", defaultValue: ResourceManager.GetString("Enter_Email_Address_For_Password_Reset"), tagName: "span") %></p>
					<% if (ViewBag.ErrorMessage != null)
					{%>
                        <%  string errorString;
                            if (ViewBag.ErrorMessage is string)
                            {
                                errorString = ViewBag.ErrorMessage as string;
                            }
                            else
                            {
                                errorString = ResourceManager.GetString("Generic_Error_Message");
                            }

                           %>
						<label class="alert alert-block alert-danger" for="GenericError"><%: Html.TextSnippet("Account/PasswordReset/GenericError", defaultValue: errorString, tagName: "span") %></label>
					<% } %>
                    </div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<button id="submit-forgot-password" class="btn btn-primary"><%: Html.SnippetLiteral("Account/PasswordReset/ForgotPasswordButtonText", ResourceManager.GetString("Send")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
	<script type="text/javascript">

		$(document).ready(function () {
			if ($('#loginValidationSummary').length) {
				if ($('#loginValidationSummary').find('li').length > 0) {
					$('#loginValidationSummary').attr("role", "alert");
					$('#loginValidationSummary').attr("aria-live", "assertive");
					$('#loginValidationSummary').focus();
				}
			}

		$(function() {
			$("#submit-forgot-password").click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
		});
		});
	</script>
</asp:Content>
