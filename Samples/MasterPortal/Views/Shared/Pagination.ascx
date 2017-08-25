<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Data.IPaginated>" %>
<%@ Import Namespace="Site.Helpers" %>

<% if (Model.TotalPages > 1) { %>
	<ul class="pagination">
		<% if (Model.HasPreviousPage) { %>
			<li>
				<a href="<%= Url.ActionWithQueryString(Page.RouteData.Values["action"] as string, new { page = Model.PageNumber - 1 }) %>">&lsaquo;</a>
			</li>
		<% }
		if (Model.TotalPages > 9 && Model.PageNumber >= 6) { %>
			<li>
				<a href="<%= Url.ActionWithQueryString(Page.RouteData.Values["action"] as string, new { page = 1 }) %>">1&hellip;</a>
			</li>
		<% }
		var pageNumber = Model.PageNumber < 6 || Model.TotalPages <= 9
			? 1
			: Model.TotalPages - Model.PageNumber < 4
				? Model.TotalPages - 8
				: Model.PageNumber - 4;
		var pageLinkCount = 0;
		while (pageLinkCount < Model.TotalPages && pageLinkCount < 9)
		{
			if (pageNumber == Model.PageNumber) { %>
				<li class="active">
					<a title="<%: pageNumber %>"><%: pageNumber %></a>
				</li> <%
			} else { %>
				<li>
					<a href="<%= Url.ActionWithQueryString(Page.RouteData.Values["action"] as string, new { page = pageNumber }) %>" title="<%: pageNumber %>"><%: pageNumber %></a>
				</li>
			<% }
			pageLinkCount++;
			pageNumber++;
		}
		if (Model.TotalPages > 9 && Model.PageNumber < Model.TotalPages - 4) { %>
			<li>
				<a href="<%= Url.ActionWithQueryString(Page.RouteData.Values["action"] as string, new { page = Model.TotalPages }) %>">&hellip;<%= Model.TotalPages %></a>
			</li>
		<% }
		if (Model.HasNextPage) { %>
			<li>
				<a href="<%= Url.ActionWithQueryString(Page.RouteData.Values["action"] as string, new { page = Model.PageNumber + 1 }) %>">&rsaquo;</a>
			</li>
		<% } %>
	</ul>
<% } %>
