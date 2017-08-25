<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormPayment.ascx.cs" Inherits="Site.Areas.Commerce.Controls.WebFormPayment" %>

<asp:Panel ID="GeneralErrorMessage" Visible="False" runat="server">
	<div class="alert alert-block alert-danger">
		<adx:Snippet SnippetName="Ecommerce/Purchase/LoadError" DefaultText="<%$ ResourceManager:Unable_To_Retrieve_Purchase_Information %>" EditType="html" runat="server"/>
	</div>
</asp:Panel>

<asp:Panel ID="AuthorizeNetErrorPanel" CssClass="alert alert-danger alert-block" Visible="False" runat="server">
	<adx:Snippet ID="PaymentErrorPreface" SnippetName="Ecommerce/Purchase/PaymentError" DefaultText="<%$ ResourceManager:Payment_Error_Message %>" Editable="True" EditType="html" runat="server"/> 
	<p><asp:Label ID="AuthorizeNetErrorMessage" runat="server"/></p>
</asp:Panel>

<asp:Panel ID="Payment" Visible="False" CssClass="payment" runat="server">
	<asp:Panel ID="CreditCardPaymentPanel" CssClass="credit-card-payment" Visible="False" runat="server">
		<asp:Panel ID="TestModePanel" CssClass="alert alert-block alert-info" Visible="False" runat="server">
			<p><strong>Payment test mode is enabled.</strong> Credit card fields have been populated with test values.</p>
		</asp:Panel>

		<section id="cvv-help" class="modal">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header"><button class="close" data-dismiss="modal">&times;</button>
						<h1 class="modal-title h4">
							<adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/CVVHelp/TitleLabel" DefaultText="<%$ ResourceManager:Credit_Card_Verification_Value_DefaultText %>" EditType="text" />
						</h1>
					</div>
					<div class="modal-body">
						<adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/CVVHelp/Content" DefaultText="<%$ ResourceManager:CVV_Help_Message %>" EditType="html" />
					</div>
					<div class="modal-footer">
						<button class="btn btn-primary" data-dismiss="modal">
							<adx:Snippet SnippetName="Ecommerce/Purchase/CVVHelp/CloseButtonLabel" DefaultText="<%$ ResourceManager:Close_DefaultText %>" EditType="text" runat="server"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<input type="hidden" id="form-action-override" value="<%: GetFormAction() %>" />
		
		<input type="hidden" id="x_fp_hash" name="x_fp_hash" value="<%: FingerprintHash %>" />
		<input type="hidden" id="x_fp_sequence" name="x_fp_sequence" value="<%: FingerprintSequence %>" />
		<input type="hidden" id="x_fp_timestamp" name="x_fp_timestamp" value="<%: FingerprintTimestamp %>" />
		<input type="hidden" id="x_login" name="x_login" value="<%: ApiLogin %>" />
		<input type="hidden" id="x_amount" name="x_amount" value="<%: Amount %>" />
		<input type="hidden" id="x_tax" name="x_tax" value="<%: Tax %>" />
		<input type="hidden" id="x_relay_url" name="x_relay_url" value="<%: RelayURL %>" />
		<input type="hidden" id="x_relay_response" name="x_relay_response" value="<%: RelayResponse %>" />
		<input type="hidden" id="order_id" name="order_id" value="<%: OrderID %>" />
		<input type="hidden" id="x_ship_to_first_name" name="x_ship_to_first_name" value="<%: FirstName %>" />
		<input type="hidden" id="x_ship_to_last_name" name="x_ship_to_last_name" value="<%: LastName %>" />
		<input type="hidden" id="x_ship_to_address" name="x_ship_to_address" value="<%: ShippingAddress %>" />
		<input type="hidden" id="x_ship_to_city" name="x_ship_to_city" value="<%: ShippingCity %>" />
		<input type="hidden" id="x_ship_to_state" name="x_ship_to_state" value="<%: ShippingProvince %>" />
		<input type="hidden" id="x_ship_to_zip" name="x_ship_to_zip" value="<%: ShippingPostalCode %>" />
		<input type="hidden" id="x_ship_to_country" name="x_ship_to_country" value="<%: ShippingCountry %>" />

		<% foreach (var lineItem in LineItems) { %>
			<input type="hidden" name="x_line_item" value="<%: lineItem %>" />
		<% } %>
		<div class="row">
			<div class="col-md-6 form-horizontal">
				<fieldset>
					<legend>
						<adx:Snippet SnippetName="Ecommerce/Purchase/BillingContact/Legend" DefaultText="<%$ ResourceManager:Billing_Contact_DefaultText %>" EditType="text" runat="server"/>
					</legend>
					<div class="form-group">
						<label for="x_first_name" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingContact/FirstName" DefaultText="<%$ ResourceManager:First_Name_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_first_name" name="x_first_name" type="text" value="<%: FirstName %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_last_name" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingContact/LastName" DefaultText="<%$ ResourceManager:Last_Name_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_last_name" name="x_last_name" type="text" value="<%: LastName %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_email" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingContact/Email" DefaultText="<%$ ResourceManager:Email_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_email" name="x_email" type="text" value="<%: Email %>" class="form-control" />
						</div>
					</div>
				</fieldset>
				<fieldset>
					<legend>
						<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/Legend" DefaultText="<%$ ResourceManager:Billing_Address_DefaultText %>" EditType="text" runat="server"/>
					</legend>
					<div class="form-group">
						<label for="x_address" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/Address" DefaultText="<%$ ResourceManager:Address_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_address" name="x_address" type="text" value="<%: Address %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_city" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/City" DefaultText="<%$ ResourceManager:City_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_city" name="x_city" type="text" value="<%: City %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_state" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/StateProvince" DefaultText="<%$ ResourceManager:State_Province_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input type="text" id="x_state" name="x_state" value="<%: Province %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_zip" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/PostalCode" DefaultText="<%$ ResourceManager:Zip_Postal_Code %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input id="x_zip" name="x_zip" type="text" value="<%: PostalCode %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_country" class="col-sm-4 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/BillingAddress/Country" DefaultText="<%$ ResourceManager:Country_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-8">
							<input type="text" id="x_country" name="x_country" value="<%: Country %>" class="form-control" />
						</div>
					</div>
				</fieldset>
			</div>

			<div class="col-md-6 form-horizontal">
				<fieldset class="purchase-summary">
					<legend>
						<adx:Snippet SnippetName="Ecommerce/Purchase/PurchaseSummary/Legend" DefaultText="<%$ ResourceManager:Purchase_Summary_DefaultText %>" EditType="text" runat="server"/>
					</legend>
					<div class="well">
						<asp:ListView ID="PurchaseItems" ViewStateMode="Enabled" runat="server">
							<LayoutTemplate>
								<table class="line-items">
									<tbody>
										<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
									</tbody>
								</table>
							</LayoutTemplate>
							<ItemTemplate>
								<tr>
									<td>
										<asp:CheckBox ID="IsSelected" Checked='<%# Bind("IsSelected") %>' Enabled='<%# Eval("IsOptional") %>' runat="server"/>
										<asp:HiddenField ID="QuoteProductId" Value='<%#: Bind("QuoteProduct.Id") %>' runat="server" />
									</td>
									<td class="title">
										<%#: Eval("Name") %>
										<asp:ListView ID="PurchaseItemDiscounts" DataSource='<%# Eval("Discounts") %>' runat="server">
											<LayoutTemplate>
												<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
											</LayoutTemplate>
											<ItemTemplate>
												<div class="discount-name"><small><%#: Eval("Name") %></small></div>
											</ItemTemplate>
										</asp:ListView>
									</td>
									<td class="qty">
										<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
											<small>
												<asp:Label ID="Quantity" runat="server" Text='<%#: Eval("Quantity", "{0:N0}") %>' />
												&times;
												<%#: Eval("PricePerUnit", "{0:C2}") %>
											</small>
										</asp:Label>
									</td>
									<td class="price">
										<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 0 %>' runat="server" >
											<asp:Label Visible='<%# ((decimal)Eval("Amount")) > ((decimal)Eval("AmountAfterDiscount")) %>' runat="server" >
												<small><del><%#: Eval("Amount", "{0:C2}") %></del></small>
											</asp:Label>
											<asp:Label runat="server" CssClass='<%# (((decimal)Eval("Amount")) > ((decimal)Eval("AmountAfterDiscount"))) ? "discount" : "" %>'>
												<%#: Eval("AmountAfterDiscount", "{0:C2}") %>
											</asp:Label>
										</asp:Label>
										<asp:Label Visible='<%# ((decimal)Eval("Quantity")) == 0 %>' CssClass="text-muted" runat="server" >
											<%#: Eval("PricePerUnit", "{0:C2}") %>
										</asp:Label>
									</td>
								</tr>
							</ItemTemplate>
						</asp:ListView>
						<table class="totals">
							<tbody>
								<% if (Purchasable.TotalLineItemAmount != Purchasable.TotalAmount) { %>
									<tr class="total">
										<td>
											<small>
												<adx:Snippet SnippetName="Ecommerce/Purchase/SubTotalLabel" DefaultText="<%$ ResourceManager:Sub_Total_DefaultText %>" runat="server" EditType="text" />
											</small>
										</td>
										<td>
											<%: Purchasable.TotalLineItemAmount.ToString("C2") %>
										</td>
									</tr>
								<% } %>
								<% if (Purchasable.TotalDiscount > 0) { %>
									<tr class="total discount">
										<td>
											<% if (Purchasable.Discounts.Any()) { %>
												<div class="pull-left">
													<asp:ListView ID="PurchaseDiscounts" runat="server">
														<LayoutTemplate>
															<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
														</LayoutTemplate>
														<ItemTemplate>
															<div class="discount-name"><small><%#: Eval("Name") %></small></div>
														</ItemTemplate>
													</asp:ListView>
												</div>
											<% } %>
											<small>
												<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
											</small>
										</td>
										<td>
											&minus;<%: Purchasable.TotalDiscount.ToString("C2") %>
										</td>
									</tr>
								<% } %>
								<% if (Purchasable.TotalPreShippingAmount != Purchasable.TotalAmount) { %>
									<tr class="total">
										<td>
											<small>
												<adx:Snippet SnippetName="Ecommerce/Purchase/PreTaxSubTotalLabel" DefaultText="<%$ ResourceManager:Sub_Total_DefaultText %>" runat="server" EditType="text" />
											</small>
										</td>
										<td>
											<%: Purchasable.TotalPreShippingAmount.ToString("C2") %>
										</td>
									</tr>
								<% } %>
								<% if (Purchasable.TotalTax > 0) { %>
									<tr class="total">
										<td>
											<small>
												<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
											</small>
										</td>
										<td>
											<%: Purchasable.TotalTax.ToString("C2") %>
										</td>
									</tr>
								<% } %>
								<% if (Purchasable.ShippingAmount > 0) { %>
									<tr class="total">
										<td>
											<small>
												<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
											</small>
										</td>
										<td>
											<%: Purchasable.ShippingAmount.ToString("C2") %>
										</td>
									</tr>
								<% } %>
								<tr class="total grand-total">
									<td>
										<small>
											<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
										</small>
									</td>
									<td>
										<%: Purchasable.TotalAmount.ToString("C2") %>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
				</fieldset>
				<fieldset>
					<legend>
						<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Legend" DefaultText="<%$ ResourceManager:Pay_With_Credit_Card_Message %>" EditType="text" runat="server"/>
					</legend>
					<div class="form-group">
						<div class="col-sm-12">
							<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Instructions" DefaultText="" Editable="True" EditType="html" runat="server"/>
						</div>
					</div>
					<!-- (c) 2005, 2012. Authorize.Net is a registered trademark of CyberSource Corporation --> 
					<div class="form-group">
						<label for="x_card_num" class="col-sm-6 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/CreditCardNumber" DefaultText="<%$ ResourceManager:Credit_Card_Number_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-6">
							<input id="x_card_num" name="x_card_num" type="text" value="<%: CreditCardNumber %>" class="form-control" />
						</div>
					</div>
					<div class="form-group">
						<label for="ExpiryMonth" class="col-sm-6 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Expiration" DefaultText="<%$ ResourceManager:Expires_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-3">
							<input type="hidden" id="ExpiryMonthDefault" clientidmode="Static" runat="server" />
							<select id="ExpiryMonth" name="ExpiryMonth" class="form-control">
								<option value="">
									<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Expiration/Month" DefaultText="<%$ ResourceManager:Month_DefaultText %>" Literal="True" runat="server"/>
								</option>
								<option value="01">1</option>
								<option value="02">2</option>
								<option value="03">3</option>
								<option value="04">4</option>
								<option value="05">5</option>
								<option value="06">6</option>
								<option value="07">7</option>
								<option value="08">8</option>
								<option value="09">9</option>
								<option value="10">10</option>
								<option value="11">11</option>
								<option value="12">12</option>
							</select>
						</div>
						<div class="col-sm-3">
							<input type="hidden" id="ExpiryYearDefault" clientidmode="Static" runat="server" />
							<select id="ExpiryYear" name="ExpiryYear" class="form-control">
								<option value="">
									<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Expiration/Year" DefaultText="<%$ ResourceManager:Year_DefaultText %>" Literal="True" runat="server"/>
								</option>
							</select>
							<input type="hidden" id="x_exp_date" name="x_exp_date" value="<%: CreditCardExpiry %>" />
						</div>
					</div>
					<div class="form-group">
						<label for="x_card_code" class="col-sm-6 control-label required">
							<adx:Snippet SnippetName="Ecommerce/Purchase/CreditCardPayment/Verification" DefaultText="<%$ ResourceManager:Card_Verification_Value_DefaultText %>" EditType="text" runat="server"/>
						</label>
						<div class="col-sm-6">
							<div class="input-group">
								<input id="x_card_code" name="x_card_code" type="text" value="<%: CreditCardVerificationValue %>" maxlength="4" class="form-control" />
								<div class="input-group-btn">
									<a id='cvv-help-link' href='#cvv-help' data-toggle='modal' class="btn btn-info">
										<span class="fa fa-question-circle" aria-hidden="true"></span>
									</a>
								</div>
							</div>
						</div>
					</div>
					<div class="form-group">
						<adx:Snippet SnippetName="Ecommerce/Purchase/Authorize.Net/VerifiedMerchantSeal" DefaultText="" EditType="html" runat="server" />
					</div>
				</fieldset>
			</div>
		</div>
		
		<%-- Hack to get authorize.net to work - without the button the payment provider will throw an error. --%>
		<asp:Button ID="BtnSubmit" runat="server" Text="" ClientIDMode="Static" Height="0" Width="0" BorderStyle="None" />
	</asp:Panel>

	<asp:Panel ID="PayPalPaymentPanel" CssClass="paypal-payment" Visible="False" runat="server">
		<fieldset class="purchase-summary">
			<legend>
				<adx:Snippet SnippetName="Ecommerce/Purchase/PurchaseSummary/Legend" DefaultText="<%$ ResourceManager:Purchase_Summary_DefaultText %>" EditType="text" runat="server"/>
			</legend>
			<div class="well">
				<asp:ListView ID="PayPalPurchaseItems" runat="server">
					<LayoutTemplate>
						<table class="line-items">
							<tbody>
								<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
							</tbody>
						</table>
					</LayoutTemplate>
					<ItemTemplate>
						<tr>
							<td>
								<%#: Eval("Name") %>
								<asp:ListView ID="PayPalPurchaseItemDiscounts" DataSource='<%# Eval("Discounts") %>' runat="server">
									<LayoutTemplate>
										<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
									</LayoutTemplate>
									<ItemTemplate>
										<div class="discount-name"><small><%#: Eval("Name") %></small></div>
									</ItemTemplate>
								</asp:ListView>
							</td>
							<td class="qty">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
									<small>
										<asp:Label ID="Quantity" runat="server" Text='<%#: Eval("Quantity", "{0:N0}") %>' />
										&times;
										<%#: Eval("PricePerUnit", "{0:C2}") %>
									</small>
								</asp:Label>
							</td>
							<td class="price">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 0 %>' runat="server" >
									<asp:Label Visible='<%# ((decimal)Eval("Amount")) > ((decimal)Eval("AmountAfterDiscount")) %>' runat="server" >
										<small><del><%#: Eval("Amount", "{0:C2}") %></del></small>
									</asp:Label>
									<asp:Label runat="server" CssClass='<%# (((decimal)Eval("Amount")) > ((decimal)Eval("AmountAfterDiscount"))) ? "discount" : "" %>'>
										<%#: Eval("AmountAfterDiscount", "{0:C2}") %>
									</asp:Label>
								</asp:Label>
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) == 0 %>' CssClass="text-muted" runat="server" >
									<%#: Eval("PricePerUnit", "{0:C2}") %>
								</asp:Label>
							</td>
						</tr>
					</ItemTemplate>
				</asp:ListView>
				<table class="totals">
					<tbody>
						<% if (Purchasable.TotalLineItemAmount != Purchasable.TotalAmount) { %>
							<tr class="total">
								<td>
									<small>
										<adx:Snippet SnippetName="Ecommerce/Purchase/SubTotalLabel" DefaultText="<%$ ResourceManager:Sub_Total_DefaultText %>" runat="server" EditType="text" />
									</small>
								</td>
								<td>
									<%: Purchasable.TotalLineItemAmount.ToString("C2") %>
								</td>
							</tr>
						<% } %>
						<% if (Purchasable.TotalDiscount > 0) { %>
							<tr class="total discount">
								<td>
									<% if (Purchasable.Discounts.Any()) { %>
										<div class="pull-left">
											<asp:ListView ID="PayPalPurchaseDiscounts" runat="server">
												<LayoutTemplate>
													<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
												</LayoutTemplate>
												<ItemTemplate>
													<div class="discount-name"><small><%#: Eval("Name") %></small></div>
												</ItemTemplate>
											</asp:ListView>
										</div>
									<% } %>
									<small>
										<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
									</small>
								</td>
								<td>
									&minus;<%: Purchasable.TotalDiscount.ToString("C2") %>
								</td>
							</tr>
						<% } %>
						<% if (Purchasable.TotalPreShippingAmount != Purchasable.TotalAmount) { %>
							<tr class="total">
								<td>
									<small>
										<adx:Snippet SnippetName="Ecommerce/Purchase/PreTaxSubTotalLabel" DefaultText="<%$ ResourceManager:Sub_Total_DefaultText %>" runat="server" EditType="text" />
									</small>
								</td>
								<td>
									<%: Purchasable.TotalPreShippingAmount.ToString("C2") %>
								</td>
							</tr>
						<% } %>
						<% if (Purchasable.TotalTax > 0) { %>
							<tr class="total">
								<td>
									<small>
										<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
									</small>
								</td>
								<td>
									<%: Purchasable.TotalTax.ToString("C2") %>
								</td>
							</tr>
						<% } %>
						<% if (Purchasable.ShippingAmount > 0) { %>
							<tr class="total">
								<td>
									<small>
										<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
									</small>
								</td>
								<td>
									<%: Purchasable.ShippingAmount.ToString("C2") %>
								</td>
							</tr>
						<% } %>
						<tr class="total grand-total">
							<td>
								<small>
									<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
								</small>
							</td>
							<td>
								<%: Purchasable.TotalAmount.ToString("C2") %>
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</fieldset>
		<adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/PayPal/Instructions" DefaultText="" EditType="html" />
	</asp:Panel>

	<div id="progress-message" style="display:none;">
		<adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/PaymentProcessingMessage" DefaultText="<h2 style='padding: 10px; text-align: center;'><img alt='Loading' src='~/xrm-adx/samples/images/ajax-loader.gif' style='vertical-align: middle;'></h2>" EditType="html" />
	</div>

	<asp:HiddenField ID="QuoteId" runat="server" />
</asp:Panel>

<asp:ScriptManagerProxy runat="server">
	<Scripts>
		<asp:ScriptReference Path="~/js/jquery.blockUI.js" />
		<asp:ScriptReference Path="~/js/jquery.validate.min.js" />
		<asp:ScriptReference Path="~/Areas/Commerce/js/webform.payment.js" />
	</Scripts>
</asp:ScriptManagerProxy>

<script type="text/javascript">
	function webFormClientValidate() {
		var isValid = $("#content_form").valid();
		if (isValid) {
			$.blockUI({ message: $("#progress-message") });
		}
		return isValid;
	}
</script>