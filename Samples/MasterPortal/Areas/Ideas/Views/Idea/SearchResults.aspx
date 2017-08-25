<%@ Page Language="C#" MasterPageFile="../Shared/Ideas.master" Inherits="System.Web.Mvc.ViewPage<Adxstudio.Xrm.Search.ICrmEntitySearchResultPage>" ValidateRequest="false"  %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Data" %>
<%@ Import Namespace="Microsoft.Xrm.Portal.Web" %>
<%@ Import Namespace="Adxstudio.Xrm.Search" %>
<%@ Import Namespace="Adxstudio.Xrm.Web" %>

<asp:Content runat="server" ContentPlaceHolderID="Title"><%: Html.SnippetLiteral("Idea Search Title", ResourceManager.GetString("Search_Ideas_Defaulttext")) %></asp:Content>
<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<ul class="breadcrumb">
		<% Html.RenderPartial("SiteMapPath"); %>
		<li class="active"><%: Html.SnippetLiteral("Idea Search Title", ResourceManager.GetString("Search_Ideas_Defaulttext")) %></li>
	</ul>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="search-results">
		<% if (Model.Any()) { %>		
			<h2><%# string.Format(ResourceManager.GetString("Search_Results_Format_String"),Model.First().ResultNumber, Model.Last().ResultNumber, Model.ApproximateTotalHits) %> <em class="querytext"><%= HttpUtility.HtmlEncode(Request["q"] ?? String.Empty) %></em></h2>
			<ul>
				<% foreach (var result in Model) { %>
					<li>
						<h3><a href="<%= result.Url %>" title="<%: result.Title %>"><%: result.Title %></a></h3>
						<p class="fragment"><%= SafeHtml.SafeHtmSanitizer.GetSafeHtml(result.Fragment) %></p>
						<a href="<%= result.Url %>" title="<%: new UrlBuilder(result.Url.ToString()) %>"><%: new UrlBuilder(result.Url.ToString()) %></a>
					</li>
				<% } %>
			</ul>
		<% Html.RenderPartial("Pagination", new PaginatedList<ICrmEntitySearchResult>(Model.PageNumber, Model.ApproximateTotalHits, Model.AsEnumerable())); %>
		<% } else if (Model.PageNumber == 1) { %>
			<h2><%: ResourceManager.GetString("Search_No_Results_Found") %> <em class="querytext"><%=  HttpUtility.HtmlEncode(Request["q"] ?? String.Empty) %></em></h2>
		<% } %>
	</div>
</asp:Content>