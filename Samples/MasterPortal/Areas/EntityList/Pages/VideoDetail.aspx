<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="VideoDetail.aspx.cs" Inherits="Site.Areas.EntityList.Pages.VideoDetail" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Areas.EntityList.Helpers" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<asp:PlaceHolder ID="VideoHead" Visible="False" runat="server">
		<%: Html.PackageLink(Url, Video.Title) %>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
	<asp:PlaceHolder ID="VideoBreadcrumbs" Visible="False" runat="server">
		<ul class="breadcrumb">
			<% foreach (var node in Html.SiteMapPath()) { %>
				<% if (node.Item2 == SiteMapNodeType.Current) { %>
					<li class="active"><%: Video.Title %></li>
				<% } else { %>
					<li>
						<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
			<% } %>
		</ul>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PageBreadcrumbs" runat="server">
		<% Html.RenderPartial("Breadcrumbs"); %>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<asp:PlaceHolder ID="VideoHeader" Visible="False" runat="server">
		<div class="page-header">
			<h1><%: Video.Title %></h1>
		</div>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<asp:PlaceHolder ID="VideoContent" Visible="False" runat="server">
		<div class="page-video embed-responsive embed-responsive-16by9">
			<% if (!string.IsNullOrWhiteSpace(Video.MediaEmbed)) { %>
				<%= Video.MediaEmbed %>
			<% } else if (!string.IsNullOrWhiteSpace(Video.MediaUrl)) { %>
				<video class="embed-responsive-item" src="<%: Video.MediaUrl %>" controls></video>
			<% } %>
		</div>
		<div class="page-copy">
			<%= Video.Copy %>
		</div>
		<div>
			<ul class="tags">
				<% foreach (var tag in Video.Tags) { %>
					<li><span class="label label-default"><span class="fa fa-tag" aria-hidden="true"></span> <%: tag %></span></li>
				<% } %>
			</ul>
		</div>
	</asp:PlaceHolder>
</asp:Content>
