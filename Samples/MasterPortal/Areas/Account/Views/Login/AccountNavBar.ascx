<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<% if (ViewBag.Settings.RegistrationEnabled && (ViewBag.Settings.OpenRegistrationEnabled || ViewBag.Settings.InvitationEnabled)) { %>
	<ul class="nav nav-tabs nav-account" role="navigation">
		<li class="<%: ViewBag.SubArea == "SignIn" ? "active" : string.Empty %>"><a href="<%: Url.SignInUrl(string.Empty) %>" title="<%: Html.SnippetLiteral("Account/Nav/SignIn", ResourceManager.GetString("Sign_In")) %>"><span class="fa fa-sign-in" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/Nav/SignIn", ResourceManager.GetString("Sign_In")) %></a></li>
		<% if (ViewBag.Settings.OpenRegistrationEnabled && !ViewBag.IsESS) { %>
			<li class="<%: ViewBag.SubArea == "Register" ? "active" : string.Empty %>">
				<a href="<%: string.IsNullOrEmpty(ViewBag.Settings.LoginButtonAuthenticationType) ? Url.SecureRegistrationUrl(string.Empty) : Url.SignInUrl(string.Empty) %>" title="<%: Html.SnippetLiteral("Account/Nav/Register", ResourceManager.GetString("Register_DefaultText")) %>"><%: Html.SnippetLiteral("Account/Nav/Register", ResourceManager.GetString("Register_DefaultText")) %></a>
			</li>
		<% } %>
		<% if (ViewBag.Settings.InvitationEnabled && !ViewBag.IsESS) { %>
		<li class="<%: ViewBag.SubArea == "Redeem" ? "active" : string.Empty %>"><a href="<%: Url.RedeemUrl(string.Empty) %>" title="<%: Html.SnippetLiteral("Account/Nav/Redeem", ResourceManager.GetString("Redeem_Invitation")) %>"><%: Html.SnippetLiteral("Account/Nav/Redeem", ResourceManager.GetString("Redeem_Invitation")) %></a></li>
		<% } %>
	</ul>
<% } %>