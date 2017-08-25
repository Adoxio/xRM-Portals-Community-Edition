<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.KnowledgeBase.ViewModels.ArticleViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Title" runat="server"><%: Model.Title %></asp:Content>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/KnowledgeBase/css/knowledgebase.css") %>">
</asp:Content>

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
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="kb-article">
		<div class="kb-article-content">
			<%: Model.Content %>
		</div>
		<% if (Model.RelatedArticles.Any()) { %>
			<div class="content-panel panel panel-default kb-article-related">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-share-square-o" aria-hidden="true"></span>
						<%: Html.TextSnippet("Knowledge Base Related Articles Heading", defaultValue: ResourceManager.GetString("Related_Articles_Defaulttext"), tagName: "span") %>
					</h4>
				</div>
				<div class="list-group">
					<% foreach (var relatedArticle in Model.RelatedArticles) { %>
						<a class="list-group-item" href="<%: relatedArticle.Url %>" title="<%: relatedArticle.Title %>"><%: relatedArticle.Title %></a>
					<% } %>
				</div>
			</div>
		<% } %>
	</div>
	
	<div class="page-metadata clearfix">
		<%: Html.Snippet("Social Share Widget Code Page Bottom") %>
	</div>
</asp:Content>
