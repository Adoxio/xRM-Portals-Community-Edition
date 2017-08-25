<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewsPanel.ascx.cs" Inherits="Site.Controls.NewsPanel" %>
<%@ Import Namespace="Adxstudio.Xrm.Blogs" %>

<asp:ObjectDataSource ID="NewsDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateNewsDataAdapter" SelectMethod="SelectPosts" runat="server">
	<SelectParameters>
		<asp:Parameter Name="startRowIndex" DefaultValue="0"/>
		<asp:Parameter Name="maximumRows" DefaultValue='<%$ SiteSetting: Home News Post Count, 4 %>'/>
	</SelectParameters>
</asp:ObjectDataSource>
<asp:ListView ID="NewsPosts" DataSourceID="NewsDataSource" runat="server">
	<LayoutTemplate>
		<div class="content-panel panel panel-default">
			<asp:ObjectDataSource ID="NewsBlogDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateNewsDataAdapter" SelectMethod="Select" runat="server" />
			<div class="panel-heading">
				<asp:Repeater DataSourceID="NewsBlogDataSource" runat="server">
					<ItemTemplate>
						<asp:HyperLink CssClass="pull-right" NavigateUrl='<%#: Eval("ApplicationPath.AbsolutePath") %>' Text='<%$ Snippet: Home All News Link Text, All News %>' ToolTip='<%$ Snippet: Home All News Link Text, All News %>' runat="server" />
						<h4>
							<asp:HyperLink CssClass="feed-icon fa fa-rss-square" NavigateUrl='<%#: Eval("FeedPath.AbsolutePath") %>' ToolTip='<%$ Snippet: Home News Feed Subscribe Tooltip Label, Subscribe to News %>' runat="server">
								<span class="sr-only"><asp:Literal Text='<%$ Snippet: Home News Feed Subscribe Tooltip Label, Subscribe to News %>' runat="server" /></span>
							</asp:HyperLink>
							<%#: Eval("Title") %>
						</h4>
					</ItemTemplate>
				</asp:Repeater>
			</div>
			<ul class="list-group">
				<li id="itemPlaceholder" runat="server" />
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li class="list-group-item" runat="server">
			<h4 class="list-group-item-heading">
				<asp:HyperLink NavigateUrl='<%#: Eval("ApplicationPath.AppRelativePath") %>' ToolTip=<%#: Eval("Title") %> runat="server"><%#: Eval("Title") %></asp:HyperLink>
			</h4>
			<div class="content-metadata">
				<abbr class="posttime"><%#: Eval("PublishDate", "{0:r}") %></abbr>
				<asp:Label runat="server" Visible='<%# (((BlogCommentPolicy)Eval("CommentPolicy")) != BlogCommentPolicy.None) %>'>
					&ndash;
					<asp:HyperLink NavigateUrl='<%#: string.Format("{0}#comments", Eval("ApplicationPath.AbsolutePath")) %>' ToolTip= <%#: Eval("CommentCount") %> runat="server">
						<span class="fa fa-comment" aria-hidden="true"></span> <%#: Eval("CommentCount") %>
					</asp:HyperLink>
				</asp:Label>
			</div>
			<div class="list-group-item-text">
				<div class="summary">
					<%#: Eval("Summary") %>
				</div>
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>
