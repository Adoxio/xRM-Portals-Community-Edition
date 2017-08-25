<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<% Func<string, bool> isCurrentAction = action => string.Equals(action, ViewContext.RouteData.Values["action"] as string, StringComparison.Ordinal); %>

<div class="panel panel-default nav-profile">
	<div class="panel-heading">
		<h3 class="panel-title"><span class="fa fa-lock" aria-hidden="true"></span> <%: Html.TextSnippet("Profile/SecurityNav/Title", defaultValue: ResourceManager.GetString("Security_Defaulttext"), tagName: "span") %></h3>
	</div>
	<div class="list-group nav-profile">
		<% if (ViewBag.Settings.LocalLoginEnabled) { %>
			<% if (ViewBag.Nav.HasPassword) { %>
				<a class="list-group-item <%: isCurrentAction("ChangePassword") ? "active" : string.Empty %>" href="<%: Url.Action("ChangePassword", "Manage", new { area = "Account", region = ViewBag.Region })%>" title='<%: Html.SnippetLiteral("Profile/SecurityNav/ChangePassword",  ResourceManager.GetString("Change_Password_DefaultText")) %>'>
					<%: Html.TextSnippet("Profile/SecurityNav/ChangePassword", defaultValue: ResourceManager.GetString("Change_Password_DefaultText"), tagName: "span") %>
				</a>
			<% } else { %>
				<a class="list-group-item <%: isCurrentAction("SetPassword") ? "active" : string.Empty %>" href="<%: Url.Action("SetPassword", "Manage", new { area = "Account", region = ViewBag.Region })%>" title="<%: Html.SnippetLiteral("Profile/SecurityNav/SetPassword", ResourceManager.GetString("Set_Password_Defaulttext")) %>">
					<%: Html.TextSnippet("Profile/SecurityNav/SetPassword", defaultValue: ResourceManager.GetString("Set_Password_Defaulttext"), tagName: "span") %>
				</a>
			<% } %>
		<% } %>
		<% if (ViewBag.Settings.LocalLoginEnabled && ViewBag.Settings.EmailConfirmationEnabled) { %>
			<a class="list-group-item <%: isCurrentAction("ChangeEmail") ? "active" : string.Empty %>" href="<%: Url.Action("ChangeEmail", "Manage", new { area = "Account", region = ViewBag.Region })%>" title="<%: Html.SnippetLiteral("Profile/SecurityNav/ChangeEmail",ResourceManager.GetString("Change_Email_Defaulttext")) %>">
				<% if (ViewBag.Settings.EmailConfirmationEnabled && !ViewBag.Nav.IsEmailConfirmed) { %>
					<span class="fa fa-exclamation-circle pull-right profile-alert" title="<%: Html.SnippetLiteral("Profile/SecurityNav/Unconfirmed", ResourceManager.GetString("Unconfirmed")) %>"></span>
				<% } %>
				<%: Html.TextSnippet("Profile/SecurityNav/ChangeEmail", defaultValue: ResourceManager.GetString("Change_Email_Defaulttext"), tagName: "span") %>
			</a>
		<% } %>
		<% if (ViewBag.Settings.MobilePhoneEnabled) { %>
			<a class="list-group-item <%: isCurrentAction("ChangePhoneNumber") ? "active" : string.Empty %>" href="<%: Url.Action("ChangePhoneNumber", "Manage", new { area = "Account", region = ViewBag.Region })%>" title="<%: Html.SnippetLiteral("Profile/SecurityNav/ChangeMobilePhone", ResourceManager.GetString("Change_Mobile_Phone_Defaulttext")) %>">
				<% if (!ViewBag.Nav.IsMobilePhoneConfirmed) { %>
					<span class="fa fa-exclamation-circle pull-right profile-alert" title="<%: Html.SnippetLiteral("Profile/SecurityNav/Unconfirmed", ResourceManager.GetString("Unconfirmed")) %>"></span>
				<% } %>
				<%: Html.TextSnippet("Profile/SecurityNav/ChangeMobilePhone", defaultValue: ResourceManager.GetString("Change_Mobile_Phone_Defaulttext"), tagName: "span") %>
			</a>
		<% } %>
		<% if (ViewBag.Settings.TwoFactorEnabled) { %>
			<a class="list-group-item <%: isCurrentAction("ChangeTwoFactor") ? "active" : string.Empty %>" href="<%: Url.Action("ChangeTwoFactor", "Manage", new { area = "Account", region = ViewBag.Region })%>" title="<%: Html.SnippetLiteral("Profile/SecurityNav/ChangeTwoFactor", ResourceManager.GetString("Change_Two_Factor_Authentication")) %>">
				<% if (ViewBag.Nav.IsTwoFactorEnabled && !ViewBag.Nav.IsEmailConfirmed) { %>
					<span class="fa fa-exclamation-circle pull-right profile-alert" title="<%: Html.SnippetLiteral("Profile/SecurityNav/Pending", ResourceManager.GetString("Pending")) %>"></span>
				<% } %>
				<%: Html.TextSnippet("Profile/SecurityNav/ChangeTwoFactor", defaultValue: ResourceManager.GetString("Change_Two_Factor_Authentication"), tagName: "span") %>
			</a>
		<% } %>
		<% if (ViewBag.Settings.ExternalLoginEnabled && string.IsNullOrWhiteSpace(ViewBag.Settings.LoginButtonAuthenticationType)) { %>
			<a class="list-group-item <%: isCurrentAction("ChangeLogin") ? "active" : string.Empty %>" href="<%: Url.Action("ChangeLogin", "Manage", new { area = "Account", region = ViewBag.Region })%>" title="<%: Html.SnippetLiteral("Profile/SecurityNav/ChangeLogin", ResourceManager.GetString("Manage_External_Authentication")) %>">
				<%: Html.TextSnippet("Profile/SecurityNav/ChangeLogin", defaultValue: ResourceManager.GetString("Manage_External_Authentication"), tagName: "span") %>
			</a> 
		<% } %>
	</div>
</div>