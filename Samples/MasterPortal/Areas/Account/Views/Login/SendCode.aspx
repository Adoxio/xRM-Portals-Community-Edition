<%@ Page Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.SendCodeViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/SendCode/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("SendCode", "Login", new { ReturnUrl = Model.ReturnUrl, InvitationCode = Model.InvitationCode })) { %>
		<%: Html.AntiForgeryToken() %>
		<%: Html.Hidden("rememberMe", Model.RememberMe) %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/SignIn/SendCodeFormHeading", defaultValue: ResourceManager.GetString("Send_Security_Code_Defaulttext"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="SelectedProvider"><%: Html.TextSnippet("Account/SignIn/SendCodeProviderLabel", defaultValue: ResourceManager.GetString("Two_Factor_Authentication_Method"), tagName: "span") %></label>
					<div class="col-sm-8">
						<%--<%: Html.DropDownListFor(model => model.SelectedProvider, Model.Providers, new { @class = "form-control" }) %>--%>
						<select class="form-control" id="SelectedProvider" name="SelectedProvider">
							<option value="PhoneCode"><%: Html.SnippetLiteral("Account/SignIn/SendCodeByPhoneOption", ResourceManager.GetString("Mobile_Phone_Defaulttext")) %></option>
							<option value="EmailCode"><%: Html.SnippetLiteral("Account/SignIn/SendCodeByEmailOption", ResourceManager.GetString("Email_DefaultText")) %></option>
						</select>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-4 col-sm-8">
						<button id="submit-send-code" class="btn btn-primary"><%: Html.SnippetLiteral("Account/SignIn/SendCodeButtonText", ResourceManager.GetString("Send")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
	<script type="text/javascript">
		$(function() {
			$("#submit-send-code").click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
		});
	</script>
</asp:Content>
