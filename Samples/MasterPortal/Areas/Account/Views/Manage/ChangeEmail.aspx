<%@ Page Title="" Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ChangeEmailViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ChangeEmail/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
  <%: Html.TextSnippet("Profile/SecurityNav/ChangeEmail", defaultValue: ResourceManager.GetString("Change_Email_Defaulttext"), tagName: "span") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
    <% Html.RenderPartial("ProfileBreadcrumbs"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ChangeEmail", "Manage")) { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal">
			<fieldset>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-2 control-label" for="Email"><%: Html.TextSnippet("Account/ChangeEmail/EmailLabel", defaultValue: ResourceManager.GetString("Email_DefaultText"), tagName: "span") %></label>
					<div class="col-sm-10">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control" }) %>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<button class="btn btn-primary"><span class="fa fa-envelope-o" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangeEmail/ChangeEmailButtonText", ViewBag.Settings.EmailConfirmationEnabled ? ResourceManager.GetString("Change_Confirm_Email") : ResourceManager.GetString("Change_Email_Defaulttext")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
</asp:Content>
