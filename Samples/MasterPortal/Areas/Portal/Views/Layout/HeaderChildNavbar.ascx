<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<% var childNodes = Html.CurrentWebLinkChildNodes("Primary Navigation", new [] { "adx_event", "adx_webfile" }).ToArray(); %>
<% if (childNodes.Any()) { %>
	<div class="header-navbar navbar navbar-inverse navbar-static-top navbar-child hidden-xs" role="navigation">
		<div class="container">
			<ul class="nav navbar-nav" data-state="sitemap" data-sitemap-current="active" data-sitemap-ancestor="active">
				<% foreach (var childNode in childNodes) { %>
					<li data-sitemap-node="<%: childNode.Url %>">
						<a href="<%: childNode.Url %>" title="<%: childNode.Title %>"><%: childNode.Title %></a>
					</li>
				<% } %>
			</ul>
		</div>
	</div>
<% } else { %>
	<div class="header-navbar navbar navbar-inverse navbar-static-top navbar-child navbar-empty hidden-xs" aria-hidden="true">
		<div class="container">
		</div>
	</div>
<% } %>