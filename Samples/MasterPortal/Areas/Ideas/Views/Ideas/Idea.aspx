<%@ Page Language="C#" MasterPageFile="../Shared/Ideas.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Ideas.ViewModels.IdeaViewModel>" ValidateRequest="false" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Ideas" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Core.Flighting" %>

<asp:Content runat="server" ContentPlaceHolderID="Title"><%: Model.Idea.Title %></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<ul class="breadcrumb">
		<% Html.RenderPartial("SiteMapPath"); %>
		<li><%: Html.ActionLink(Model.IdeaForum.Title, "Ideas", new { ideaForumPartialUrl = Model.IdeaForum.PartialUrl, ideaPartialUrl = string.Empty }) %></li>
		<li class="active"><%: Model.Idea.Title %></li>
	</ul>
	<div class="page-header">
		<h1><%: Model.Idea.Title %></h1>
	</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
		{ %>
			<div id="vote-status-<%: Model.Idea.Id %>">
				<% Html.RenderPartial("Votes", Model.Idea); %>
			</div>
	<% } %>
	<div class="idea-container">
		<p><%: Html.SnippetLiteral("Idea Author Label", ResourceManager.GetString("Suggested_By"))%>
		<% if (Model.Idea.AuthorId.HasValue) { %>
			<a href="<%: Url.AuthorUrl(Model.Idea) %>" title="<%: Model.Idea.AuthorName %>"><%: Model.Idea.AuthorName %></a>
						<% } else { %>
							<%: Model.Idea.AuthorName %>
						<% } %>
		&ndash;<% Html.RenderPartial("IdeaStatus", Model.Idea); %></p>
		<%= Model.Idea.Copy %>
		<% if (!string.IsNullOrWhiteSpace(Model.Idea.StatusComment) && FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
	 { %>
			<h3><%: Html.SnippetLiteral("Idea Status Comment Label", ResourceManager.GetString("Status_Details"))%></h3>
			<%: @Html.Raw(Model.Idea.StatusComment) %>
		<% } %>
	</div>
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
	{%>
		<div id="comments">
			<% Html.RenderPartial("Comments", Model.Comments); %>
		</div>
	<% } %>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="SideBarContent">
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback)) { %>
		<% if ((Model.IdeaForum.VotingPolicy == IdeaForumVotingPolicy.OpenToAuthenticatedUsers || Model.Idea.CommentPolicy == IdeaForumCommentPolicy.OpenToAuthenticatedUsers) && !Request.IsAuthenticated) { %>
			<div class="section">
			<div class="alert alert-block alert-info"><%: Html.SnippetLiteral("idea/sign-in-message", ResourceManager.GetString("Feedback_For_This_Idea"))%></div>
			</div>
		<% } %>
		<% if (Model.IdeaForum.VotesPerUser.HasValue || Model.Idea.VotingType == IdeaForumVotingType.UpOrDown || (Model.Idea.VotesPerIdea > 1 || Model.Idea.VotingType == IdeaForumVotingType.UpOrDown)) { %>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-thumbs-o-up" aria-hidden="true"></span>
					<%: Html.TextSnippet("Ideas Voting Heading", defaultValue: ResourceManager.GetString("Voting_Defaulttext"), tagName: "span") %>
					</h4>
				</div>
				<ul class="list-group">
					<% if (Model.IdeaForum.VotesPerUser.HasValue) { %>
						<li id="votes-left" class="list-group-item">
							<span class="badge"><%: Model.IdeaForum.VotesPerUser - Model.IdeaForum.CurrentUserActiveVoteCount %></span>
						<%: Html.SnippetLiteral("Ideas User Votes Left", ResourceManager.GetString("Your_Number_Of_Votes_Left"))%>
						</li>
					<% }
					if (Model.Idea.VotingType == IdeaForumVotingType.UpOrDown) { %>
						<li class="list-group-item">
							<span class="badge"><%: Model.Idea.VoteUpCount %></span>
						<%: Html.SnippetLiteral("Idea Votes Up Label", ResourceManager.GetString("Votes_Up_For_This_Idea")) %>
						</li>
						<li class="list-group-item">
							<span class="badge"><%: Model.Idea.VoteDownCount %></span>
						<%: Html.SnippetLiteral("Idea Votes Down Label", ResourceManager.GetString("Votes_Down_For_This_Idea")) %>
						</li>
					<% }
					if (Model.Idea.VotesPerIdea > 1 || Model.Idea.VotingType == IdeaForumVotingType.UpOrDown) { %>
						<li class="list-group-item">
							<span class="badge"><%: Model.Idea.VoterCount %></span>
						<%: Html.SnippetLiteral("Idea Voter Count Label", ResourceManager.GetString("Votes_For_This_Idea")) %>
						</li>
					<% } %>
				</ul>
			</div>
		<% } %>
	<% } %>
</asp:Content>
