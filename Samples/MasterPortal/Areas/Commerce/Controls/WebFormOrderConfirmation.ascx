<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormOrderConfirmation.ascx.cs" Inherits="Site.Areas.Commerce.Controls.WebFormOrderConfirmation" %>

<asp:Panel ID="GeneralErrorMessage" Visible="False" runat="server">
	<div class="alert alert-block alert-danger">
		<adx:Snippet SnippetName="Ecommerce/Purchase/Order/LoadError" DefaultText="<%$ ResourceManager:Unable_To_Retrieve_Order_Information %>" EditType="html" runat="server"/>
	</div>
</asp:Panel>

<asp:Panel ID="Order" Visible="False" runat="server">
	<div class="purchase-summary">
		<adx:Snippet CssClass="message" SnippetName="Ecommerce/Order/OrderMessage" DefaultText="" EditType="html"  runat="server"/>
		<fieldset>
			<legend>
				<adx:Snippet SnippetName="Ecommerce/Order/OrderNumberLabel" DefaultText="<%$ ResourceManager:Order_DefaultText %>" runat="server" EditType="text" />
				<asp:Label ID="OrderNumber" runat="server"/>
			</legend>
			<div class="well">
				<asp:ListView ID="OrderItems" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="line-item">
							<div class="pull-right">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
									<asp:Label ID="Quantity" runat="server" Text='<%#: Eval("Quantity", "{0:N0}") %>' />
									&times;
								</asp:Label>
								<%#: Eval("PricePerUnit", "{0:C2}") %>
							</div>
							<div class="column title"><%#: Eval("Name") %></div>
						</li>
					</ItemTemplate>
				</asp:ListView>
				<asp:Panel ID="OrderTotalTax" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalTaxAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotalShipping" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalShippingAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotalDiscount" CssClass="total discount" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
					&minus;<asp:Label ID="OrderTotalDiscountAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotal" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalAmount" runat="server"/>
				</asp:Panel>
			</div>
		</fieldset>
	</div>
</asp:Panel>

<asp:Panel ID="Invoice" Visible="False" runat="server">
	<div class="purchase-summary">
		<adx:Snippet CssClass="message" SnippetName="Ecommerce/Invoice/InvoiceMessage" DefaultText="" EditType="html"  runat="server"/>
		<fieldset>
			<legend>
				<adx:Snippet SnippetName="Ecommerce/Invoice/InvoiceNumberLabel" DefaultText="<%$ ResourceManager:Invoice_DefaultText %>" runat="server" EditType="text" />
				<asp:Label ID="InvoiceNumber" runat="server"/>
			</legend>
			<div class="well">
				<asp:ListView ID="InvoiceItems" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="line-item">
							<div class="pull-right">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
									<asp:Label ID="Quantity" runat="server" Text='<%#: Eval("Quantity", "{0:N0}") %>' />
									&times;
								</asp:Label>
								<%#: Eval("PricePerUnit", "{0:C2}") %>
							</div>
							<div class="column title"><%#: Eval("Name") %></div>
						</li>
					</ItemTemplate>
				</asp:ListView>
				<asp:Panel ID="InvoiceTotalTax" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalTaxAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotalShipping" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalShippingAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotalDiscount" CssClass="total discount" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
					&minus;<asp:Label ID="InvoiceTotalDiscountAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotal" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalAmount" runat="server"/>
				</asp:Panel>
			</div>
		</fieldset>
	</div>
</asp:Panel>