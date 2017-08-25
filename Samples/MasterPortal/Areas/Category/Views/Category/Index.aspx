<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Category.ViewModels.CategoryViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Title" runat="server"><%: Model.Title %></asp:Content>

<asp:Content ContentPlaceHolderID="Head" runat="server"></asp:Content>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server">
	<div class="page-heading">
		<div class="container">
			<ul class="breadcrumb">
				<% foreach (var node in Html.SiteMapPath().Where(e => e.Item2 != SiteMapNodeType.Current)) { %>
					<li>
						<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
				<li class="active">
					<%: Model.Number %>
				</li>
			</ul>
			<div class="page-header">
				<div class="pull-right">
					<div role="toolbar" class="btn-toolbar">
						<div class="btn-group">
							<a href="javascript:window.print()" class="btn btn-default btn-sm" title="<%: Html.SnippetLiteral("Category - Print Button Label", ResourceManager.GetString("Print_Defaulttext")) %>"><i class="fa fa-print"></i> <%: Html.TextSnippet("Category - Print Button Label", defaultValue: ResourceManager.GetString("Print_Defaulttext"), tagName: "span") %></a>
						</div>
					</div>
				</div>
				<h1><%: Model.Title %></h1>
			</div>
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="category">
		<div class="row">
			<div class="col-sm-6">
				<div class="content-panel panel panel-default knowledge-article-related">
					<div class="panel-heading">
						<h4>
							<span class="fa fa-share-square-o" aria-hidden="true"></span>
							<%: Html.TextSnippet("Category - Related Articles Heading", defaultValue: ResourceManager.GetString("Related_Articles_Defaulttext"), tagName: "span") %>
						</h4>
					</div>
					<div class="list-group">
						<% if (Model.RelatedArticles.Any()) { %>
							<% foreach (var relatedArticle in Model.RelatedArticles) { %>
								<a class="list-group-item" href="<%: relatedArticle.Url %>" title="<%: relatedArticle.Title %>"><%: relatedArticle.Title %></a>
							<% } %>
						<% } else { %>
							<span class="list-group-item text-muted">none</span>
						<% } %>
					</div>
				</div>
			</div>
			<div class="col-sm-6">
				<div class="content-panel panel panel-default child-category">
					<div class="panel-heading">
						<h4>
							<span class="fa fa-share-square-o" aria-hidden="true"></span>
							<%: Html.TextSnippet("Category - Child Categories Heading", defaultValue: ResourceManager.GetString("Child_Categories_Defaulttext"), tagName: "span") %>
						</h4>
					</div>
					<div class="list-group">
						<% if (Model.ChildCategories.Any()) { %>
							<% foreach (var childCategory in Model.ChildCategories) { %>
								<% if (!string.IsNullOrWhiteSpace(childCategory.Url)) { %>
									<a class="list-group-item" href="<%: childCategory.Url %>" title="<%: childCategory.Title %>"><%: childCategory.Title %></a>
								<% } else { %>
									<span class="list-group-item"><%: childCategory.Title %></span>
								<% } %>
							<% } %>
						<% } else { %>
							<span class="list-group-item text-muted">none</span>
						<% } %>
					</div>
				</div>
			</div>
		</div>
	</div>
	<div class="page-metadata clearfix">
		<%: Html.Snippet("Social Share Widget Code Page Bottom") %>
	</div>
</asp:Content>
