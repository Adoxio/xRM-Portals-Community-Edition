<%@ Page Language="C#" MasterPageFile="../MasterPages/Products.master" AutoEventWireup="true" CodeBehind="ProductCollection.aspx.cs" Inherits="Site.Areas.Products.Pages.ProductCollection" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="Scripts">
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
			<asp:ObjectDataSource ID="BrandDataSource" TypeName="Adxstudio.Xrm.Products.IBrandDataAdapter" OnObjectCreating="CreateBrandDataAdapter" SelectMethod="SelectBrand" runat="server">
				<SelectParameters>
					<asp:QueryStringParameter Name="id" QueryStringField="brand"/>
				</SelectParameters>
			</asp:ObjectDataSource>
			<asp:ListView DataSourceID="BrandDataSource" runat="server">
				<LayoutTemplate>
					<small>
						<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
					</small>
				</LayoutTemplate>
				<ItemTemplate>
					<%#: Eval("Name") %>
				</ItemTemplate>
			</asp:ListView>
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<ul class="nav nav-tabs toolbar-nav">
		<li class="dropdown active">
			<a class="dropdown-toggle" data-toggle="dropdown" href="#" title="<%= CurrentSortOptionLabel %>"><span class="fa fa-list" aria-hidden="true"></span> <%= CurrentSortOptionLabel %> <b class="caret"></b></a>
			<ul class="dropdown-menu">
				<% foreach (var option in SortOptions) { %>
					<li>
						<a href="<%: GetSortUrl(option.Key) %>" title="<%= option.Value %>"><%= option.Value %></a>
					</li>
				<% } %>
			</ul>
		</li>
	</ul>
	<asp:ObjectDataSource ID="ProductsDataSource" TypeName="Adxstudio.Xrm.Products.IFilterableProductAggregationDataAdapter" OnObjectCreating="CreateSubjectProductsDataAdapter" SelectMethod="SelectProducts" SelectCountMethod="SelectProductCount" EnablePaging="True" runat="server">
		<SelectParameters>
			<asp:QueryStringParameter Name="brand" QueryStringField="brand"/>
			<asp:QueryStringParameter Name="rating" QueryStringField="rating"/>
			<asp:QueryStringParameter Name="sortExpression" QueryStringField="orderby"/>
		</SelectParameters>
	</asp:ObjectDataSource>
	<asp:ListView ID="Products" DataSourceID="ProductsDataSource" runat="server">
		<LayoutTemplate>
			<div class="row product-grid">
				<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
			</div>
			<adx:UnorderedListDataPager CssClass="pagination" PageSize="20" PagedControlID="Products" QueryStringField="page" runat="server">
				<Fields>
					<adx:ListItemNextPreviousPagerField ShowNextPageButton="false" ShowFirstPageButton="True" FirstPageText="&laquo;" PreviousPageText="&lsaquo;" />
					<adx:ListItemNumericPagerField ButtonCount="10" PreviousPageText="&hellip;" NextPageText="&hellip;" />
					<adx:ListItemNextPreviousPagerField ShowPreviousPageButton="false" ShowLastPageButton="True" LastPageText="&raquo;" NextPageText="&rsaquo;" />
				</Fields>
			</adx:UnorderedListDataPager>
		</LayoutTemplate>
		<ItemTemplate>
			<div class="col-md-3 col-sm-4">
				<a class="thumbnail" href="<%#: Url.Action("Product", "Products", new { productIdentifier = Eval("PartialURL") ?? Eval("SKU"), area = "Products" }) %>"> <%--href='<%# string.Format("~/products/product/{0}/", Eval("PartialURL") ?? Eval("SKU")) %>'>--%>
					<img src="<%#: string.IsNullOrWhiteSpace(Eval("ImageThumbnailURL").ToString()) ? "/image-not-available-150x150.png/" : Eval("ImageThumbnailURL") %>" alt="<%#: Eval("Name") %>" />
					<div class="caption text-center">
						<div class="product-name">
							<%#: Eval("Name") %>
						</div>
						<div class="product-price">
							<%#: Eval("ListPrice", "{0:C2}") %>
						</div>
						<div class="product-rating">
							<div data-rateit-readonly="true" data-rateit-ispreset="true" data-rateit-value="<%#: Eval("RatingInfo.Average") %>" class="rateit"></div>
						</div>
					</div>
				</a>
			</div>
		</ItemTemplate>
		<EmptyDataTemplate>
			<adx:Snippet SnippetName="No Products Message" DefaultText="<%$ ResourceManager:No_Products_Message %>" EditType="html" runat="server"/>
		</EmptyDataTemplate>
	</asp:ListView>
</asp:Content>

<asp:Content ContentPlaceHolderID="Filters" runat="server">
	<asp:ObjectDataSource ID="BrandsDataSource" TypeName="Adxstudio.Xrm.Products.IBrandDataAdapter" OnObjectCreating="CreateBrandDataAdapter" SelectMethod="SelectBrands" runat="server" />
	<asp:ListView ID="Brands" DataSourceID="BrandsDataSource" runat="server">
		<LayoutTemplate>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4><adx:Snippet SnippetName="Brand Filter Header" DefaultText="<%$ ResourceManager:Brands_DefaultText %>" runat="server"/></h4>
				</div>
				<div class="list-group">
					<asp:PlaceHolder runat="server">
						<a class="list-group-item <%: NoBrandFilter ? "active" : string.Empty %>" href="<%: AllBrandsUrl %>" title="<%: Html.SnippetLiteral("All Brands Brand Filter", ResourceManager.GetString("All_Brands")) %>"><%: Html.SnippetLiteral("All Brands Brand Filter", ResourceManager.GetString("All_Brands")) %></a>
					</asp:PlaceHolder>
					<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
				</div>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<asp:HyperLink CssClass='<%#: IsActiveBrandFilter(Eval("Id")) ? "list-group-item active" : "list-group-item" %>' NavigateUrl='<%#: GetBrandFilterUrl(Eval("Id")) %>' Text='<%#: Eval("Name") %>' ToolTip='<%#: Eval("Name") %>' runat="server"/>
		</ItemTemplate>
	</asp:ListView>
	<div class="content-panel panel panel-default">
		<div class="panel-heading">
			<h4>
				<span class="fa fa-star-o" aria-hidden="true"></span>
				<adx:Snippet SnippetName="Rating Filter Header" DefaultText="<%$ ResourceManager:Rating_DefaultText %>" runat="server"/>
			</h4>
		</div>
		<div class="list-group">
			<a class="list-group-item <%: NoRatingFilter ? "active" : string.Empty %>" href="<%: AnyRatingUrl %>" title="<%: Html.SnippetLiteral("All Ratings Filter", ResourceManager.GetString("Any_Rating")) %>"><%: Html.SnippetLiteral("All Ratings Filter", ResourceManager.GetString("Any_Rating")) %></a>
			<% foreach (var rating in RatingFilterOptions) { %>
				<a class="list-group-item <%: IsActiveRatingFilter(rating.Key) ? "active" : string.Empty %>" href="<%: GetRatingFilterUrl(rating.Key) %>" title="<%= rating.Value %>">
					<div data-rateit-readonly="true" data-rateit-ispreset="true" data-rateit-value="<%: rating.Key %>" class="rateit"></div>
					<strong><%= rating.Value %></strong>
				</a>
			<% } %>
		</div>
	</div>
</asp:Content>
