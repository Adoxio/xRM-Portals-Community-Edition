<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ProductCollections.aspx.cs" Inherits="Site.Areas.Products.Pages.ProductCollections" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Products/css/products.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="row product-grid product-grid-lg">
		<asp:ListView ID="ChildView" runat="server">
			<LayoutTemplate>
				<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
			</LayoutTemplate>
			<ItemTemplate>
				<div class="col-md-3 col-sm-6">
					<a class="thumbnail" href='<%#: Eval("Url") %>'>
						<asp:Literal runat="server" Text='<%# BuildImageTag(Eval("Entity") as Entity) %>'></asp:Literal>
						<div class="caption text-center">
							<div class="product-name"><%#: Eval("Title") %></div>
						</div>
					</a>
				</div>
			</ItemTemplate>
		</asp:ListView>
	</div>
</asp:Content>
