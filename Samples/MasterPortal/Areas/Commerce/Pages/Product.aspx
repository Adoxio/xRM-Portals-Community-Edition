<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" Inherits="Site.Areas.Commerce.Pages.ProductPage" Codebehind="Product.aspx.cs" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Commerce/css/commerce.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/Commerce/js/commerce.js") %>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" ViewStateMode="Enabled" runat="server">
	<asp:ListView ID="Products" OnItemCommand="ProductItemCommand" runat="server">
		<LayoutTemplate>
			<div class="section">
				<ul class="list-unstyled products">
					<li id="itemPlaceholder" runat="server"/>
				</ul>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<li>
				<div class="well clearfix">
					<div class="pull-right">
						<asp:Button ID="AddToCart" runat="server" CssClass="btn btn-primary" Text='<%$ Snippet: buttontext/shoppingcart/additem, Add to Cart %>' CommandName="AddToCart" CommandArgument='<%# Eval("ProductId") %>'/>
					</div>
					<h4><%#: Eval("Name") %></h4>
					<div class="price"><%#: Eval("Price") %></div>
				</div>
			</li>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>
