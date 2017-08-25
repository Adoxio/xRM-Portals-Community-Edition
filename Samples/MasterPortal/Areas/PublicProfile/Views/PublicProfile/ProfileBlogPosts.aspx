<%@ Page Language="C#" MasterPageFile="PublicProfile.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.PublicProfile.ViewModels.ProfileViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Data" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<ul class="toolbar-nav nav nav-tabs">
				<li class="active"><a href="#blog-posts" data-toggle="tab" title="<%:ResourceManager.GetString("Blog_Posts_Label") %>"><%: Html.SnippetLiteral("Blog/Posts/Label", ResourceManager.GetString("Blog_Posts_Label")) %></a></li>
		<% if (Model.IsIdeasEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Ideas/Label",ResourceManager.GetString("Ideas_Label")), "ProfileIdeas")%></li>
		<% } %>
		<% if (Model.IsForumPostsEnable) { %>
			<li><%: Html.ActionLink(Html.SnippetLiteral("Forum/Posts/Label",ResourceManager.GetString("Forum_Posts_Label")), "ProfileForumPosts")%></li>
		<% } %>
	</ul>
	<div class="tab-content">
		<div class="tab-pane fade active in" id="blog-posts">
			<ul class="activity-list">
				<% IPaginated paginatedBlogList = Model.BlogPosts;
				foreach (var post in Model.BlogPosts.Where(p => p.IsPublished)) { %>
					<li class="blog-post">
						<h3>
							<a href='<%= post.ApplicationPath.AppRelativePath %>'><%= post.Title ?? post.Entity.GetAttributeValue<string>("adx_name") %> </a>
						</h3>
						<div class="metadata">
							<abbr class="timeago"><%: post.Entity.GetAttributeValue<DateTime>("adx_date").ToString("r") %></abbr>
							&ndash;
							<a href="<%= post.ApplicationPath.AbsolutePath + "#comments" %>" title="<%= post.CommentCount %>">
								<span class="fa fa-comment" aria-hidden="true"></span> <%= post.CommentCount %> 
							</a>
						</div>
						<div>
							<% if (post.HasExcerpt) { %>
								<%= post.Summary %>
							<% } %>
						</div>
					</li>
				<% } %>
			</ul>
			<% Html.RenderPartial("Pagination", paginatedBlogList); %>
		</div>
	</div>
</asp:Content>
