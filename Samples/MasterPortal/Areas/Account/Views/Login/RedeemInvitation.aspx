<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.RedeemInvitationViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="AccountNavBar" runat="server">
	<% Html.RenderPartial("AccountNavBar", new ViewDataDictionary(ViewData) { { "SubArea", "Redeem" } }); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/RedeemInvitation/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("RedeemInvitation", "Login", new { area = "Account", ReturnUrl = ViewBag.ReturnUrl })) { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal" >
			<fieldset>
				<legend><%: Html.TextSnippet("Account/Redeem/InvitationCodeFormHeading", defaultValue: ResourceManager.GetString("Signup_With_Invitation_Code"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new { @class = "alert alert-block alert-danger",@id="redeemInvitation-validation-summary"}) %>
				<div class="form-group">
					<% var invitationCode = ResourceManager.GetString("Invitation_Code_Defaulttext"); %>
					<label class="col-sm-2 control-label required" for="InvitationCode"><%: Html.TextSnippet("Account/Redeem/InvitationCodeLabel", defaultValue: invitationCode, tagName: "span") %></label>
					<div class="col-sm-10">
						<%= Html.TextBoxFor(model => model.InvitationCode, new { @class = "form-control", aria_label = string.Format(ResourceManager.GetString("Required_Field_Error"), invitationCode) }) %>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<div class="checkbox">
							<label>
								<%: Html.CheckBoxFor(model => model.RedeemByLogin) %>
								<%: Html.TextSnippet("Account/Redeem/RedeemByLoginLabel", defaultValue: ResourceManager.GetString("Account_Already_Exist_Message"), tagName: "span") %>
							</label>
						</div>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<button id="submit-redeem-invitation" class="btn btn-primary"><%: Html.SnippetLiteral("Account/Redeem/InvitationCodeFormButtonText", ResourceManager.GetString("Register_DefaultText")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
	<script type="text/javascript">
		$(function() {
			$("#submit-redeem-invitation").click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
		});
		$(document).ready(function () {
		    if ($('#redeemInvitation-validation-summary').length) {
		        $('#redeemInvitation-validation-summary').attr("role", "alert");
		        $('#redeemInvitation-validation-summary').attr("aria-live", "assertive");
		    }
		});
	</script>
</asp:Content>
