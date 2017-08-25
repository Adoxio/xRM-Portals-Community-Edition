<%@ Control Language="C#" AutoEventWireup="true" ViewStateMode="Enabled" CodeBehind="WebFormPurchaseSummary.ascx.cs" Inherits="Site.Areas.Commerce.Controls.WebFormPurchaseSummary" %>

<asp:Panel ID="GeneralErrorMessage" Visible="False" runat="server">
	<div class="alert alert-block alert-danger">
		<adx:Snippet SnippetName="Ecommerce/Purchase/LoadError" DefaultText="<%$ ResourceManager:Unable_To_Retrieve_Purchase_Information %>" EditType="html" runat="server"/>
	</div>
</asp:Panel>

<asp:Panel ID="PurchaseSummary" CssClass="row purchase-summary" Visible="False" ViewStateMode="Enabled" runat="server">
	<div class="<%: Purchasable.RequiresShipping ? "col-md-6" : "col-md-12" %>">
		<fieldset>
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
			<div class="well form-inline">
				<div class="discount-code-validation">
					<asp:Panel ID="DiscountErrorAlreadyApplied" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/AlreadyApplied" DefaultText="<%$ ResourceManager:Discount_Already_Applied %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorCodeNotSpecified" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/CodeNotSpecified" DefaultText="<%$ ResourceManager:Discount_Code_Not_specified %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorDoesNotExist" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/DoesNotExist" DefaultText="<%$ ResourceManager:Inavlid_Discount_Code %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorInvalidDiscount" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/InvalidDiscountConfiguration" DefaultText="<%$ ResourceManager:Invalid_Discount %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorMaximumRedemptions" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/MaximumRedemptions" DefaultText="<%$ ResourceManager:Discount_Reached_Maximum_Number_Of_Redemptions %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorMinimumAmountNotMet" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/MinimumAmountNotMet" DefaultText="<%$ ResourceManager:Total_Amount_Less_Than_Required_Amount %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorUnknown" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/Unknown" DefaultText="<%$ ResourceManager:Unknown_Error_Occured_Message %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorZeroAmount" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/ZeroAmount" DefaultText="<%$ ResourceManager:Amount_Zero_Discount_Not_Applied %>" Literal="true" EditType="text" />
					</asp:Panel>
					<asp:Panel ID="DiscountErrorNotApplicable" class="alert alert-danger" Visible="False" runat="server">
						<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a> <adx:Snippet runat="server" SnippetName="Ecommerce/Purchase/DiscountCode/ValidationErrors/NotApplicable" DefaultText="<%$ ResourceManager:Discount_Code_Not_Applicable %>" Literal="true" EditType="text" />
					</asp:Panel>
				</div>
				<label for="DiscountCode">
					<adx:Snippet SnippetName="Ecommerce/Purchase/DiscountCode/CodeInputFieldLabel" DefaultText="<%$ ResourceManager:Discount_Code_DefaultText %>" EditType="text" runat="server" />
				</label>
				<input id="DiscountCode" type="text" runat="server" ClientIDMode="Static" class="form-control" />
				<asp:Button ID="ApplyDiscount" runat="server" Text="<%$ Snippet: Ecommerce/Purchase/DiscountCode/SubmitButtonLabel, Apply %>" CssClass="btn btn-warning" OnClick="ApplyDiscount_OnClick" CausesValidation="False" UseSubmitBehavior="False" />
			</div>
		</fieldset>
	</div>
	<asp:Panel ID="Shipping" CssClass="col-md-6 form-horizontal" Visible="False" runat="server">
		<asp:ValidationSummary ID="ShippingAddressValidationSummary" CssClass="alert alert-block alert-danger" ValidationGroup="ShippingAddress" runat="server"/>
		<fieldset>
			<legend>
				<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/Legend" DefaultText="<%$ ResourceManager:Shipping_Address_DefaultText %>" EditType="text" runat="server"/>
			</legend>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingName" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/Name" DefaultText="<%$ ResourceManager:Name_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingName" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingName" ValidationGroup="ShippingAddress" EnableClientScript="False" Display="None" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/Name/RequiredMessage, Name_Required_Field_Validation_Message %>' runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingAddressLine1" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/Line1" DefaultText="<%$ ResourceManager:Address_Line_1_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingAddressLine1" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingAddressLine1" ValidationGroup="ShippingAddress" EnableClientScript="False" Display="None" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/Line1/RequiredMessage, Address_Line_1_Required_Field_Validation_Message %>' runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingAddressLine2" CssClass="col-sm-4 control-label" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/Line2" DefaultText="<%$ ResourceManager:Address_Line_2_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingAddressLine2" ValidationGroup="ShippingAddress" CssClass="form-control" runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingCity" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/City" DefaultText="<%$ ResourceManager:City_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingCity" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingCity" ValidationGroup="ShippingAddress" EnableClientScript="False" Display="None" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/City/RequiredMessage, City_Required_Field_Validation_Message %>' runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingStateProvince" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/StateProvince" DefaultText="<%$ ResourceManager:State_Province_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingStateProvince" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingStateProvince" ValidationGroup="ShippingAddress" EnableClientScript="False" Display="None" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/StateProvince/RequiredMessage, State_Province_Required_Field_Validation_Message %>' runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingPostalCode" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/PostalCode" DefaultText="<%$ ResourceManager:Zip_Postal_Code %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingPostalCode" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingPostalCode" ValidationGroup="ShippingAddress" Display="None" EnableClientScript="False" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/PostalCode/RequiredMessage, Zip_Postal_Code_Required %>' runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="ShippingCountry" CssClass="col-sm-4 control-label required" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/ShippingAddress/Country" DefaultText="<%$ ResourceManager:Country_DefaultText %>" EditType="text" runat="server"/>
				</asp:Label>
				<div class="col-sm-8">
					<asp:TextBox ID="ShippingCountry" ValidationGroup="ShippingAddress" CssClass="required form-control" runat="server"/>
					<asp:RequiredFieldValidator ControlToValidate="ShippingCountry" ValidationGroup="ShippingAddress" Display="None" EnableClientScript="False" ErrorMessage='<%$ Snippet: Ecommerce/Purchase/ShippingAddress/Country/RequiredMessage, CountryRegion_Required_Field_Validation_Message %>' runat="server"/>
				</div>
			</div>
		</fieldset>
	</asp:Panel>
	<asp:HiddenField ID="QuoteId" runat="server" />
</asp:Panel>

<div id="progress-message" style="display:none;">
	<h2 style='padding: 10px; text-align: center;'>
		<img alt="Submitting" src="<%: Url.Content("~/xrm-adx/samples/images/ajax-loader.gif") %>" style="vertical-align: middle;">
	</h2>
</div>

<asp:ScriptManagerProxy runat="server">
	<Scripts>
		<asp:ScriptReference Path="~/js/jquery.blockUI.js" />
		<asp:ScriptReference Path="~/js/jquery.validate.min.js" />
		<asp:ScriptReference Path="~/Areas/Commerce/js/webform.purchase.js" />
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