<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ExternalLoginConfirmationViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ExternalLoginConfirmation/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ExternalLoginConfirmation", "Login", new { ReturnUrl = ViewBag.ReturnUrl, InvitationCode = ViewBag.InvitationCode })) { %>
		<%: Html.AntiForgeryToken() %>
		<%: Html.HiddenFor(model => model.FirstName) %>
		<%: Html.HiddenFor(model => model.LastName) %>
		<%: Html.HiddenFor(model => model.Username) %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/Register/AssociateFormHeading", defaultValue: ResourceManager.GetString("Register_External_Account_Defaulttext"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<% if (!string.IsNullOrWhiteSpace(ViewBag.InvitationCode)) { %>
					<div class="form-group">
						<label class="col-sm-2 control-label" for="InvitationCode"><%: Html.TextSnippet("Account/Redeem/InvitationCodeLabel", defaultValue: ResourceManager.GetString("Invitation_Code_Defaulttext"), tagName: "span") %></label>
						<div class="col-sm-10">
							<%: Html.TextBox("InvitationCode", (string) ViewBag.InvitationCode, new { @class = "form-control", @readonly = "readonly" }) %>
						</div>
					</div>
				<% } %>
				<div class="form-group">
					<label class="col-sm-2 control-label" for="Email"><%: Html.TextSnippet("Account/Register/EmailLabel", defaultValue: ResourceManager.GetString("Email_DefaultText")) %></label>
					<div class="col-sm-10">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control" }) %>
						<p class="help-block"><%: Html.TextSnippet("Account/Register/EmailInstructionsText", defaultValue: ResourceManager.GetString("Provide_Email_Address"), tagName: "span") %></p>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<button class="btn btn-primary"><%: Html.SnippetLiteral("Account/Register/RegisterButtonText", ResourceManager.GetString("Register_DefaultText")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
</asp:Content>
