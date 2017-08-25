<%@ Page Language="C#" MasterPageFile="../MasterPages/Blogs.master" AutoEventWireup="true" CodeBehind="Blogs.aspx.cs" Inherits="Site.Areas.Blogs.Pages.Blogs" ValidateRequest="false" %>
<%@ OutputCache CacheProfile="User" %>


<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<asp:ObjectDataSource ID="BlogDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogDataAdapter" OnObjectCreating="CreateBlogAggregationDataAdapter" SelectMethod="Select" runat="server" />
	<div class="page-header">
		<asp:ListView DataSourceID="BlogDataSource" runat="server">
			<LayoutTemplate>
				<div class="pull-right">
					<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
				</div>
			</LayoutTemplate>
			<ItemTemplate>
				<asp:HyperLink CssClass="feed-icon fa fa-rss-square" NavigateUrl='<%#: Eval("FeedPath.AbsolutePath") %>' ToolTip='<%$ Snippet: Blog Subscribe Heading, Subscribe_DefaultText %>' runat="server" />
			</ItemTemplate>
		</asp:ListView>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<asp:ObjectDataSource ID="BlogAggregationDataSource" TypeName="Adxstudio.Xrm.Blogs.IBlogAggregationDataAdapter" OnObjectCreating="CreateBlogAggregationDataAdapter" SelectMethod="SelectBlogs" runat="server" />
	<asp:ListView DataSourceID="BlogAggregationDataSource" runat="server">
		<LayoutTemplate>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4><adx:Snippet SnippetName="Blogs Heading" DefaultText="<%$ ResourceManager:Blogs_DefaultText %>" runat="server" /></h4>
				</div>
				<div class="list-group">
					<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
				</div>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<asp:HyperLink CssClass="list-group-item" NavigateUrl='<%#: Eval("ApplicationPath.AbsolutePath") %>' Text='<%#: Eval("Title")  %>' ToolTip='<%# Eval("Title") %>' runat="server"/>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>