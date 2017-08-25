<%@ Page Language="C#" MasterPageFile="../Shared/Ideas.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Ideas.ViewModels.IdeaForumViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Ideas" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Core.Flighting" %>
<%@ Import Namespace="Adxstudio.Xrm.Globalization" %>
<%@ Import Namespace="Site.Areas.Ideas" %>

<asp:Content runat="server" ContentPlaceHolderID="Title"><%: Model.IdeaForum.Title %></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<ul class="breadcrumb">
		<% Html.RenderPartial("SiteMapPath"); %>
		<li class="active"><%: Model.IdeaForum.Title %></li>
	</ul>
	<div class="page-header">
		<h1><%: Model.IdeaForum.Title %></h1>
	</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<div class="bottom-break"><%= Model.IdeaForum.Summary %></div>
	<ul class="ideas-nav nav nav-tabs">
		<li <%= (RouteData.Values["filter"] ?? "top") as string == "top" ? @"class=""active""" : string.Empty %>>
			<%: Html.ActionLink(ResourceManager.GetString("IdeaForum_Top"), "Filter", new { filter = "top", status = RouteData.Values["status"], timeSpan = RouteData.Values["timeSpan"] }, new { title = ResourceManager.GetString("IdeaForum_Top") })%>
		</li>
		<li <%= RouteData.Values["filter"] as string == "hot" ? @"class=""active""" : string.Empty %>>
			<%: Html.ActionLink(ResourceManager.GetString("IdeaForum_Hot"), "Filter", new { filter = "hot", status = RouteData.Values["status"], timeSpan = RouteData.Values["timeSpan"] }, new { title = ResourceManager.GetString("IdeaForum_Hot") })%>
		</li>
		<li <%= RouteData.Values["filter"] as string == "new" ? @"class=""active""" : string.Empty %>>
			<%: Html.ActionLink(ResourceManager.GetString("IdeaForum_New"), "Filter", new { filter = "new", status = RouteData.Values["status"], timeSpan = RouteData.Values["timeSpan"] }, new { title = ResourceManager.GetString("IdeaForum_New") })%>
		</li>
		<li id="status-filter" class="dropdown">
			<a class="dropdown-toggle" data-toggle="dropdown" href="#" title="<%: ResourceManager.GetString("IdeaForum_Status") %> <%: Model.CurrentStatusLabel %>"><%: ResourceManager.GetString("IdeaForum_Status") %> <%: Model.CurrentStatusLabel %> <b class="caret"></b></a>
			<ul class="dropdown-menu">
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_Any"), "Filter", new { status = (int?)IdeaStatus.Any }, new {title = ResourceManager.GetString("IdeaForum_Any") })%></li>
				<% foreach (var option in Model.IdeaForum.IdeaStatusOptionSetMetadata.Options)
					{ %>
					<li>
						<% var statusLabel = option.Label.GetLocalizedLabelString().ToLower(); %>
						<% var statusValue = option.Value; %>
						<%: Html.ActionLink(statusLabel, "Filter", new { status = statusValue }, new { title = statusLabel }) %>
					</li>
				<% } %>
			</ul>
		</li>
		<li id="time-span-filter" class="dropdown">
			<a class="dropdown-toggle" data-toggle="dropdown" href="#" title="<%: ResourceManager.GetString("IdeaForum_From") %> <%: RouteData.Values["timeSpan"] == null ? ResourceManager.GetString("IdeaForum_All_Time") : RouteData.Values["timeSpan"].ToString() == "all-time" ? ResourceManager.GetString("IdeaForum_All_Time") : RouteData.Values["timeSpan"].ToString() == "today" ? ResourceManager.GetString("IdeaForum_Today") : RouteData.Values["timeSpan"].ToString() == "this-week" ? ResourceManager.GetString("IdeaForum_This_Week") : RouteData.Values["timeSpan"].ToString() == "this-month" ? ResourceManager.GetString("IdeaForum_This_Month") : RouteData.Values["timeSpan"].ToString() == "this-year" ? ResourceManager.GetString("IdeaForum_This_Year") : ResourceManager.GetString("IdeaForum_All_Time")%>"><%: ResourceManager.GetString("IdeaForum_From") %> <%: RouteData.Values["timeSpan"] == null ? ResourceManager.GetString("IdeaForum_All_Time") : RouteData.Values["timeSpan"].ToString() == "all-time" ? ResourceManager.GetString("IdeaForum_All_Time") : RouteData.Values["timeSpan"].ToString() == "today" ? ResourceManager.GetString("IdeaForum_Today") : RouteData.Values["timeSpan"].ToString() == "this-week" ? ResourceManager.GetString("IdeaForum_This_Week") : RouteData.Values["timeSpan"].ToString() == "this-month" ? ResourceManager.GetString("IdeaForum_This_Month") : RouteData.Values["timeSpan"].ToString() == "this-year" ? ResourceManager.GetString("IdeaForum_This_Year") : ResourceManager.GetString("IdeaForum_All_Time")%><b class="caret"></b></a>
			<ul class="dropdown-menu">
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_All_Time"), "Filter", new { timeSpan = "all-time", status = RouteData.Values["status"] },new {title = ResourceManager.GetString("IdeaForum_All_Time") })%></li>
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_Today"), "Filter", new { timeSpan = "today", status = RouteData.Values["status"] }, new {title = ResourceManager.GetString("IdeaForum_Today") })%></li>
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_This_Week"), "Filter", new { timeSpan = "this-week", status = RouteData.Values["status"] }, new {title = ResourceManager.GetString("IdeaForum_This_Week") })%></li>
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_This_Month"), "Filter", new { timeSpan = "this-month", status = RouteData.Values["status"] }, new {title = ResourceManager.GetString("IdeaForum_This_Month") })%></li>
				<li><%: Html.ActionLink(ResourceManager.GetString("IdeaForum_This_Year"), "Filter", new { timeSpan = "this-year", status = RouteData.Values["status"] }, new {title = ResourceManager.GetString("IdeaForum_This_Year") })%></li>
			</ul>
		</li>        
	</ul>
	<ul id="ideas" class="list-unstyled clearfix">
		<% foreach (var idea in Model.Ideas) { %>
			<li>
				<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
				   { %>
					<div id="vote-status-<%: idea.Id %>">
						<% Html.RenderPartial("Votes", idea); %>
					</div>
				<% } %>
				<div class="idea-container">
					<h3><a href= "<%: idea.Url %>" title= "<%: idea.Title %>" > <%: idea.Title %> </a></h3>
					<p>
						<%: Html.SnippetLiteral("Idea Author Label", ResourceManager.GetString("Suggested_By"))%>
						<% if (idea.AuthorId.HasValue) { %>
							<a href="<%: Url.AuthorUrl(idea) %>" title="<%: idea.AuthorName %>"><%: idea.AuthorName %></a>
						<% } else { %>
							<%: idea.AuthorName %>
						<% } %>
						&ndash;<% Html.RenderPartial("IdeaStatus", idea); %>
						&ndash; <a href="<%: Url.Action("Ideas", "Ideas", new { ideaForumPartialUrl = idea.IdeaForumPartialUrl, ideaPartialUrl = idea.PartialUrl }) %>#comments" title=" <%: idea.CommentCount %>">
							<span class="fa fa-comment" aria-hidden="true"></span> <%: idea.CommentCount %></a>
					</p>
					<%= idea.Copy %>
				</div>
			</li>
		<% } %>
	</ul>
	<% Html.RenderPartial("Pagination", Model.Ideas); %>
	<% if (Model.IdeaForum.CurrentUserCanSubmitIdeas) { %>
		<div id="create-idea">
			<% Html.RenderPartial("CreateIdea", Model.IdeaForum); %>
		</div>
	<% } %>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="SideBarContent">
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
	   { %>
		<% if ((Model.IdeaForum.VotingPolicy == IdeaForumVotingPolicy.OpenToAuthenticatedUsers || Model.IdeaForum.IdeaSubmissionPolicy == IdeaForumIdeaSubmissionPolicy.OpenToAuthenticatedUsers) && !Request.IsAuthenticated)
		   { %>
			<div class="section">
			<div class="alert alert-block alert-info"><%: Html.SnippetLiteral("idea-forum/sign-in-message", ResourceManager.GetString("Feedback_In_This_Idea_Forum"))%></div>
			</div>
		<% } %>
		<% if (Model.IdeaForum.VotesPerUser.HasValue)
		   { %>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-thumbs-o-up" aria-hidden="true"></span>
					<%: Html.TextSnippet("Ideas Voting Heading", defaultValue: ResourceManager.GetString("Voting_Defaulttext"), tagName: "span") %>
					</h4>
				</div>
				<ul class="list-group">
					<li id="votes-left" class="list-group-item">
						<span class="badge"><%: Model.IdeaForum.VotesPerUser - Model.IdeaForum.CurrentUserActiveVoteCount %></span>
					<%: Html.SnippetLiteral("Ideas User Votes Left", ResourceManager.GetString("Your_Number_Of_Votes_Left"))%>
					</li>
				</ul>
			</div>
		<% } %>
	<% } %>
</asp:Content>
