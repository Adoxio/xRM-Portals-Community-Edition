<%@ Page Language="C#" MasterPageFile="../Shared/Ideas.master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Ideas.ViewModels.IdeasViewModel>" ValidateRequest="false"  %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Microsoft.Xrm.Portal.Configuration" %>
<%@ Import Namespace="Adxstudio.Xrm.Cms" %>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<% Html.RenderPartial("Breadcrumbs"); %>
	<div class="page-header">
		<h1><%: Html.TextAttribute("adx_name") %></h1>
	</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<div class="page-copy">
		<%= Html.HtmlAttribute("adx_copy") %>
	</div>
	<ul class="ideas list-unstyled">
		<% foreach (var ideaForum in Model.IdeaForums)
		 { %>
		<li>
			<h3><a href= "<%: ideaForum.Url %>" title= "<%: ideaForum.Title %>" > <%: ideaForum.Title %> </a></h3>
			<div class="bottom-break"><%= ideaForum.Summary %></div>
		</li>
		<% } %>
	</ul>
</asp:Content>
