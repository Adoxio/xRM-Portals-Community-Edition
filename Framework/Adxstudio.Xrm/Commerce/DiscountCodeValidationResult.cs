/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	public class DiscountCodeValidationResult
	{
		public DiscountCodeValidationResult() { }

		public DiscountCodeValidationResult(bool isValid)
		{
			IsValid = isValid;
		}

		public bool IsValid { get; set; }
		public DiscountErrorCode ErrorCode = DiscountErrorCode.Unknown;
		public string ExistingDiscountCodes { get; set; }
		public IEnumerable<DiscountError> DiscountErrors = Enumerable.Empty<DiscountError>();
		public IEnumerable<Guid> DiscountableQuoteProductIds = Enumerable.Empty<Guid>();

		public enum DiscountErrorCode
		{
			AlreadyApplied = 1,
			CodeNotSpecified = 2,
			DoesNotExist = 3,
			InvalidDiscountConfiguration = 4,
			MaximumRedemptions = 5,
			MinimumAmountNotMet = 6,
			QuoteNotFound = 7,
			Unknown = 8,
			UpdateFailed = 9,
			ZeroAmount = 10,
			NotApplicable = 11
		}

		public static DiscountCodeValidationResult ValidateDiscountCode(OrganizationServiceContext context, Guid quoteId, string code)
		{
			var errorCode = DiscountErrorCode.Unknown;
			var isValid = false;
			var discountableQuoteProductIds = new List<Guid>();
			if (string.IsNullOrWhiteSpace(code))
			{
				var result = new DiscountCodeValidationResult
								{
									ErrorCode = DiscountErrorCode.CodeNotSpecified
								};
				return result;
			}

			var quote = context.CreateQuery("quote").FirstOrDefault(q => q.GetAttributeValue<Guid>("quoteid") == quoteId);

			if (quote == null)
			{
				var result = new DiscountCodeValidationResult
								{
									ErrorCode = DiscountErrorCode.QuoteNotFound,
								};
				return result;
			}

			var existingDiscountCodes = quote.GetAttributeValue<string>("adx_discountcodes") ?? string.Empty;

			if (existingDiscountCodes.Contains(code))
			{
				var result = new DiscountCodeValidationResult
								{
									ErrorCode = DiscountErrorCode.AlreadyApplied,
								};
				return result;
			}

			var prefreightAmount = GetDecimalFromMoney(quote, "totalamountlessfreight");

			if (prefreightAmount <= 0)
			{
				var result = new DiscountCodeValidationResult
								{
									ErrorCode = DiscountErrorCode.ZeroAmount,
								};
				return result;
			}

			var discountErrors = new List<DiscountError>();

			var orderScopedDiscounts =
				context.CreateQuery("adx_discount")
					.Where(
						d =>
							d.GetAttributeValue<OptionSetValue>("statecode").Value == 0 &&
							(d.GetAttributeValue<OptionSetValue>("adx_scope") != null &&
							 d.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)DiscountScope.Order) &&
							((d.GetAttributeValue<DateTime?>("adx_startdate") == null ||
							  d.GetAttributeValue<DateTime?>("adx_startdate") <= DateTime.UtcNow) &&
							 (d.GetAttributeValue<DateTime?>("adx_enddate") == null ||
							  d.GetAttributeValue<DateTime?>("adx_enddate") >= DateTime.UtcNow)) &&
							d.GetAttributeValue<string>("adx_code") == code)
					.ToList();

			if (orderScopedDiscounts.Any())
			{
				var discountPercentage = quote.GetAttributeValue<decimal?>("discountpercentage") ?? 0;
				var discountAmount = GetDecimalFromMoney(quote, "discountamount");
				var newDiscountPercentage = discountPercentage;
				var newDiscountAmount = discountAmount;
				var appliedDiscounts = (from d in context.CreateQuery("adx_discount")
										join dq in context.CreateQuery("adx_discount_quote") on
											d.GetAttributeValue<Guid>("adx_discountid") equals dq.GetAttributeValue<Guid>("adx_discountid")
										where dq.GetAttributeValue<Guid>("quoteid") == quote.Id
										select d).ToList();
				var newDiscounts = new List<Entity>();

				foreach (var discount in orderScopedDiscounts)
				{
					var applied = appliedDiscounts.Any(d => d.GetAttributeValue<Guid>("adx_discountid") == discount.Id);

					if (applied)
					{
						discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.AlreadyApplied });
						continue;
					}

					var minimumPurchaseAmount = GetDecimalFromMoney(discount, "adx_minimumpurchaseamount");
					var maximumRedemptions = discount.GetAttributeValue<int?>("adx_maximumredemptions").GetValueOrDefault(0);
					var redemptions = discount.GetAttributeValue<int?>("adx_redemptions").GetValueOrDefault(0);
					var typeOption = discount.GetAttributeValue<OptionSetValue>("adx_type");
					decimal percentage = 0;
					decimal amount = 0;

					if (typeOption == null)
					{
						discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.InvalidDiscountConfiguration });
						continue;
					}

					switch (typeOption.Value)
					{
						case (int)DiscountType.Percentage:
							percentage = discount.GetAttributeValue<decimal?>("adx_percentage") ?? 0;
							break;
						case (int)DiscountType.Amount:
							amount = GetDecimalFromMoney(discount, "adx_amount");
							break;
						default:
							discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.InvalidDiscountConfiguration });
							continue;
					}

					if (minimumPurchaseAmount > 0 && prefreightAmount < minimumPurchaseAmount)
					{
						discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.MinimumAmountNotMet });
					}
					else if (maximumRedemptions > 0 && redemptions >= maximumRedemptions)
					{
						discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.MaximumRedemptions });
					}
					else
					{
						newDiscountPercentage += percentage;
						newDiscountAmount += amount;
						appliedDiscounts.Add(discount);
						newDiscounts.Add(discount);
					}
				}

				if (newDiscountPercentage != discountPercentage || newDiscountAmount != discountAmount)
				{
					isValid = true;
				}
			}

			if (!isValid)
			{
				// Check for valid quotedetail items

				var quoteDetails =
					context.CreateQuery("quotedetail")
							.Where(q => q.GetAttributeValue<EntityReference>("quoteid").Equals(quote.ToEntityReference()))
							.ToList();

				if (quoteDetails.Any())
				{
					var priceList = quote.GetAttributeValue<EntityReference>("pricelevelid");

					var productScopeDiscounts =
						context.CreateQuery("adx_discount")
							.Where(
								d =>
									d.GetAttributeValue<OptionSetValue>("statecode").Value == 0 &&
									(d.GetAttributeValue<OptionSetValue>("adx_scope") != null &&
									 d.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)DiscountScope.Product) &&
									((d.GetAttributeValue<DateTime?>("adx_startdate") == null ||
									  d.GetAttributeValue<DateTime?>("adx_startdate") <= DateTime.UtcNow) &&
									 (d.GetAttributeValue<DateTime?>("adx_enddate") == null ||
									  d.GetAttributeValue<DateTime?>("adx_enddate") >= DateTime.UtcNow)) &&
									d.GetAttributeValue<string>("adx_code") == code)
							.ToList();

					if (!productScopeDiscounts.Any())
					{
						var result = new DiscountCodeValidationResult
										{
											ErrorCode = DiscountErrorCode.DoesNotExist,
											DiscountErrors = discountErrors
										};
						return result;
					}

					foreach (var quotedetail in quoteDetails)
					{
						var baseAmount = GetDecimalFromMoney(quotedetail, "baseamount");

						if (baseAmount <= 0)
						{
							discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.ZeroAmount });
							continue;
						}

						var appliedDiscounts = (from d in context.CreateQuery("adx_discount")
												join i in context.CreateQuery("adx_discount_quotedetail") on
													d.GetAttributeValue<Guid>("adx_discountid") equals i.GetAttributeValue<Guid>("adx_discountid")
												where i.GetAttributeValue<Guid>("quotedetailid") == quotedetail.Id
												select d).ToList();
						var newDiscounts = new List<Entity>();
						var discountAmount = GetDecimalFromMoney(quotedetail, "manualdiscountamount");
						var newDiscountAmount = discountAmount;

						foreach (var discount in productScopeDiscounts)
						{
							var applied = appliedDiscounts.Any(d => d.GetAttributeValue<Guid>("adx_discountid") == discount.Id);

							if (applied)
							{
								discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.AlreadyApplied });
								continue;
							}

							var discountProductPriceLevel = (from pl in context.CreateQuery("productpricelevel")
															join dp in context.CreateQuery("adx_discount_productpricelevel") on
																pl.GetAttributeValue<Guid>("productpricelevelid") equals dp.GetAttributeValue<Guid>("productpricelevelid")
															where dp.GetAttributeValue<Guid>("adx_discountid") == discount.Id
															where pl.GetAttributeValue<EntityReference>("pricelevelid").Equals(priceList)
															select pl).ToList();

							if (!discountProductPriceLevel.Any())
							{
								continue;
							}

							var quotedetailid = quotedetail.Id;
							var quoteProductDiscounts = (from d in discountProductPriceLevel
														join q in
															context.CreateQuery("quotedetail")
																	.Where(q => q.GetAttributeValue<Guid>("quotedetailid") == quotedetailid)
															on
															new
																{
																	productid = d.GetAttributeValue<EntityReference>("productid"),
																	uomid = d.GetAttributeValue<EntityReference>("uomid")
																} equals
															new
																{
																	productid = q.GetAttributeValue<EntityReference>("productid"),
																	uomid = q.GetAttributeValue<EntityReference>("uomid")
																}
														select q).ToList();

							if (!quoteProductDiscounts.Any())
							{
								continue;
							}

							var maximumRedemptions = discount.GetAttributeValue<int?>("adx_maximumredemptions").GetValueOrDefault(0);
							var redemptions = discount.GetAttributeValue<int?>("adx_redemptions").GetValueOrDefault(0);
							var typeOption = discount.GetAttributeValue<OptionSetValue>("adx_type");
							decimal amount = 0;

							if (typeOption == null)
							{
								discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.InvalidDiscountConfiguration });
								continue;
							}

							switch (typeOption.Value)
							{
								case (int)DiscountType.Percentage:
									var percentage = discount.GetAttributeValue<decimal?>("adx_percentage") ?? 0;
									if (percentage > 0 && baseAmount > 0)
									{
										amount = baseAmount * percentage / 100;
									}
									break;
								case (int)DiscountType.Amount:
									amount = GetDecimalFromMoney(discount, "adx_amount");
									break;
								default:
									discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.InvalidDiscountConfiguration });
									continue;
							}

							if (maximumRedemptions > 0 && redemptions >= maximumRedemptions)
							{
								discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.MaximumRedemptions });
								continue;
							}

							newDiscountAmount += amount;
							appliedDiscounts.Add(discount);
							newDiscounts.Add(discount);
							discountableQuoteProductIds.Add(quotedetail.Id);
						}

						if (newDiscountAmount == discountAmount)
						{
							continue;
						}

						isValid = true;

						break;
					}
				}
			}

			if (!isValid && !discountErrors.Any())
			{
				discountErrors.Add(new DiscountError { ErrorCode = DiscountErrorCode.NotApplicable });
				errorCode = DiscountErrorCode.NotApplicable;
			}

			return new DiscountCodeValidationResult(isValid)
						{
							ErrorCode = errorCode,
							ExistingDiscountCodes = existingDiscountCodes,
							DiscountableQuoteProductIds = discountableQuoteProductIds.Distinct(),
							DiscountErrors = discountErrors
						};
		}

		private static decimal GetDecimalFromMoney(Entity entity, string attributeLogicalName, decimal defaultValue = 0)
		{
			var value = entity.GetAttributeValue<Money>(attributeLogicalName);

			return value == null ? defaultValue : value.Value;
		}
	}

	public class DiscountError
	{
		public DiscountError()
		{
			ErrorCode = DiscountCodeValidationResult.DiscountErrorCode.Unknown;
		}

		public DiscountCodeValidationResult.DiscountErrorCode ErrorCode { get; set; }
	}
}
