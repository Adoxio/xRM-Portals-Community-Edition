<%@ Page Language="C#" MasterPageFile="../MasterPages/Forums.master" AutoEventWireup="true" CodeBehind="Forums.aspx.cs" Inherits="Site.Areas.Forums.Pages.Forums" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Forums" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Areas.Forums" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Register TagPrefix="site" TagName="ChildNavigation" Src="~/Controls/ChildNavigation.ascx" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>

	<asp:ObjectDataSource ID="ForumsDataSource" TypeName="Adxstudio.Xrm.Forums.IForumAggregationDataAdapter" OnObjectCreating="CreateForumAggregationDataAdapter" SelectMethod="SelectForums" runat="server" />
	<asp:ListView DataSourceID="ForumsDataSource" runat="server">
		<LayoutTemplate>
			<table class="table forums table-striped table-fluid">
				<thead>
					<tr>
						<th class="name">
							<adx:Snippet SnippetName="Forum Name Heading" DefaultText="<%$ ResourceManager:Forum_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="last-post">
							<adx:Snippet SnippetName="Forum Last Post Heading" DefaultText="<%$ ResourceManager:Last_Post_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="count">
							<adx:Snippet SnippetName="Forum Thread Count Heading" DefaultText="<%$ ResourceManager:Threads_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="count">
							<adx:Snippet SnippetName="Forum Post Count Heading" DefaultText="<%$ ResourceManager:Posts_DefaultText %>" EditType="text" runat="server"/>
						</th>
					</tr>
				</thead>
				<tbody>
					<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
				</tbody>
			</table>
		</LayoutTemplate>
		<ItemTemplate>
			<tr>
				<td class="name">
					<h3><asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Name") %>' ToolTip='<%# HttpUtility.HtmlDecode(Eval("Name").ToString()) %>' runat="server"/></h3>
					<p><%# Eval("Description") %></p>
				</td>
				<td class="last-post">
					<asp:Panel CssClass="media" Visible='<%# Eval("LatestPost") != null %>' runat="server">
						<div class="media-left">
							<asp:HyperLink CssClass="author-link" NavigateUrl='<%# Url.AuthorUrl(Eval("LatestPost.Author") as IForumAuthor) %>' ToolTip='<%# Eval("LatestPost.Author.DisplayName") %>' aria-label='<%#: string.Format(ResourceManager.GetString("User_Avatar_Label"), Eval("LatestPost.Author.DisplayName")) %>' runat="server">
								<asp:Image CssClass="author-img" ImageUrl='<%# Url.UserImageUrl(Eval("LatestPost.Author") as IForumAuthor, 40) %>' AlternateText='<%# Eval("LatestPost.Author.DisplayName") %>' runat="server"/>
							</asp:HyperLink>
						</div>
						<div class="media-body">
							<div class="last-post-info small">
								<asp:HyperLink CssClass="author-link" NavigateUrl='<%#: Url.AuthorUrl(Eval("LatestPost.Author") as IForumAuthor) %>' Text='<%#: Eval("LatestPost.Author.DisplayName") ?? "" %>' ToolTip='<%# Eval("LatestPost.Author.DisplayName") %>' runat="server" />
								<div class="postedon">
									<abbr class="timeago">
										<%#: ForumHelpers.PostedOn(Eval("LatestPost") as IForumPostInfo, "r") %>
									</abbr>
								</div>
							</div>
						</div>
					</asp:Panel>
				</td>
				<td class="count"><%#: Eval("ThreadCount") %></td>
				<td class="count"><%#: Eval("PostCount") %></td>
			</tr>
		</ItemTemplate>
	</asp:ListView>
	
	<site:ChildNavigation Exclude="adx_communityforum" runat="server"/>
</asp:Content>