/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Globalization;

namespace Adxstudio.Xrm.Core
{
	/// <summary>
	/// Class provides methods to assist with entity and attribute metadata.
	/// </summary>
	public static class MetadataHelper
	{
		/// <summary>
		/// Retrieve entity metadata
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="logicalName"></param>
		/// <returns>Entity metadata</returns>
		public static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string logicalName)
		{
			try
			{
				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = logicalName,
					EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships
				});

				return response.EntityMetadata;
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());

                return new EntityMetadata();
			}
		}

		/// <summary>
		/// Creates a dictionary of an entity's attribute logical names and attribute type codes
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="entityLogicalName"></param>
		/// <returns>Dictionary of an entity's attribute logical names and attribute type codes</returns>
		public static Dictionary<string, AttributeTypeCode?> BuildAttributeTypeCodeDictionary(OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			var entityMetadata = GetEntityMetadata(serviceContext, entityLogicalName);

			if (entityMetadata == null)
			{
				return null;
			}

			var attributes = entityMetadata.Attributes;

			var attributeDictionary = attributes.Where(attribute => attribute.AttributeType != null).ToDictionary(attribute => attribute.LogicalName, attribute => attribute.AttributeType != null ? attribute.AttributeType.Value : (AttributeTypeCode?)null);

			return attributeDictionary;
		}

		/// <summary>
		/// Retrieve an entity attribute's attribute type code.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns>AttributeTypeCode</returns>
		public static AttributeTypeCode? GetAttributeTypeCode(OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName)
		{
			var entityMetadata = GetEntityMetadata(serviceContext, entityLogicalName);

			if (entityMetadata == null)
			{
				return null;
			}

			if (!entityMetadata.Attributes.Select(a => a.LogicalName).Contains(attributeLogicalName))
			{
				return null;
			}

			var attribute = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);
			
			return attribute == null ? null : attribute.AttributeType;
		}

		/// <summary>
		/// Checks the entity metadata attribute collection to determine if the specified attribute logical name exists.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns>True if attribute exists otherwise returns False</returns>
		public static bool IsAttributeLogicalNameValid(OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName)
		{
			var entityMetadata = GetEntityMetadata(serviceContext, entityLogicalName);

			return entityMetadata != null && entityMetadata.Attributes.Select(a => a.LogicalName).Contains(attributeLogicalName);
		}

		/// <summary>
		/// Retrieve the name of the entity's Primary Key attribute.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="logicalName"></param>
		/// <returns>Logical name of the Primary Key attribute.</returns>
		public static string GetEntityPrimaryKeyAttributeLogicalName(OrganizationServiceContext serviceContext, string logicalName)
		{
			var entityMetadata = GetEntityMetadata(serviceContext, logicalName);

			return entityMetadata.PrimaryIdAttribute;
		}

		private static readonly IDictionary<string, string> OptionSetLabelCache = new Dictionary<string, string>();

		/// <summary>
		/// Retrieve the label of an Option Set value.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		/// <param name="languageCode"></param>
		/// <returns>Label of the Option Set value.</returns>
		public static string GetOptionSetLabelByValue(OrganizationServiceContext context, string entityLogicalName, string attributeLogicalName, int value, int? languageCode)
		{
			if (string.IsNullOrEmpty(entityLogicalName) || string.IsNullOrEmpty(attributeLogicalName))
			{
				return string.Empty;
			}

			string cachedLabel;

			var cachedItemToFind = string.Format("{0}:{1}:{2}", entityLogicalName, attributeLogicalName, value);

			if (OptionSetLabelCache.TryGetValue(cachedItemToFind, out cachedLabel))
			{
				return cachedLabel;
			}

			var retrieveAttributeRequest = new RetrieveAttributeRequest
			{
				EntityLogicalName = entityLogicalName,
				LogicalName = attributeLogicalName
			};

			var retrieveAttributeResponse = (RetrieveAttributeResponse)context.Execute(retrieveAttributeRequest);

			var retrievedPicklistAttributeMetadata = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

			var option = retrievedPicklistAttributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == value);

			if (option == null)
			{
				return string.Empty;
			}

			var label = option.Label.GetLocalizedLabelString();

			if (languageCode != null)
			{
				foreach (var item in option.Label.LocalizedLabels.Where(item => item.LanguageCode == languageCode))
				{
					label = item.Label;

					break;
				}
			}

			if (option.Value.HasValue)
			{
				OptionSetLabelCache[cachedItemToFind] = label;
			}

			return label;
		}
	}
}
