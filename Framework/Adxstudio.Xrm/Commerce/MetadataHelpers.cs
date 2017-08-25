/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Commerce
{
	public static class MetadataHelpers
	{
		public static decimal GetDecimalFromMoney(Entity entity, string attributeLogicalName, decimal defaultValue = 0)
		{
			var value = entity.GetAttributeValue<Money>(attributeLogicalName);

			return value == null ? defaultValue : value.Value;
		}

		public static decimal GetDecimalFromMoneyAndRoundToPrecision(OrganizationServiceContext serviceContext, Entity entity, string attributeLogicalName, int precision, decimal defaultValue = 0)
		{
			var value = GetDecimalFromMoney(entity, attributeLogicalName, defaultValue);

			var result = Math.Round(value, precision, MidpointRounding.AwayFromZero);

			return result;
		}

		public static decimal GetDecimalFromMoneyAndRoundToPrecision(OrganizationServiceContext serviceContext, Entity entity, string attributeLogicalName, decimal defaultValue = 0, int defaultPrecision = 2)
		{
			var precision = GetPrecisionFromMoney(serviceContext, entity.LogicalName, attributeLogicalName, defaultPrecision);

			var value = GetDecimalFromMoney(entity, attributeLogicalName, defaultValue);

			var result = Math.Round(value, precision, MidpointRounding.AwayFromZero);

			return result;
		}

		public static Money ConvertDecimalToMoneyAndRoundToPrecision(OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName, decimal value, int defaultPrecision = 2)
		{
			var precision = GetPrecisionFromMoney(serviceContext, entityLogicalName, attributeLogicalName, defaultPrecision);

			var result = Math.Round(value, precision, MidpointRounding.AwayFromZero);

			return new Money(result);
		}

		public static int GetPrecisionFromMoney(OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName, int defaultPrecision = 2)
		{
			var entityMetadata = GetEntityMetadata(serviceContext, entityLogicalName);

			if (entityMetadata == null)
			{
				return defaultPrecision;
			}

			var attributeMetadata = GetAttributeMetadata(entityMetadata, attributeLogicalName) as MoneyAttributeMetadata;

			if (attributeMetadata == null)
			{
				return defaultPrecision;
			}

			var precision = attributeMetadata.Precision.GetValueOrDefault(defaultPrecision);

			return precision;
		}

		public static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			var retrieveAttributeRequest = new RetrieveEntityRequest
			{
				LogicalName = entityLogicalName,
				EntityFilters = EntityFilters.Attributes
			};

			var response = (RetrieveEntityResponse)serviceContext.Execute(retrieveAttributeRequest);

			return response.EntityMetadata;
		}

		public static AttributeMetadata GetAttributeMetadata(EntityMetadata entityMetadata, string attributeLogicalName)
		{
			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			return attributeMetadata;
		}

	}
}
