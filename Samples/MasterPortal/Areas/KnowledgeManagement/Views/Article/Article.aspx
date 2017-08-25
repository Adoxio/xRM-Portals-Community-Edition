<%@ Page Language="C#" MasterPageFile="../Shared/Article.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.KnowledgeManagement.ViewModels.ArticleViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Core.Flighting" %>
<%@ Import Namespace="Site.Areas.KnowledgeManagement.Controllers" %>
<script runat="server">
	private string caseUrl = ArticleController.GetPortalUri("Create Case");
</script>
<asp:Content ContentPlaceHolderID="Title" runat="server"><%: Model.KnowledgeArticle.Title %></asp:Content>

<asp:Content ContentPlaceHolderID="Head" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server">
			<ul class="breadcrumb">
				<%="" %><!--dummy expression to resolve CS0103 error The name '__o' does not exist in the current context-->
				<% foreach (var node in Html.SiteMapPath().Where(e => e.Item2 != SiteMapNodeType.Current)) { %>
					<li>
						<a href="<%: node.Item1.Url %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
				<li class="active">
					<%: Model.KnowledgeArticle.ArticlePublicNumber %>
				</li>
			</ul>
			<div class="page-header">
				<div class="pull-right">
					<div role="toolbar" class="btn-toolbar">
						<div class="btn-group">
							<a href="javascript:window.print()" class="btn btn-default btn-sm"><i class="fa fa-print"></i> <%: Html.TextSnippet("Knowledge Management - Print Button Label", defaultValue: ResourceManager.GetString("Print_Defaulttext"), tagName: "span") %></a>
						</div>
					</div>
				</div>
				<h1><%: Model.KnowledgeArticle.Title %></h1>
				<div id="foundmyanswer-rating" class="row">
					<% if (Html.ViewContext.HttpContext.Request.UrlReferrer != null)
						{
							if (!string.IsNullOrEmpty(caseUrl))
							{
								if (Html.ViewContext.HttpContext.Request.UrlReferrer.AbsoluteUri.Contains(caseUrl))
									{ %>
										<div id="found-my-answer">
											<label id="foundmyanswerlabel" style="float:left;padding:4px;margin-left:16px;margin-top:8px;display:none"><%: ResourceManager.GetString("Marked_As_Your_Answer") %></label>
											<button type="button" id="foundmyanswerbutton" class="btn btn-success btn-xs" style="float:left;padding:4px;margin-left:16px" onclick="postcaseDeflection('<%: Url.Action("CaseDeflectionCreate", "Article", new {id = Model.KnowledgeArticle.Id, isRatingEnabled = Model.KnowledgeArticle.IsRatingEnabled, searchText = Html.ViewContext.HttpContext.Request.QueryString["caseTitle"]}) %>')" ><span class='fa fa-check'></span> <%: ResourceManager.GetString("Found_My_Answer") %></button>
										</div>
								<%  }
								} %>
						<%	} %>
				</div>
			</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<% if (Model.RelatedArticles != null && Model.RelatedArticles.Any()) { %>
	<div class="col-lg-8">
<% }else { %>
	<div class="col-lg-12">
<% } %>
	<div class="knowledge-article">
		<div class="row">
				<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) && Model.KnowledgeArticle.IsRatingEnabled) { %>
						<div id="article-rating" class="col-sm-6"><% Html.RenderPartial("Rating", Model.KnowledgeArticle); %></div>
				<% } else { %>
						<div class="col-sm-6"></div>
				<% } %>
			
			<div id="found-my-answer-bottom-comments" class="col-sm-6" ><adx:Snippet SnippetName="Knowledge Management - Knowledge Article Views Heading" DefaultText="<%$ ResourceManager:Knowledge_Article_Views_DefaultText %>" HtmlTag="Span" runat="server"/><span id="viewcount-value" data-url='<%: Url.Action("GetArticleViewCount", "Article", new { id = this.Model.KnowledgeArticle.Id }) %>'></span>
				<% if (Html.ViewContext.HttpContext.Request.UrlReferrer != null)
					{
						if (!string.IsNullOrEmpty(caseUrl))
							{
								if (Html.ViewContext.HttpContext.Request.UrlReferrer.AbsoluteUri.Contains(caseUrl))
								{ %>
									<label id="foundmyanswerlabelbottom" style="display:none;margin-left:16px" ><%: ResourceManager.GetString("Marked_As_Your_Answer") %></label>
									<button type="button" id="foundmyanswerbuttonbottom" class="btn btn-success btn-xs" style="padding:4px;margin-left:16px;margin-bottom:10px" onclick="postcaseDeflection('<%: Url.Action("CaseDeflectionCreate", "Article", new {id = Model.KnowledgeArticle.Id, isRatingEnabled = Model.KnowledgeArticle.IsRatingEnabled, searchText = Html.ViewContext.HttpContext.Request.QueryString["caseTitle"]}) %>')" ><span class='fa fa-check'></span> <%: ResourceManager.GetString("Found_My_Answer") %></button>
							<%  }
						} %>
				<%	} %>
			</div>
		</div>

		<div class="knowledge-article-content">
			<%: @Html.Raw(Model.KnowledgeArticle.Content) %>
		</div>
	</div>

	<% if (!string.IsNullOrEmpty(Model.KnowledgeArticle.Keywords) || (Model.RelatedProducts != null && Model.RelatedProducts.Any())) { %>
		<div class="panel panel-default">
			<div class="panel-body">
				<% if (!string.IsNullOrEmpty(Model.KnowledgeArticle.Keywords)){ %>
				<div class="knowledge-article-keywords" style="<%: Model.RelatedProducts != null && Model.RelatedProducts.Any() ? "margin-bottom: 10px;" : "margin: 0;" %>">
					<span><%: Html.TextSnippet("Knowledge Management - Keywords Heading", defaultValue: ResourceManager.GetString("Keywords_Defaulttext"), tagName: "span") %> <%: Model.KnowledgeArticle.Keywords %></span>
				</div>
				<% } %>

			<% if (Model.RelatedProducts != null && Model.RelatedProducts.Any()) { %>
				<div>
					<%: Html.TextSnippet("Knowledge Management - Related Products Heading", defaultValue: ResourceManager.GetString("Related_Products_Defaulttext"), tagName: "span") %>
					<span><%: string.Join(", ", Model.RelatedProducts.Where(rp => string.IsNullOrWhiteSpace(rp.Url)).Select(v => v.Name)) %></span>
				</div>
			<% } %>
			</div>
		</div>
	<% } %>
</div>

	<script type="text/javascript">
			$(function () {
				var url = $("#viewcount-increment").attr("data-url");
				shell.ajaxSafePost({
				type: "POST",
				url: url,
				success: function (result) {
					if (document.getElementById("foundmyanswerbutton") != null) {
						if (document.getElementById("post-rating") != null) {
							document.getElementById("post-rating").style.marginTop = "13px";
						}
					}
					if (document.getElementById("foundmyanswerbuttonbottom") != null) {
						document.getElementById("found-my-answer-bottom-comments").style.display = "inline";
					}
					$("#viewcount-increment").html(result);
				},
					error: function(error) { console.log(error); }
				});

				var viewcountUrl = $("#viewcount-value").attr("data-url");
				$.ajax({
					url: viewcountUrl,
					type: 'GET',
					success: function (result) {
						$("#viewcount-value").html(result);
					},
					error: function (error) { console.log(error); }
				});

				return false;
			});

	function postcaseDeflection(url) {
		shell.ajaxSafePost({
			type: 'POST',
			url: url,
			success: function (result) {
				document.getElementById("foundmyanswerlabel").style.display = "inline";
				document.getElementById("foundmyanswerlabelbottom").style.display = "inline";
				document.getElementById("foundmyanswerbutton").style.display = "none";
				document.getElementById("foundmyanswerbuttonbottom").style.display = "none";
				document.getElementById("found-my-answer-bottom-comments").style.display = "block";
			}
		});
	}
	</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarContent" Runat="Server">
	<% if (Model.RelatedArticles != null && Model.RelatedArticles.Any()) { %>
		<div class="col-lg-4">
				<div id="RelatedArticles" class="content-panel panel panel-default knowledge-article-related hidden-print">
					<div class="panel-heading">
						<h4>
							<%: Html.TextSnippet("Knowledge Management - Related Articles Heading", defaultValue: ResourceManager.GetString("Related_Articles_Defaulttext"), tagName: "span") %>
							<span> (<%: Model.RelatedArticles.Count() %>)</span>
						</h4>
					</div>
					<div runat="server" class="list-group">
						<%="" %><!--dummy expression to resolve CS0103 error The name '__o' does not exist in the current context-->
						<% var count = 0;%>	
						<% foreach (var relatedArticle in Model.RelatedArticles) { %>
							<%count++;%>
							<%if (count <= 5) {%>
									<a class="list-group-item" title="<%: relatedArticle.Title %>" href="<%: relatedArticle.Url %>"><%: relatedArticle.Title %></a>
							<% } else {%>
									<a class="list-group-item" style="display: none;" title="<%: relatedArticle.Title %>" href="<%: relatedArticle.Url %>"><%: relatedArticle.Title %></a>
							<% } %>
						<% } %>
					</div>
					<% if (Model.RelatedArticles.Count() > 5) { %>
						<a class="panel-footer" id="showMoreArticleButton" data-parent="#RelatedArticles" role="button" href="#article" style="display: block;"> <%: Html.TextSnippet("Search/Facet/More", defaultValue: ResourceManager.GetString("Facet_Show_More"), tagName: "span") %> </a>
						<a class="panel-footer" id="showLessArticleButton" data-parent="#RelatedArticles" role="button" href="#article" style="display: none;"> <%: Html.TextSnippet("Search/Facet/Less", defaultValue: ResourceManager.GetString("Facet_Show_Less"), tagName: "span") %> </a>
				<% } %>
				</div>
			</div>
	<% } %>
</asp:Content>

<asp:Content ContentPlaceHolderID="Notes" Runat="Server">
		<% if (Model.RelatedNotes != null && Model.RelatedNotes.Any()) { %>
		<div id="RelatedNotes" class="content-panel panel panel-default knowledge-article-related hidden-print">
			<div class="panel-heading">
				<h4>
					<%: Html.TextSnippet("Knowledge Management - Related Attachments Heading", defaultValue: ResourceManager.GetString("Related_Attachments_Defaulttext"), tagName: "span") %>
					<span> (<%: Model.RelatedNotes.Count() %>)</span>
				</h4>
			</div>
			<ul runat="server" id="relatedNotesShown" class="list-group">
				<%="" %><!--dummy expression to resolve CS0103 error The name '__o' does not exist in the current context-->
						<% var count = 0;%>
							<% foreach (var relatedNote in Model.RelatedNotes) { %>
								<%count++;%>
								<%if (count <= 5) {%>
									<li class="list-group-item">
										<p ><%: relatedNote.Description %></p>
										<% if (!string.IsNullOrEmpty(relatedNote.FileUrl)) { %>
												<a href="<%: relatedNote.FileUrl %>"><span class="glyphicon glyphicon-file" aria-hidden="true"></span>&nbsp;<%: relatedNote.FileName %></a>
										 <% } %>
									</li>
								<% } else {%>
									<li class="list-group-item" style="display: none;">
										<p ><%: relatedNote.Description %></p>
										<% if (!string.IsNullOrEmpty(relatedNote.FileUrl)) { %>
												<a href="<%: relatedNote.FileUrl %>"><span class="glyphicon glyphicon-file" aria-hidden="true"></span>&nbsp;<%: relatedNote.FileName %></a>
										 <% } %>
									</li>
								<% } %>
						<% } %>
				</ul>
			<% if(Model.RelatedNotes.Count()>5) { %>
				<a class="panel-footer" id="showMoreNotesButton" data-parent="#RelatedNotes" role="button" href="#notes"  style="display: block"> <%: Html.TextSnippet("Search/Facet/More", defaultValue: ResourceManager.GetString("Facet_Show_More"), tagName: "span") %> </a>
				<a class="panel-footer" id="showLessNotesButton" data-parent="#RelatedNotes" role="button" href="#notes" style="display: none"> <%: Html.TextSnippet("Search/Facet/Less", defaultValue: ResourceManager.GetString("Facet_Show_Less"), tagName: "span") %> </a>
			<% } %>
	<% } %>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="Comments" Runat="Server">
		<div id="comments">
	<% if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
		{ %>
		<% Html.RenderPartial("Comments", Model.Comments); %>
	<% } %>
	</div>

	<div class="page-metadata clearfix">
		<%: Html.Snippet("Social Share Widget Code Page Bottom") %>
	</div>

	<div id="viewcount-increment" data-url='<%: Url.Action("IncrementViewCount", "Article", new { id = Model.KnowledgeArticle.Id, referrer = this.Request.UrlReferrer }) %>'>
	</div>
</asp:Content>