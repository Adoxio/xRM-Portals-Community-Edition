<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<% Func<string, bool> isCurrentAction = action => string.Equals(action, ViewContext.RouteData.Values["action"] as string, StringComparison.Ordinal); %>

<div class="panel panel-default nav-profile">
	<div class="panel-heading">
		<h3 class="panel-title"><span class="fa fa-lock" aria-hidden="true"></span> <%: Html.TextSnippet("Profile/SecurityNav/Title", defaultValue: ResourceManager.GetString("Security_Defaulttext"), tagName: "span") %></h3>
	</div>
	<div class="list-group nav-profile">
		<a class="list-group-item <%: isCurrentAction("ChangeEmail") ? "active" : string.Empty %>" href="<%: Url.Action("ChangeEmail", "Manage", new { area = "Account", region = ViewBag.Region })%>">
			<% if (!ViewBag.Nav.IsEmailConfirmed) { %>
				<span class="fa fa-exclamation-circle pull-right profile-alert" title="<%: Html.SnippetLiteral("Profile/SecurityNav/Unconfirmed", ResourceManager.GetString("Unconfirmed")) %>"></span>
			<% } %>
			<%: Html.TextSnippet("Profile/SecurityNav/ChangeEmail", defaultValue: ResourceManager.GetString("Change_Email_Defaulttext"), tagName: "span") %>
		</a>
		<a class="list-group-item <%: isCurrentAction("ChangePhoneNumber") ? "active" : string.Empty %>" href="<%: Url.Action("ChangePhoneNumber", "Manage", new { area = "Account", region = ViewBag.Region })%>">
			<% if (!ViewBag.Nav.IsMobilePhoneConfirmed) { %>
				<span class="fa fa-exclamation-circle pull-right profile-alert" title="<%: Html.SnippetLiteral("Profile/SecurityNav/Unconfirmed", ResourceManager.GetString("Unconfirmed")) %>"></span>
			<% } %>
			<%: Html.TextSnippet("Profile/SecurityNav/ChangeMobilePhone", defaultValue: ResourceManager.GetString("Change_Mobile_Phone_Defaulttext"), tagName: "span") %>
		</a>
	</div>
</div>