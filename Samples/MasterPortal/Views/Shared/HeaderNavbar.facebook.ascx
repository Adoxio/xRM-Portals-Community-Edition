<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="DevTrends.MvcDonutCaching" %>

<div class="navbar navbar-default">
	<div class="container">
		<div class="navbar-left">
			<%: Html.HtmlSnippet("Facebook/Navbar/Left") %>
		</div>
		<%: Html.WebLinksNavBar("Facebook Primary Navigation", "weblinks navbar-left", "active", "active", clientSiteMapState: true) %>
		<% if (Request.IsAuthenticated) { %>
			<% var profileNavigation = Html.WebLinkSet("Facebook Profile Navigation"); %>
			<% if (profileNavigation != null && profileNavigation.WebLinks.Any()) { %>
				<ul class="nav navbar-nav navbar-right">
					<li class="dropdown">
						<a href="#" class="dropdown-toggle" data-toggle="dropdown">
							<span class="fa fa-user" aria-hidden="true"></span> <span class="caret"></span>
						</a>
						<ul class="dropdown-menu">
							<% foreach (var webLink in profileNavigation.WebLinks) { %>
								<%: Html.WebLinkListItem(webLink, false, false) %>
							<% } %>
						</ul>
					</li>
				</ul>
			<% } %>
		<% } else { %>
			<ul class="nav navbar-nav navbar-right">
				<li>
					<% Html.RenderAction("SignInLink", "Layout", new { area = "Portal" }, true); %>
				</li>
			</ul>
		<% } %>
		<div class="navbar-right">
			<%: Html.HtmlSnippet("Facebook/Navbar/Right") %>
		</div>
	</div>
</div>
