<%@ Page Language="C#" MasterPageFile="../Shared/Issues.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Issues.ViewModels.IssuesViewModel>" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<% Html.RenderPartial("Breadcrumbs"); %>
	<div class="page-header">
		<h1><%: Html.TextAttribute("adx_name") %></h1>
	</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<div class="page-copy">
		<%= Html.HtmlAttribute("adx_copy") %>
	</div>
	<ul class="issues list-unstyled">
		<% foreach (var issueForum in Model.IssueForums)
		 { %>
		<li>
			<h3><%: Html.ActionLink(issueForum.Title, "Issues", new { issueForumPartialUrl = issueForum.PartialUrl }, new {title=issueForum.Title } ) %></h3>
			<div class="bottom-break"><%= issueForum.Summary %></div>
		</li>
		<% } %>
	</ul>
</asp:Content>
