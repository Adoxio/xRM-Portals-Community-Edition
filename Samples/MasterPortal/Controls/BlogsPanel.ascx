<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlogsPanel.ascx.cs" Inherits="Site.Controls.BlogsPanel" %>
<%@ Import Namespace="Adxstudio.Xrm.Blogs" %>

<asp:ObjectDataSource ID="PostDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateBlogDataAdapter" SelectMethod="SelectPosts" runat="server">
	<SelectParameters>
		<asp:Parameter Name="startRowIndex" DefaultValue="0"/>
		<asp:Parameter Name="maximumRows" DefaultValue='<%$ SiteSetting: Home Blog Post Count, 4 %>'/>
	</SelectParameters>
</asp:ObjectDataSource>
<asp:ListView ID="Posts" DataSourceID="PostDataSource" runat="server">
	<LayoutTemplate>
		<div class="content-panel panel panel-default">
			<asp:ObjectDataSource ID="BlogDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateBlogDataAdapter" SelectMethod="Select" runat="server" />
			<div class="panel-heading">
				<asp:HyperLink CssClass="pull-right" NavigateUrl='<%$ CrmSiteMap: SiteMarker=Blog Home, Return=Url %>' Text='<%$ Snippet: Home All Blogs Link Text, All_Blogs_DefaultText %>' ToolTip='<%$ Snippet: Home All Blogs Link Text, All_Blogs_DefaultText %>' runat="server" />
				<h4>
					<asp:Repeater DataSourceID="BlogDataSource" runat="server">
						<ItemTemplate>
							<asp:HyperLink CssClass="feed-icon fa fa-rss-square" NavigateUrl='<%#: Eval("FeedPath.AbsolutePath") %>' ToolTip='<%$ Snippet: Blog Subscribe Heading, Subscribe_To_Blogs_DefaultText %>' runat="server">
								<span class="sr-only"><asp:Literal Text='<%$ Snippet: Blog Subscribe Heading, Subscribe_To_Blogs_DefaultText %>' runat="server" /></span>
							</asp:HyperLink>
						</ItemTemplate>
					</asp:Repeater>
					<adx:Snippet SnippetName="Home Blog Activity Heading" DefaultText="<%$ ResourceManager:Blogs_DefaultText %>" EditType="text" runat="server" />
				</h4>
			</div>
			<ul class="list-group">
				<li id="itemPlaceholder" runat="server" />
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li class="list-group-item" runat="server">
			<asp:HyperLink CssClass="user-avatar" NavigateUrl='<%# Url.AuthorUrl(Eval("Author") as IBlogAuthor) %>' ImageUrl='<%# Url.UserImageUrl(Eval("Author.EmailAddress")) %>' ToolTip='<%# HttpUtility.HtmlEncode(Eval("Author.Name") ?? "") %>' runat="server"/>
			<h4 class="list-group-item-heading">
				<asp:HyperLink NavigateUrl='<%#: Eval("ApplicationPath.AppRelativePath") %>' ToolTip='<%#: Eval("Title") %>' runat="server"><%#: Eval("Title") %></asp:HyperLink>
			</h4>
			<div class="content-metadata">
				<abbr class="timeago"><%#: Eval("PublishDate", "{0:r}") %></abbr>
				&ndash;
				<asp:HyperLink NavigateUrl='<%#: Url.AuthorUrl(Eval("Author") as IBlogAuthor) %>' Text='<%#: Eval("Author.Name") ?? "" %>' ToolTip='<%#: Eval("Author.Name") ?? "" %>' runat="server" />
				&ndash;
				<asp:HyperLink NavigateUrl='<%#: string.Format("{0}#comments", Eval("ApplicationPath.AbsolutePath")) %>' ToolTip=<%#: Eval("CommentCount") %> runat="server">
					<span class="fa fa-comment" aria-hidden="true"></span> <%#: Eval("CommentCount") %>
                    <span class="content-blog-count">comments</span>
				</asp:HyperLink>
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>