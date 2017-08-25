<%@ Page Language="C#" MasterPageFile="../MasterPages/Blogs.master" AutoEventWireup="true" CodeBehind="BlogArchive.aspx.cs" Inherits="Site.Areas.Blogs.Pages.BlogArchive" ValidateRequest="false" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Blogs" %>
<%@ Import Namespace="System.Web.Security.AntiXss" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Import Namespace="Adxstudio.Xrm.Web" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<asp:ObjectDataSource ID="BlogHeaderDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateBlogDataAdapter" SelectMethod="Select" runat="server" />
	<asp:ListView DataSourceID="BlogHeaderDataSource" runat="server">
		<LayoutTemplate>
			<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
		</LayoutTemplate>
		<ItemTemplate>
			<crm:CrmEntityDataSource ID="CurrentEntity" DataItem='<%# Eval("Entity") %>' runat="server" />
			<div class="page-header">
				<h1>
					<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_name" EditType="text" runat="server" />
					<asp:SiteMapDataSource ID="CurrentNode" StartFromCurrentNode="True" ShowStartingNode="True" runat="server" />
					<asp:ListView DataSourceID="CurrentNode" runat="server">
						<LayoutTemplate>
							<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
						</LayoutTemplate>
						<ItemTemplate>
							<small><%#: Eval("Title") %></small>
						</ItemTemplate>
					</asp:ListView>
				</h1>
			</div>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<asp:ObjectDataSource ID="PostDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateBlogDataAdapter" SelectMethod="SelectPosts" SelectCountMethod="SelectPostCount" EnablePaging="True" runat="server" />
	<asp:ListView ID="Posts" DataSourceID="PostDataSource" runat="server">
		<LayoutTemplate>
			<ol class="nav">
				<li id="itemPlaceholder" runat="server" />
			</ol>
			<adx:UnorderedListDataPager PageSize="10" PagedControlID="Posts" CssClass="pager" QueryStringField="page" runat="server">
				<Fields>
					<adx:ListItemNextPreviousPagerField ListItemCssClass="previous" ShowNextPageButton="False" PreviousPageText='<%$ Snippet: Blog Post Previous Page Text, Newer_ButtonText %>' />
					<adx:ListItemNextPreviousPagerField ListItemCssClass="next" ShowPreviousPageButton="False" NextPageText='<%$ Snippet: Blog Post Next Page Text, Older_ButtonText %>' />
				</Fields>
			</adx:UnorderedListDataPager>
		</LayoutTemplate>
		<ItemTemplate>
			<li class='<%# "blog-post" + ((bool)Eval("IsPublished") ? "" : " unpublished") %>' runat="server">
				<crm:CrmEntityDataSource ID="Post" DataItem='<%# Eval("Entity") %>' runat="server" />
				<asp:HyperLink CssClass="user-avatar" NavigateUrl='<%# Url.AuthorUrl(Eval("Author") as IBlogAuthor) %>' ImageUrl='<%# Url.UserImageUrl(Eval("Author.EmailAddress")) %>' ToolTip='<%# Eval("Author.Name")  ?? "" %>' runat="server"/>
				<h3 class="title">
					<asp:HyperLink NavigateUrl='<%#: Eval("ApplicationPath.AppRelativePath") %>' runat="server">
						<adx:Property DataSourceID="Post" PropertyName="adx_name" Literal="True" runat="server" />
					</asp:HyperLink>
				</h3>
				<div class="metadata">
					<asp:Label Visible='<%# !(bool)Eval("IsPublished") %>' CssClass="label label-info" Text='<%$ Snippet: Unpublished Post Label, Unpublished_DefaultText %>' runat="server"></asp:Label>
					<asp:HyperLink NavigateUrl='<%# Url.AuthorUrl(Eval("Author") as IBlogAuthor) %>' Text='<%#: Eval("Author.Name") %>' ToolTip='<%# Eval("Author.Name") %>' runat="server" />
					&ndash;
					<abbr class="timeago"><%#: Eval("PublishDate", "{0:r}") %></abbr>
					&ndash;
					<asp:HyperLink NavigateUrl='<%#: string.Format("{0}#comments", Eval("ApplicationPath.AbsolutePath")) %>' ToolTip='<%#: Eval("CommentCount") %>' runat="server">
						<span class="fa fa-comment" aria-hidden="true"></span> <%#: Eval("CommentCount") %>
					</asp:HyperLink>
				</div>
				<div>
					<asp:Panel Visible='<%# Eval("HasExcerpt") %>' runat="server">
						<adx:Property DataSourceID="Post" PropertyName="adx_summary" Editable="False" EditType="html" runat="server" />
						<p>
                            <asp:HyperLink NavigateUrl='<%#: string.Format("{0}#extended", Eval("ApplicationPath.AbsolutePath")) %>' Text='<%$ Snippet: Blog Post Extended Content Link Text, ContinueReading_ButtonText %>' ToolTip='<%$ Snippet: Blog Post Extended Content Link Text, ContinueReading_ButtonText %>' runat="server" />
						</p>
					</asp:Panel>
					<asp:Panel Visible='<%# !(bool)Eval("HasExcerpt") %>' runat="server">
						<adx:Property DataSourceID="Post" PropertyName="adx_copy" Editable="False" EditType="html" CssClass="page-copy" runat="server" />
					</asp:Panel>
				</div>
				<div>
					<asp:ListView runat="server" DataSource='<%# Eval("Tags") %>'>
						<LayoutTemplate>
							<ul class="tags">
								<li id="itemPlaceholder" runat="server" />
							</ul>
						</LayoutTemplate>
						<ItemTemplate>
							<li runat="server">
								<asp:HyperLink CssClass="btn btn-default btn-xs" NavigateUrl='<%#: Eval("ApplicationPath.AppRelativePath") %>' ToolTip='<%# Eval("Name") %>' runat="server">
									<span class="fa fa-tag" aria-hidden="true"></span>
									<%#: Eval("Name") %>
								</asp:HyperLink>
							</li>
						</ItemTemplate>
					</asp:ListView>
				</div>
			</li>
		</ItemTemplate>
		<EmptyDataTemplate>
			<div class="search-results">
				<asp:Repeater DataMember="Info" DataSourceID="SearchData" runat="server">
					<ItemTemplate>
						<asp:Panel Visible='<%# ((int)Eval("Count")) > 0 %>' runat="server">
							<h2><%# string.Format(ResourceManager.GetString("Search_Results_Format_String"), Eval("FirstResultNumber"), Eval("LastResultNumber"), Eval("ApproximateTotalHits")) %><em class="querytext"><%#: (Eval("[Query]") ?? string.Empty) %></em></h2>					
							<asp:ListView DataSourceID="SearchData" ID="SearchResults" runat="server">
								<LayoutTemplate>
									<ul>
										<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
									</ul>
								</LayoutTemplate>
								<ItemTemplate>
									<li runat="server">
										<h3><asp:HyperLink Text='<%#: Eval("Title") %>' NavigateUrl='<%#: Eval("Url") %>' ToolTip='<%#: Eval("Title") %>' runat="server" /></h3>
										<p class="fragment"><%# Eval(SafeHtml.SafeHtmSanitizer.GetSafeHtml("Fragment")) %></p>
										<asp:HyperLink Text='<%#: GetDisplayUrl(Eval("Url")) %>' NavigateUrl='<%#: Eval("Url") %>' runat="server" />
									</li>
								</ItemTemplate>
							</asp:ListView>
					
							<adx:UnorderedListDataPager ID="SearchResultPager" CssClass="pagination" PagedControlID="SearchResults" QueryStringField="page" PageSize="10" runat="server">
									<Fields>
										<adx:ListItemNextPreviousPagerField ShowNextPageButton="false" ShowFirstPageButton="True" FirstPageText="&laquo;" PreviousPageText="&lsaquo;" />
										<adx:ListItemNumericPagerField ButtonCount="10" PreviousPageText="&hellip;" NextPageText="&hellip;" />
										<adx:ListItemNextPreviousPagerField ShowPreviousPageButton="false" ShowLastPageButton="True" LastPageText="&raquo;" NextPageText="&rsaquo;" />
									</Fields>
								</adx:UnorderedListDataPager>
						</asp:Panel>
						<asp:Panel Visible='<%# ((int)Eval("Count")) == 0 && ((int)Eval("PageNumber")) == 1 %>' runat="server">							
							<p><%# ResourceManager.GetString("Search_No_Results_Found") %> <em class="querytext"><%#: (Eval("[Query]") ?? string.Empty) %></em></p>
						</asp:Panel>
					</ItemTemplate>
				</asp:Repeater>
			</div>
		</EmptyDataTemplate>
	</asp:ListView>
	<adx:SearchDataSource ID="SearchData" LogicalNames="adx_blog,adx_blogpost" OnSelected="SearchData_OnSelected" runat="server">
		<SelectParameters>
			<asp:Parameter Name="Query" DefaultValue='<%$ CrmSiteMap: Current, Eval=Title %>' />
			<asp:QueryStringParameter Name="PageNumber" QueryStringField="page" />
			<asp:Parameter Name="PageSize" DefaultValue="10" />
		</SelectParameters>
	</adx:SearchDataSource>
</asp:Content>
