<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" Codebehind="Home.aspx.cs" Inherits="Site.Areas.Company.Pages.HomePage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server"/>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	
	<div class="row">
		<div class="col-md-4">
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<asp:HyperLink CssClass="pull-right" NavigateUrl='<%$ CrmSiteMap: SiteMarker=Products, Return=Url %>' Text='<%$ Snippet: Home All Products Link Text, All Products %>' ToolTip='<%$ Snippet: Home All Products Link Text, All Products %>' runat="server" />
					<h4>
						<adx:Snippet SnippetName="Products Home Title" EditType="text" DefaultText="<%$ ResourceManager:Products_DefaultText %>" runat="server"/>
					</h4>
				</div>
				<asp:SiteMapDataSource ID="ProductsData" StartingNodeUrl='<%$ CrmSiteMap: SiteMarker=Products, Url %>' ShowStartingNode="False" runat="server"/>
				<asp:ListView DataSourceID="ProductsData" runat="server">
					<LayoutTemplate>
						<ul class="list-group">
							<li ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="list-group-item">
							<crm:CrmEntityDataSource ID="ProductPage" DataItem="<%# Container.DataItem %>" runat="server" />
							<h4 class="list-group-item-heading">
								<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' runat="server">
									<adx:Property DataSourceID="ProductPage" PropertyName="adx_title,adx_name" Literal="true" runat="server" />
								</asp:HyperLink>
							</h4>
							<div class="list-group-item-text">
								<adx:Property DataSourceID="ProductPage" PropertyName="adx_summary" EditType="html" runat="server" />
							</div>
						</li>
					</ItemTemplate>
				</asp:ListView>
			</div>
		</div>
		<div class="col-md-4">
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<asp:HyperLink CssClass="pull-right" NavigateUrl='<%$ CrmSiteMap: SiteMarker=News, Return=Url %>' Text='<%$ Snippet: Home All News Link Text, All News %>' ToolTip='<%$ Snippet: Home All News Link Text, All News %>' runat="server" />
					<h4>
						<a class="feed-icon fa fa-rss-square" href="<%: Url.RouteUrl("NewsFeed") %>" title="<%: Html.SnippetLiteral("News Subscribe Heading", ResourceManager.GetString("Subscribe_DefaultText")) %>"></a>
						<adx:Snippet SnippetName="News Home Title" EditType="text" DefaultText="<%$ ResourceManager:News_DefaultText %>" runat="server"/>
					</h4>
				</div>
				<asp:SiteMapDataSource ID="NewsData" StartingNodeUrl='<%$ CrmSiteMap: SiteMarker=News, Url %>' ShowStartingNode="False" runat="server"/>
				<asp:ListView DataSourceID="NewsData" runat="server">
					<LayoutTemplate>
						<ul class="list-group">
							<li ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="list-group-item">
							<h4 class="list-group-item-heading">
								<asp:HyperLink Text='<%#: Eval("Title") %>' NavigateUrl='<%#: Eval("Url") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
							</h4>
							<div class="list-group-item-text">
								<%# Eval("Description") %>
							</div>
						</li>
					</ItemTemplate>
				</asp:ListView>
			</div>
		</div>
		<div class="col-md-4">
			<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Home", __portalScopeId__ = Website.Id}); %>
		</div>
	</div>
</asp:Content>
