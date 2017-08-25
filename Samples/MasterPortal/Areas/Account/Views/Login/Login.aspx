<%@ Page Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<asp:Content ContentPlaceHolderID="AccountNavBar" runat="server">
	<% Html.RenderPartial("AccountNavBar", new ViewDataDictionary(ViewData) { { "SubArea", "SignIn" } }); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/SignIn/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% if (!string.IsNullOrWhiteSpace(ViewBag.InvitationCode)) { %>
		<div class="alert alert-info"><%: Html.SnippetLiteral("Account/Redeem/InvitationCodeAlert", ResourceManager.GetString("Redeeming_code")) %> <strong><%: ViewBag.InvitationCode %></strong></div>
	<% } %>
	<div class="row">
		<% if (ViewBag.Settings.LocalLoginEnabled) { %>
			<div class="col-md-6">
				<% Html.RenderPartial("LoginLocal", ViewData["local"]); %>
			</div>
		<% } %>
		<% if (ViewBag.AzureAdOrExternalLoginEnabled) { %>
			<div class="col-md-6">
				<% Html.RenderPartial("LoginExternal", ViewData["external"]); %>
			</div>
		<% } %>
	</div>
	<script type="text/javascript">
				$(document).ready(function () {
					shell.refreshToken();
					if ($('#loginValidationSummary').length) {
						if($('#loginValidationSummary').find('li').length>0)
						{
							$('#loginValidationSummary').attr("role", "alert");
							$('#loginValidationSummary').attr("aria-live", "assertive");
							$('#loginValidationSummary').focus();
						}
					}
				});
</script>
</asp:Content>
