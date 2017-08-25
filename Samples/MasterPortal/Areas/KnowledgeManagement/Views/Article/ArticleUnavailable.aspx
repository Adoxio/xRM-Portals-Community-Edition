<%@ Page Language="C#" MasterPageFile="../Shared/Article.master" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<asp:Content ContentPlaceHolderID="Title" runat="server">
	<%: ResourceManager.GetString("Knowledge_Article_Unavailable") %>
</asp:Content>
<asp:Content ContentPlaceHolderID="ContentHeader" runat="server">
	<ul class="breadcrumb">
		<% foreach (var node in Html.SiteMapPath().Where(e => e.Item2 != SiteMapNodeType.Current))
			{ %>
				<li>
					<a href="<%: node.Item1.Url %>"><%: node.Item1.Title %></a>
				</li>
		<% } %>

		<li class="active">
			<%: Html.TextSnippet("Knowledge Management - Article Unavailable", defaultValue: ResourceManager.GetString("Knowledge_Article_Unavailable"), tagName: "span")  %>
		</li>

	</ul>
	<div class="page-header">
		<h1><%: Html.TextSnippet("Knowledge Management - Article Unavailable", defaultValue: ResourceManager.GetString("Knowledge_Article_Unavailable"), tagName: "span")  %></h1>
		
	</div>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="row">		<div class="col-md-12">			<div class="alert alert-block alert-danger">
				<%: Html.TextSnippet("KnowledgeManagement/ArticleUnavailableMessage", defaultValue: ResourceManager.GetString("KnowledgeManagement_Article_Unavailable_Message"), tagName: "span")  %>
			</div>		</div>	</div>
</asp:Content>