<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" MasterPageFile="Account.Master" CodeBehind="Registration.aspx.cs" Inherits="Site.Areas.Account.Views.Login.Registration" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="AccountNavBar" runat="server">
	<% Html.RenderPartial("~/Areas/Account/Views/Login/AccountNavBar.ascx", new ViewDataDictionary(Html.ViewData) { { "SubArea", "Register" } }); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<adx:Snippet ID="PageCopy" SnippetName="Account/Register/PageCopy" CssClass="page-copy" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<form id="Register" runat="server">
		<asp:ScriptManager runat="server" />
		<asp:Panel ID="SecureRegister" Visible="false" runat="server">
			<% if (!string.IsNullOrWhiteSpace(this.InvitationCode)) { %>
				<div class="alert alert-info">
					<adx:Snippet ID="InvitationCodeAlert" SnippetName="Account/Redeem/InvitationCodeAlert" DefaultText="<%$ ResourceManager:Redeeming_code %>" Visible="false" CssClass="alert alert-info text-wrap" runat="server" Literal="True" />
					<strong><%: this.InvitationCode %></strong>
				</div>
			<% } %>
			<div class="row">
				<asp:Panel ID="LocalLogin" Visible="false" runat="server">
					<div class="col-md-6">
						<div class="form-horizontal">
								<fieldset>
									<legend>
										<adx:Snippet ID="RegisterLocalFormHeading" SnippetName="Account/Register/RegisterLocalFormHeading" DefaultText="<%$ ResourceManager:Register_For_New_Local_Account %>" runat="server" />
									</legend>
									<asp:ValidationSummary ID="ValidationSummary1" CssClass="alert alert-block alert-danger" runat="server" ValidationGroup="loginValidationSummary" DisplayMode="BulletList" EnableClientScript="False"></asp:ValidationSummary>
									<asp:Panel ID="ShowEmail" Visible="false" runat="server">
										<div class="form-group">
											<label class="col-sm-4 control-label required">
												<adx:Snippet ID="EmailLabel" SnippetName="Account/Register/EmailLabel" DefaultText="<%$ ResourceManager:Email_DefaultText %>" HtmlTag="Span" runat="server" /></label>
											<div class="col-sm-8">
												<asp:TextBox ID="EmailTextBox" CssClass="form-control" runat="server"></asp:TextBox>
											</div>
										</div>
									</asp:Panel>
									<asp:Panel ID="ShowUserName" Visible="false" runat="server">
										<div class="form-group">
											<label class="col-sm-4 control-label required">
												<adx:Snippet ID="UsernameLabel" SnippetName="Account/Register/UsernameLabel" DefaultText="<%$ ResourceManager:UserName_Exception %>" HtmlTag="Span" runat="server" /></label>
											<div class="col-sm-8">
												<asp:TextBox ID="UserNameTextBox" CssClass="form-control" runat="server"></asp:TextBox>
											</div>
										</div>
									</asp:Panel>
									<div class="form-group">
										<label class="col-sm-4 control-label required">
											<adx:Snippet ID="PasswordLabel" SnippetName="Account/Register/PasswordLabel" DefaultText="<%$ ResourceManager:Password_Defaulttext %>" HtmlTag="Span" runat="server" /></label>
										<div class="col-sm-8">
											<asp:TextBox ID="PasswordTextBox" CssClass="form-control" TextMode="Password" runat="server"></asp:TextBox>
										</div>
									</div>
									<div class="form-group">
										<label class="col-sm-4 control-label required">
											<adx:Snippet ID="ConfirmPasswordLabel" SnippetName="Account/Register/ConfirmPasswordLabel" DefaultText="<%$ ResourceManager:Confirm_Password %>" HtmlTag="Span" runat="server" /></label>
										<div class="col-sm-8">
											<asp:TextBox ID="ConfirmPasswordTextBox" CssClass="form-control" TextMode="Password" runat="server"></asp:TextBox>
										</div>
									</div>
									<asp:Panel ID="CaptchaRowPlaceHolder" Visible="false" runat="server">
										<div class="form-group">
											<label class="col-sm-4 control-label"></label>
											<div class="col-sm-8">
												<asp:Panel ID="CaptchaControlContainer" runat="server">
												</asp:Panel>
											</div>
										</div>
									</asp:Panel>
									<div class="form-group">
										<div class="col-sm-offset-4 col-sm-8">
											<asp:Button ID="SubmitButton" OnClick="SubmitButton_Click" Text='<%$ Snippet: Account/Register/RegisterButtonText, Register_DefaultText %>' CssClass="btn btn-primary"
												ValidationGroup="loginValidationSummary" runat="server" />
										</div>
									</div>
								</fieldset>
							</div>
					</div>
				</asp:Panel>
				<asp:Panel ID="ExternalLogin" Visible="false" runat="server">
					<div class="col-md-6">
						<%: Html.AntiForgeryToken() %>
						<div class="form-horizontal">
							<fieldset>
								<legend>
									<adx:Snippet ID="RegisterExternalLabel" SnippetName="Account/Register/RegisterExternalFormHeading" DefaultText="<%$ ResourceManager:Register_Using_External_Account %>" HtmlTag="Span" runat="server" /></legend>
								<asp:Panel ID="ExternalLoginButtons" Visible="true" runat="server"></asp:Panel>
							</fieldset>
						</div>
					</div>
				</asp:Panel>
			</div>
		</asp:Panel>
		<asp:Panel runat="server" ID="RegistrationDisabled">
			<div class="col-md-12">
				<fieldset>
					<legend>
						<adx:Snippet ID="RegistrationDisabledMessage" SnippetName="Account/Register/RegistrationDisabledMessage" DefaultText="<%$ ResourceManager:Account_Register_Registration_Disabled_Message %>" HtmlTag="Span" runat="server" />
					</legend>
				</fieldset>
			</div>
		</asp:Panel>
		<script type="text/javascript">
			$(document).ready(function () {
				if ($('#loginValidationSummary').length) {
					if ($('#loginValidationSummary').find('li').length > 0) {
						$('#loginValidationSummary').attr("role", "alert");
						$('#loginValidationSummary').attr("aria-live", "assertive");
						$('#loginValidationSummary').focus();
					}
				}

				$(function () {
					$("#SubmitButton").click(function () {
						$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
					});
				});
			});
		</script>
	</form>
</asp:Content>
