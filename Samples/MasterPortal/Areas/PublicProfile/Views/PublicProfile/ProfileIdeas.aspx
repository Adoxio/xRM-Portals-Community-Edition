<%@ Page Language="C#" MasterPageFile="PublicProfile.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.PublicProfile.ViewModels.ProfileViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Data" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<ul class="toolbar-nav nav nav-tabs">
		<% if (Model.IsBlogPostsEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Blog/Posts/Label",ResourceManager.GetString("Blog_Posts_Label")), "ProfileBlogPosts")%></li>
		<% } %>
		<li class="active"><a href="#ideas" data-toggle="tab" title="<%:ResourceManager.GetString("Ideas_Label") %>"><%: Html.SnippetLiteral("Ideas/Label",ResourceManager.GetString("Ideas_Label")) %></a></li>
		<% if (Model.IsForumPostsEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Forum/Posts/Label",ResourceManager.GetString("Forum_Posts_Label")), "ProfileForumPosts")%></li>
		<% } %>
	</ul>
	<div class="tab-content">
		<div class="tab-pane fade active in" id="ideas">
			<ul class="activity-list">
				<% IPaginated paginatedList = Model.Ideas;
				foreach (var idea in Model.Ideas) { %>
					<li class="bottom-break">
						<h3><%: Html.ActionLink(idea.Title, "Ideas", "Ideas", new { ideaForumPartialUrl = idea.IdeaForumPartialUrl, ideaPartialUrl = idea.PartialUrl, area = "Ideas" }, null) %></h3>
						<p class="metadata">
							<abbr class="timeago"><%: idea.SubmittedOn.ToString("r") %></abbr>
							&ndash;
							<%: Html.ActionLink(idea.IdeaForumTitle, "Ideas", "Ideas", new { ideaForumPartialUrl = idea.IdeaForumPartialUrl, area = "Ideas" }, null) %>
							&ndash;
							<% Html.RenderPartial("~/Areas/Ideas/Views/Shared/IdeaStatus.ascx", idea); %>			
						</p>
						<div><%= idea.Copy %></div>
					</li>
				<% } %>
			</ul>
			<% Html.RenderPartial("Pagination", paginatedList); %>
		</div>
	</div>
</asp:Content>
