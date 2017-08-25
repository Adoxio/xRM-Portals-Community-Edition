<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Retail.Pages.Home" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Retail/css/retail.css") %>" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="Scripts">
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ContentHeader"/>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	<div class="row">
		<div class="col-md-8">
			<asp:Panel ID="FeaturedProductsPanel" runat="server">
				<asp:ObjectDataSource ID="FeaturedProductsDataSource" TypeName="Adxstudio.Xrm.Products.CampaignProductsDataAdapter" OnObjectCreating="CreateCampaignProductsDataAdapter" SelectMethod="SelectProducts" SelectCountMethod="SelectProductCount" EnablePaging="True" runat="server" />
				<asp:ListView ID="FeaturedProducts" DataSourceID="FeaturedProductsDataSource" runat="server">
					<LayoutTemplate>
						<div class="content-panel panel panel-default">
							<div class="panel-heading">
								<h4>
									<span class="fa fa-star-o" aria-hidden="true"></span>
									<adx:Snippet SnippetName="Retail Featured Products Title" EditType="text" DefaultText="<%$ ResourceManager:Featured_Products_DefaultText %>" runat="server"/>
								</h4>
							</div>
							<div class="panel-body">
								<div class="row product-grid">
									<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
								</div>
							</div>
						</div>
					</LayoutTemplate>
					<ItemTemplate>
						<div class="col-sm-3">
							<a class="thumbnail" href="<%# Url.Action("Product", "Products", new {productIdentifier = Eval("PartialURL") ?? Eval("SKU"), area = "Products"}) %>">
								<img src="<%#: string.IsNullOrWhiteSpace(Eval("ImageThumbnailURL").ToString()) ? "/image-not-available-150x150.png/" : Eval("ImageThumbnailURL") %>" alt="<%# Eval("Name") %>" />
								<div class="caption text-center">
									<div class="product-name"><%#: Eval("Name") %></div>
									<div class="product-price">
										<strong><%#: Eval("ListPrice", "{0:C2}") %></strong>
									</div>
									<div class="product-rating">
										<div data-rateit-readonly="true" data-rateit-ispreset="true" data-rateit-value="<%#: Eval("RatingInfo.Average") %>" class="rateit"></div>
									</div>
								</div>
							</a>
						</div>
					</ItemTemplate>
				</asp:ListView>
			</asp:Panel>
			<% var featuredCollections = Html.WebLinkSet("Featured Collections"); %>
			<% if (featuredCollections != null)
			   { %>
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<h4>
							<span class="fa fa-star-o" aria-hidden="true"></span>
							<%: Html.TextAttribute(featuredCollections, "adx_title", tagName: "span") ?? new HtmlString("Featured Categories") %>
						</h4>
					</div>
					<div class="panel-body">
						<div class="weblinks <%: featuredCollections.Editable ? "xrm-entity xrm-editable-adx_weblinkset" : string.Empty %>" data-weblinks-maxdepth="1">
							<ul class="row list-unstyled product-grid">
								<% foreach (var webLink in featuredCollections.WebLinks)
								   { %>
									<li class="col-sm-3">
										<a class="thumbnail" href="<%: webLink.Url %>" title="<%: webLink.ToolTip %>">
											<%: Html.WebLinkImage(webLink) %>
											<div class="caption text-center">
												<div class="product-name"><%: webLink.Name %></div>
											</div>
										</a>
									</li>
								<% } %>
							</ul>
							<% if (featuredCollections.Editable)
							   { %>
								<%: Html.WebLinkSetEditingMetadata(featuredCollections) %>
							<% } %>
						</div>
					</div>
				</div>
			<% } %>
		</div>
		<div class="col-md-4">
			<div class="sidebar">
				<%: Html.WebLinksListGroup("Secondary Navigation") %>
				<% Html.RenderAction("PollPlacement", "Poll", new {Area = "Cms", id = "Sidebar", __portalScopeId__ = Website.Id}); %>
				<% Html.RenderAction("AdPlacement", "Ad", new { Area = "Cms", id = "Sidebar Bottom", __portalScopeId__ = Website.Id }); %>
			</div>
		</div>
	</div>
</asp:Content>