<%@ Page Title="" Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.VerifyPhoneNumberViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/VerifyPhoneNumber/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("VerifyPhoneNumber", "Manage")) { %>
		<%: Html.AntiForgeryToken() %>
		<%: Html.Hidden("phoneNumber", Model.PhoneNumber) %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/ChangePhoneNumber/ChangePhoneNumberFormHeading", defaultValue: ResourceManager.GetString("Change_Mobile_Phone_Defaulttext"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<%: Html.Partial("ProfileMessage", (string) ViewBag.Message ?? string.Empty) %>
				<div class="form-group">
					<label class="col-sm-2 control-label" for="Number"><%: Html.TextSnippet("Account/ChangePhoneNumber/CodeLabel", defaultValue: ResourceManager.GetString("Code_Defaulttext"), tagName: "span") %></label>
					<div class="col-sm-10">
						<%: Html.TextBoxFor(model => model.Code, new { @class = "form-control" }) %>
						<p class="help-block"><span class="fa fa-mobile" aria-hidden="true"></span> <%: Html.TextSnippet("Account/ChangePhoneNumber/VerifyCodeFromPhoneText", defaultValue: ResourceManager.GetString("Check_Your_Mobile_For_Security_Code"), tagName: "span") %></p>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<input type="submit" class="btn btn-primary" value="<%: Html.SnippetLiteral("Account/ChangePhoneNumber/VerifyPhoneNumberButtonText", ResourceManager.GetString("Verify")) %>"/>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
	<% if (ViewBag.Settings.IsDemoMode && ViewBag.DemoModeCode != null) { %>
		<div class="panel panel-warning">
			<div class="panel-heading">
				<h3 class="panel-title"><span class="fa fa-wrench" aria-hidden="true"></span> DEMO MODE <small>LOCAL ONLY</small></h3>
			</div>
			<div class="panel-body">
				<%: ViewBag.DemoModeCode %>
			</div>
		</div>
	<% } %>
</asp:Content>
