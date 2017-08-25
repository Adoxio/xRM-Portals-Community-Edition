/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// A <see cref="ViewDataAdapter"/> special case that will filter the existing product (productid)
	/// lookup on an opportunity product by parent opportunity price list, and selected unit.
	/// </summary>
	internal class OpportunityProductExistingProductLookupViewSpecialCase : OpportunityPriceListSpecialCase
	{
		public override bool IsApplicable(IViewConfiguration configuration)
		{
			return string.Equals(configuration.EntityName, "product", StringComparison.InvariantCulture)
				&& string.Equals(configuration.ModalLookupEntityLogicalName, "opportunityproduct", StringComparison.InvariantCulture)
				&& string.Equals(configuration.ModalLookupAttributeLogicalName, "productid", StringComparison.InvariantCulture)
				&& configuration.ModalLookupFormReferenceEntityId != null
				&& string.Equals(configuration.ModalLookupFormReferenceEntityLogicalName, "opportunity", StringComparison.InvariantCulture)
				&& string.Equals(configuration.ModalLookupFormReferenceRelationshipName, "product_opportunities", StringComparison.InvariantCulture);
		}

		public override bool TryApply(IViewConfiguration configuration, IDataAdapterDependencies dependencies, IDictionary<string, string> customParameters, Fetch fetch)
		{
			if (!IsApplicable(configuration))
			{
				return false;
			}

			// This should be the ID of the parent opportunity for the opportunity product. If it's
			// null, we can't do any filtering.
			if (configuration.ModalLookupFormReferenceEntityId == null)
			{
				return false;
			}

			// Filter out non-published products,
			fetch.AddFilter(new Filter
			{
				Conditions = new List<Condition>
				{
					new Condition("statecode", ConditionOperator.Equal, 0)
				}
			});

			// Use distinct because you can have multiple price list items per product in a price list.
			fetch.Distinct = true;

			// Add the primary ID attribute explicitly because that allows Entity.Id to be set properly
			// when doing a distinct query.
			fetch.Entity.AddAttributes("productid");

			var productPriceLevelFilter = new Filter
			{
				Type = LogicalOperator.And,
				Conditions = new List<Condition>
				{
					new Condition(
						"pricelevelid",
						ConditionOperator.Equal,
						// If the opportunity doesn't have a price list, or we fail to get it for some reason, filter
						// by the empty GUID so that nothing will be returned.
						GetPriceListId(dependencies, configuration.ModalLookupFormReferenceEntityId.Value).GetValueOrDefault(Guid.Empty))
				}
			};
		
			// Filter by the selected unit of measure if available,
			string uomIdParameter;
			Guid uomId;

			if (customParameters != null
				&& customParameters.TryGetValue("uomid", out uomIdParameter)
				&& Guid.TryParse(uomIdParameter, out uomId))
			{
				productPriceLevelFilter.Conditions.Add(new Condition("uomid", ConditionOperator.Equal, uomId));
			}

			// Filter by price list items in parent opporunity price list.
			fetch.AddLink(new Link
			{
				Name = "productpricelevel",
				FromAttribute = "productid",
				ToAttribute = "productid",
				Type = JoinOperator.Inner,
				Filters = new List<Filter>
				{
					productPriceLevelFilter
				}
			});

			return true;
		}
	}
}
