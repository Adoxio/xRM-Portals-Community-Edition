/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	public class LineItem : Tuple<Entity, EntityReference, decimal, bool, string, string, int>
	{
		public LineItem(Entity product, EntityReference unitOfMeasure, decimal quantity, bool optional, string description, string instructions, int order)
			: base(product, unitOfMeasure, quantity, optional, description, instructions, order) { }

		public string Description
		{
			get { return Item5; }
		}

		public string Instructions
		{
			get { return Item6; }
		}

		public bool Optional
		{
			get { return Item4; }
		}

		public int Order
		{
			get { return Item7; }
		}

		public Entity Product
		{
			get { return Item1; }
		}

		public decimal Quantity
		{
			get { return Item3; }
		}

		public EntityReference UnitOfMeasure
		{
			get { return Item2; }
		}

		public virtual EntityReference GetUnitOfMeasure(IEnumerable<Entity> productPriceListItems)
		{
			productPriceListItems = productPriceListItems.ToArray();

			if (!productPriceListItems.Any())
			{
				return null;
			}

			// Favour a price list item matching the line item UoM, if it has one.
			if (UnitOfMeasure != null)
			{
				var itemMatchingUom = productPriceListItems.FirstOrDefault(e => UnitOfMeasure.Equals(e.GetAttributeValue<EntityReference>("uomid")));

				if (itemMatchingUom != null)
				{
					return itemMatchingUom.GetAttributeValue<EntityReference>("uomid");
				}
			}

			// Otherwise, just take the first. Hopefully that's fine...
			return productPriceListItems.First().GetAttributeValue<EntityReference>("uomid");
		}

		public static LineItem GetLineItemFromLineItemEntity(Entity entity, string productAttribute, string descriptionAttribute, string instructionsAttribute, string orderAttribute, string requiredAttribute, string quantityAttribute, string uomAttribute, Dictionary<Guid, Entity> products)
		{
			if (entity == null)
			{
				return null;
			}

			var productReference = entity.GetAttributeValue<EntityReference>(productAttribute);

			if (productReference == null || productReference.LogicalName != "product")
			{
				return null;
			}

			Entity product;

			if (!products.TryGetValue(productReference.Id, out product))
			{
				return null;
			}

			var description = string.IsNullOrEmpty(descriptionAttribute)
				? product.GetAttributeValue<string>("description")
				: entity.GetAttributeValue<string>(descriptionAttribute);

			var instructions = string.IsNullOrEmpty(instructionsAttribute)
				? null
				: entity.GetAttributeValue<string>(instructionsAttribute);

			var order = string.IsNullOrEmpty(orderAttribute)
				? int.MaxValue
				: entity.GetAttributeValue<int?>(orderAttribute).GetValueOrDefault(int.MaxValue);

			var required = string.IsNullOrEmpty(requiredAttribute)
				|| entity.GetAttributeValue<bool?>(requiredAttribute).GetValueOrDefault();

			var quantity = string.IsNullOrEmpty(quantityAttribute)
				? required ? 1 : 0
				: entity.GetAttributeValue<decimal?>(quantityAttribute).GetValueOrDefault();

			var uom = string.IsNullOrEmpty(uomAttribute)
				? null
				: entity.GetAttributeValue<EntityReference>(uomAttribute);

			return new LineItem(product, uom, quantity, !required, description, instructions, order);
		}

	}
}
