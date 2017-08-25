<%@ Page Language="C#" MasterPageFile="../Shared/Issues.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Issues.ViewModels.IssueViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Issues" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Core.Flighting" %>

<asp:Content runat="server" ContentPlaceHolderID="Title"><%: Model.Issue.Title %></asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<ul class="breadcrumb">
		<% Html.RenderPartial("SiteMapPath"); %>
		<li><%: Html.ActionLink(Model.IssueForum.Title, "Issues", new { issueForumPartialUrl = Model.IssueForum.PartialUrl, issuePartialUrl = string.Empty } ,new {title=Model.IssueForum.Title}) %></li>
		<li class="active"><%: Model.Issue.Title %></li>
	</ul>
	<div class="page-header">
		<h1><%: Model.Issue.Title %></h1>
	</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<div class="issue-container">
		<p>
		<% if (Model.Issue.AuthorId.HasValue) { %>
			<a href="<%: Url.AuthorUrl(Model.Issue) %>" title='<%: Model.Issue.AuthorName%>'><%: Model.Issue.AuthorName%></a>
		<% } else { %>
			<%: Model.Issue.AuthorName%>
		<% } %>
		&ndash; <abbr class="timeago"><%: Model.Issue.SubmittedOn.ToString("r") %></abbr>
		&ndash;<% Html.RenderPartial("IssueStatus", Model.Issue); %></p>
		<%= Model.Issue.Copy %>
		<% if (!string.IsNullOrWhiteSpace(Model.Issue.StatusComment)) { %>
			<h3><%: Html.SnippetLiteral("Issue Status Comment Label", ResourceManager.GetString("Status_Details"))%></h3>
			<%: Model.Issue.StatusComment %>
		<% } %>
	</div>

	<div id="comments">
		<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback)) 
		{ %>
			<% Html.RenderPartial("Comments", Model.Comments); %>
		<% } %>
	</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="SideBarContent">
	<% if ((Model.Issue.CommentPolicy == IssueForumCommentPolicy.OpenToAuthenticatedUsers) && !Request.IsAuthenticated) { %>
		<div class="section">
			<div class="alert alert-block alert-info"><%: Html.SnippetLiteral("issue/sign-in-message", ResourceManager.GetString("Provide_All_Types_Of_Feedback_For_This_Issue"))%></div>
		</div>
	<% } else { %>
		<div class="section">
			<div id="issue-tracking">
				<% Html.RenderPartial("Tracking", Model); %>
			</div>
		</div>
	<% } %>
</asp:Content>
