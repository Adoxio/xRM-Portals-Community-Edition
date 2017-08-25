<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShoppingCart.ascx.cs" Inherits="Site.Areas.Commerce.Controls.ShoppingCart" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Panel ID="ShoppingCartEmptyPanel" Visible="False" runat="server">
	<%: Html.HtmlSnippet("Ecommerce/ShoppingCart/EmptyMessage",
		defaultValue: @"<div class=""alert alert-block alert-info""><p>Your shopping cart is currently empty.</p></div>") %>
</asp:Panel>

<asp:Panel ID="ShoppingCartPanel" CssClass="shopping-cart" ViewStateMode="Enabled" runat="server">
	<asp:ListView ID="CartItems" OnItemCommand="CartItems_ItemCommand" runat="server">
		<LayoutTemplate>
			<table class="table">
				<thead>
					<th>
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemDescriptionHeader" DefaultText="<%$ ResourceManager:Description_DefaultText %>" EditType="text" runat="server"/>
					</th>
					<th>
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemPriceHeader" DefaultText="<%$ ResourceManager:Price_DefaultText %>" EditType="text" runat="server"/>
					</th>
					<th>
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemQuantityHeader" DefaultText="<%$ ResourceManager:Quantity_DefaultText %>" EditType="text" runat="server"/>
					</th>
					<th>
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemRemoveHeader" DefaultText="<%$ ResourceManager:Remove_DefaultText %>" EditType="text" runat="server"/>
					</th>
					<th>
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemTotalHeader" DefaultText="<%$ ResourceManager:Total_DefaultText_Label %>" EditType="text" runat="server"/>
					</th>
				</thead>
				<tbody>
					<tr id="itemPlaceholder" runat="server"/>
				</tbody>
			</table>
		</LayoutTemplate>
		<ItemTemplate>
			<tr>
				<td class="description">
					<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Description") %>' ToolTip='<%#: Eval("Description") %>' runat="server"/>
				</td>
				<td class="price"><%#: Eval("Price", "{0:C2}") %></td>
				<td class="quantity">
					<div class="input-group">
						<div class="input-group-addon">&times;</div>
						<asp:TextBox ID="Quantity" CssClass="form-control" runat="server" Text='<%# Eval("Quantity", "{0:N0}") %>' ClientIDMode="Static" />
					</div>
				</td>
				<td class="delete">
					<asp:LinkButton CommandName="Remove" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-xs btn-danger" runat="server">
						<adx:Snippet SnippetName="Ecommerce/ShoppingCart/ItemRemoveButtonText" DefaultText="<%$ ResourceManager:Remove_DefaultText %>" Literal="True" runat="server"/>
					</asp:LinkButton>
					<asp:TextBox ID="CartItemID" runat="server" Visible="false" Text='<%# Eval("Id") %>' />
				</td>
				<td class="total"><%#: Eval("Total", "{0:C2}") %></td>
			</tr>
		</ItemTemplate>
	</asp:ListView>
	<div class="grand-total">
		<adx:Snippet SnippetName="Ecommerce/ShoppingCart/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
		<asp:Label ID="Total" runat="server" />
	</div>
	<div class="form-actions">
		<asp:Button ID="UpdateCart" CssClass="btn btn-default" runat="server" Text="<%$ Snippet: Ecommerce/ShoppingCart/UpdateCartButtonLabel, Update Cart %>" OnClick="OnUpdateCart" />
		<asp:Button runat="server" ID="SaveToQuote" CssClass="btn btn-default" Text="<%$ Snippet: Ecommerce/ShoppingCart/SaveToQuoteButtonLabel, Save Quote %>" Visible="False" OnClick="OnSaveToQuote" />
		<asp:Button ID="CheckOut" CssClass="btn btn-primary" runat="server" Text="<%$ Snippet: Ecommerce/ShoppingCart/CheckoutButtonLabel, Checkout %>" OnClick="OnCheckOut" />
	</div>
	<script type="text/javascript">
		jQuery.fn.restrictNumbers = function () {
			return this.each(function () {
				$(this).keydown(function (e) {
					var key = e.which || e.keyCode;
					if (!e.shiftKey && !e.altKey && !e.ctrlKey &&
						// numbers
						key >= 48 && key <= 57 ||
						// Numeric keypad
						key >= 96 && key <= 105 ||
						// comma, period and minus, . on keypad
						//key == 190 || key == 188 || key == 109 || key == 110 ||
						// Backspace and Tab and Enter
						key == 8 || key == 9 || key == 13 ||
						// Home and End
						key == 35 || key == 36 ||
						// left and right arrows
						key == 37 || key == 39 ||
						// Del and Ins
						key == 46 || key == 45) {
						return true;
					}
					return false;
				});
			});
		};
		$(document).ready(function() {
			$("#Quantity").restrictNumbers();
		});
	</script>
</asp:Panel>