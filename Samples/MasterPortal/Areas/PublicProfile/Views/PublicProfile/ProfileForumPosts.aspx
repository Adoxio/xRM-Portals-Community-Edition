<%@ Page Language="C#" MasterPageFile="PublicProfile.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.PublicProfile.ViewModels.ProfileViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Data" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>


<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<ul class="toolbar-nav nav nav-tabs">
		<% if (Model.IsBlogPostsEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Blog/Posts/Label", ResourceManager.GetString("Blog_Posts_Label")), "ProfileBlogPosts")%></li>
		<% } %>
		<% if (Model.IsIdeasEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Ideas/Label",ResourceManager.GetString("Ideas_Label")), "ProfileIdeas")%></li>
		 <% } %>
		<li class="active"><a href="#forum-posts" data-toggle="tab" title="<%:ResourceManager.GetString("Forum_Posts_Label") %>"><%: Html.SnippetLiteral("Forum/Posts/Label", ResourceManager.GetString("Forum_Posts_Label")) %></a></li>
	</ul>
	<div class="tab-content">
		<div class="tab-pane fade active in" id="forum-posts">
			<ul class="activity-list">
				<% IPaginated paginatedForumPostList = Model.ForumPosts;
				foreach (var post in Model.ForumPosts) { %>
					<li>
						<h3>
							<a href="<%: post.Url %>" title="<%: post.Name %>"><%: post.Name %></a>
						</h3>
						<div class="metadata">
							<abbr class="timeago"><%: post.Entity.GetAttributeValue<DateTime>("adx_date").ToString("r") %></abbr>
							&ndash;
							<a href="<%: post.Thread.Url %>"><%: post.Thread.Name ?? post.Thread.Entity.GetAttributeValue<string>("adx_name") %></a>
						</div>
						<div>
							<%= post.Content %>
						</div>
					</li>
				<% } %>
			</ul>
			<% Html.RenderPartial("Pagination", paginatedForumPostList); %>
		</div>
	</div>
</asp:Content>
