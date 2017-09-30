/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm;

namespace Site.Areas.Commerce.Controls
{
	public partial class WebFormPurchaseSummary : WebFormCommercePortalUserControl
	{
		protected IPurchasable Purchasable { get; private set; }

		protected EntityReference Target { get; private set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			Target = GetTargetEntityReference();

			if (Target == null)
			{
				return;
			}

			var options = IsPostBack
				? GetPostedOptions()
				: Enumerable.Empty<IPurchasableItemOptions>();

			Guid quoteId;

			if (IsPostBack && Guid.TryParse(QuoteId.Value, out quoteId))
			{
				WebForm.CurrentSessionHistory.QuoteId = quoteId;
			}

			var dataAdapter = CreatePurchaseDataAdapter(Target, CurrentStepEntityPrimaryKeyLogicalName);

			Purchasable = dataAdapter.Select(options);
		}

		private IEnumerable<IPurchasableItemOptions> GetPostedOptions()
		{
			var options = new List<IPurchasableItemOptions>();

			foreach (var item in PurchaseItems.Items)
			{
				var itemValues = new OrderedDictionary();

				PurchaseItems.ExtractItemValues(itemValues, item, true);

				var quoteProductIdValue = itemValues["QuoteProduct.Id"];

				if (quoteProductIdValue == null)
				{
					continue;
				}

				Guid quoteProductId;

				if (!Guid.TryParse(quoteProductIdValue.ToString(), out quoteProductId))
				{
					continue;
				}

				var isSelectedValue = itemValues["IsSelected"];

				if (!(isSelectedValue is bool))
				{
					continue;
				}

				var quoteProductReference = new EntityReference("quotedetail", quoteProductId);
				var isSelected = (bool)isSelectedValue;

				options.Add(new PurchasableItemOptions(quoteProductReference, isSelected));
			}

			return options;
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (Purchasable == null)
			{
				PurchaseSummary.Visible = false;
				GeneralErrorMessage.Visible = true;

				WebForm.EnableDisableNextButton(false);

				return;
			}

			PurchaseSummary.Visible = true;
			GeneralErrorMessage.Visible = false;
			Shipping.Visible = Purchasable.RequiresShipping;

			PurchaseDiscounts.DataSource = Purchasable.Discounts;
			PurchaseDiscounts.DataBind();

			PurchaseItems.DataSource = Purchasable.Items;
			PurchaseItems.DataBind();

			QuoteId.Value = Purchasable.Quote.Id.ToString();

			if (Purchasable.RequiresShipping && Purchasable.ShipToAddress != null)
			{
				ShippingCity.Text = Purchasable.ShipToAddress.City;
				ShippingCountry.Text = Purchasable.ShipToAddress.Country;
				ShippingName.Text = Purchasable.ShipToAddress.Name;
				ShippingPostalCode.Text = Purchasable.ShipToAddress.PostalCode;
				ShippingStateProvince.Text = Purchasable.ShipToAddress.StateOrProvince;
				ShippingAddressLine1.Text = Purchasable.ShipToAddress.Line1;
				ShippingAddressLine2.Text = Purchasable.ShipToAddress.Line2;
			}
		}

		protected override void OnSubmit(object sender, WebFormSubmitEventArgs e)
		{
			if (Purchasable == null)
			{
				e.Cancel = true;

				PurchaseSummary.Visible = false;
				GeneralErrorMessage.Visible = true;

				WebForm.EnableDisableNextButton(false);

				return;
			}

			WebForm.CurrentSessionHistory.QuoteId = Purchasable.Quote.Id;

			Page.Validate();

			if (!Page.IsValid)
			{
				e.Cancel = true;

				return;
			}

			if (Purchasable.RequiresShipping)
			{
				var dataAdapter = CreatePurchaseDataAdapter(Target, CurrentStepEntityPrimaryKeyLogicalName);

				dataAdapter.UpdateShipToAddress(new PurchaseAddress
				{
					City = ShippingCity.Text,
					Country = ShippingCountry.Text,
					Line1 = ShippingAddressLine1.Text,
					Line2 = ShippingAddressLine2.Text,
					Name = ShippingName.Text,
					PostalCode = ShippingPostalCode.Text,
					StateOrProvince = ShippingStateProvince.Text
				});
			}

			SetAttributeValuesAndSave();
		}

		protected void ApplyDiscount_OnClick(object sender, EventArgs e)
		{
			if (Purchasable == null)
			{
				return;
			}
			
			var discountCode = DiscountCode.Value;

			var discountCodeValidationResult = DiscountCodeValidationResult.ValidateDiscountCode(ServiceContext, Purchasable.Quote.Id, discountCode);
			
			if (!discountCodeValidationResult.IsValid)
			{
				DiscountErrorAlreadyApplied.Visible = discountCodeValidationResult.ErrorCode ==
													DiscountCodeValidationResult.DiscountErrorCode.AlreadyApplied ||
													discountCodeValidationResult.DiscountErrors.Any(
														o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.AlreadyApplied);
				DiscountErrorCodeNotSpecified.Visible = discountCodeValidationResult.ErrorCode ==
														DiscountCodeValidationResult.DiscountErrorCode.CodeNotSpecified ||
														discountCodeValidationResult.DiscountErrors.Any(
															o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.CodeNotSpecified);
				DiscountErrorDoesNotExist.Visible = discountCodeValidationResult.ErrorCode ==
													DiscountCodeValidationResult.DiscountErrorCode.DoesNotExist ||
													discountCodeValidationResult.DiscountErrors.Any(
														o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.DoesNotExist);
				DiscountErrorInvalidDiscount.Visible = discountCodeValidationResult.ErrorCode ==
														DiscountCodeValidationResult.DiscountErrorCode.InvalidDiscountConfiguration ||
														discountCodeValidationResult.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.QuoteNotFound ||
														discountCodeValidationResult.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.UpdateFailed ||
														discountCodeValidationResult.DiscountErrors.Any(
															o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.InvalidDiscountConfiguration);
				DiscountErrorMaximumRedemptions.Visible = discountCodeValidationResult.ErrorCode ==
														DiscountCodeValidationResult.DiscountErrorCode.MaximumRedemptions ||
														discountCodeValidationResult.DiscountErrors.Any(
															o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.MaximumRedemptions);
				DiscountErrorMinimumAmountNotMet.Visible = discountCodeValidationResult.ErrorCode ==
															DiscountCodeValidationResult.DiscountErrorCode.MinimumAmountNotMet ||
															discountCodeValidationResult.DiscountErrors.Any(
																o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.MinimumAmountNotMet);
				DiscountErrorUnknown.Visible = discountCodeValidationResult.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.Unknown || (discountCodeValidationResult.ErrorCode == 0 && !discountCodeValidationResult.DiscountErrors.Any());
				DiscountErrorZeroAmount.Visible = discountCodeValidationResult.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.ZeroAmount ||
				discountCodeValidationResult.DiscountErrors.Any(
					o => o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.ZeroAmount);
				DiscountErrorNotApplicable.Visible = discountCodeValidationResult.ErrorCode ==
													 DiscountCodeValidationResult.DiscountErrorCode.NotApplicable ||
													 discountCodeValidationResult.DiscountErrors.Any(
														 o =>
															 o.ErrorCode == DiscountCodeValidationResult.DiscountErrorCode.NotApplicable);

				return;
			}
			
			DiscountErrorAlreadyApplied.Visible = false;
			DiscountErrorCodeNotSpecified.Visible = false;
			DiscountErrorDoesNotExist.Visible = false;
			DiscountErrorInvalidDiscount.Visible = false;
			DiscountErrorMaximumRedemptions.Visible = false;
			DiscountErrorMinimumAmountNotMet.Visible = false;
			DiscountErrorUnknown.Visible = false;
			DiscountErrorZeroAmount.Visible = false;

			try
			{
				// Add new discount code to existing discount codes and update quote, plugins will process the code.
				var updateContext = new OrganizationServiceContext(new OrganizationService("Xrm"));
				var quoteUpdate = new Entity("quote") { Id = Purchasable.Quote.Id };
				var updateDiscountCodes = string.IsNullOrWhiteSpace(discountCodeValidationResult.ExistingDiscountCodes) ? discountCode : string.Format("{0},{1}", discountCodeValidationResult.ExistingDiscountCodes, discountCode);
				quoteUpdate["adx_discountcodes"] = updateDiscountCodes;
				updateContext.Attach(quoteUpdate);
				updateContext.UpdateObject(quoteUpdate);
				updateContext.SaveChanges();
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}

			Target = GetTargetEntityReference();

			if (Target == null)
			{
				return;
			}

			var options = IsPostBack
				? GetPostedOptions()
				: Enumerable.Empty<IPurchasableItemOptions>();

			Guid quoteId;

			if (IsPostBack && Guid.TryParse(QuoteId.Value, out quoteId))
			{
				WebForm.CurrentSessionHistory.QuoteId = quoteId;
			}

			var dataAdapter = CreatePurchaseDataAdapter(Target, CurrentStepEntityPrimaryKeyLogicalName);
			
			var quoteProducts = ServiceContext.CreateQuery("quotedetail").Where(q => q.GetAttributeValue<EntityReference>("quoteid") == Purchasable.Quote).ToArray();

			foreach (var quoteProduct in quoteProducts)
			{
				ServiceContext.TryRemoveFromCache(quoteProduct);
			}

			Purchasable = dataAdapter.Select(options);

			PurchaseDiscounts.DataSource = Purchasable.Discounts;
			PurchaseDiscounts.DataBind();

			PurchaseItems.DataSource = Purchasable.Items;
			PurchaseItems.DataBind();

			DiscountCode.Value = string.Empty;
		}

		protected virtual void SetAttributeValuesAndSave()
		{
			if (CurrentStepEntityID == Guid.Empty)
			{
				return;
			}

			using (var serviceContext = PortalCrmConfigurationManager.CreateServiceContext())
			{
				var currentStepEntityUpdate = new Entity(CurrentStepEntityLogicalName)
				{
					Id = CurrentStepEntityID
				};

				SetAttributeValuesAndSave(serviceContext, currentStepEntityUpdate);
			}
		}
	}
}
