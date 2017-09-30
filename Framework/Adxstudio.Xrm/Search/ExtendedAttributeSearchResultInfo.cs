/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Search
{
	internal class ExtendedAttributeSearchResultInfo
	{
		private static readonly int[] _extendedInfoAttributeQueryTypes = new[] { 64, 0 };

		public ExtendedAttributeSearchResultInfo(OrganizationServiceContext context, string logicalName, IDictionary<string, EntityMetadata> metadataCache)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			Context = context;
			Metadata = GetEntityMetadata(context, logicalName, metadataCache);
			DisplayName = GetEntityDisplayName(Metadata);
			TypeCode = GetEntityTypeCode(Metadata);
			AttributeLayoutXml = GetAttributeLayoutXml(context, Metadata, TypeCode);
		}

		public string DisplayName { get; private set; }

		public EntityMetadata Metadata { get; private set; }

		public int TypeCode { get; private set; }

		protected XDocument AttributeLayoutXml { get; private set; }
		
		protected OrganizationServiceContext Context { get; private set; }

		public IDictionary<string, string> GetAttributes(Entity entity, IDictionary<string, EntityMetadata> metadataCache)
		{
			if (AttributeLayoutXml == null || entity == null)
			{
				return CreateBaseAttributesDictionary();
			}

			var attributeLookup = Metadata.Attributes.ToDictionary(a => a.LogicalName, a => a);

			var attributes = CreateBaseAttributesDictionary();

			var names = from cell in AttributeLayoutXml.XPathSelectElements("//cell")
				select cell.Attribute("name")
				into nameAttibute where nameAttibute != null
				select nameAttibute.Value
				into name where !string.IsNullOrEmpty(name)
				select name;

			foreach (var name in names)
			{
				AttributeMetadata attributeMetadata;

				if (!attributeLookup.TryGetValue(name, out attributeMetadata))
				{
					continue;
				}

				var resultAttribute = GetSearchResultAttribute(Context, entity, attributeMetadata, metadataCache);

				if (resultAttribute == null)
				{
					continue;
				}

				attributes[resultAttribute.Value.Key] = resultAttribute.Value.Value;
			}

			return attributes;
		}

		private IDictionary<string, string> CreateBaseAttributesDictionary()
		{
			return new Dictionary<string, string>
			{
				{ "_EntityDisplayName", DisplayName },
				{ "_EntityTypeCode", TypeCode.ToString() },
			};
		}

		private static XDocument GetAttributeLayoutXml(OrganizationServiceContext context, EntityMetadata metadata, int typeCode)
		{
			var queries = context.CreateQuery("savedquery")
				.Where(e => e.GetAttributeValue<bool?>("isdefault").GetValueOrDefault(false) && e.GetAttributeValue<int>("returnedtypecode") == typeCode)
				.ToList();

			return (
				from queryType in _extendedInfoAttributeQueryTypes
				select queries.FirstOrDefault(e => e.GetAttributeValue<int>("querytype") == queryType)
				into query where query != null
				select XDocument.Parse(query.GetAttributeValue<string>("layoutxml"))).FirstOrDefault();
		}

		private static string GetEntityDisplayName(EntityMetadata metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			if (metadata.DisplayName != null && metadata.DisplayName.UserLocalizedLabel != null)
			{
				return metadata.DisplayName.GetLocalizedLabelString();
			}

			throw new InvalidOperationException("Unable to retrieve the label for entity name.".FormatWith(metadata.LogicalName));
		}

		private static EntityMetadata GetEntityMetadata(OrganizationServiceContext context, string logicalName, IDictionary<string, EntityMetadata> metadataCache)
		{
			EntityMetadata cachedMetadata;

			if (metadataCache.TryGetValue(logicalName, out cachedMetadata))
			{
				return cachedMetadata;
			}

			var metadataReponse = context.Execute(new RetrieveEntityRequest { LogicalName = logicalName, EntityFilters = EntityFilters.Attributes }) as RetrieveEntityResponse;

			if (metadataReponse != null && metadataReponse.EntityMetadata != null)
			{
				metadataCache[logicalName] = metadataReponse.EntityMetadata;

				return metadataReponse.EntityMetadata;
			}

			throw new InvalidOperationException("Unable to retrieve the metadata for entity name {0}.".FormatWith(logicalName));
		}

		private static int GetEntityTypeCode(EntityMetadata metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			if (metadata.ObjectTypeCode != null)
			{
				return metadata.ObjectTypeCode.Value;
			}

			throw new InvalidOperationException("Unable to retrieve the object type code for entity name {0}.".FormatWith(metadata.LogicalName));
		}

		private static KeyValuePair<string, string>? GetSearchResultAttribute(OrganizationServiceContext context, Entity entity, AttributeMetadata attributeMetadata, IDictionary<string, EntityMetadata> metadataCache)
		{
			var label = attributeMetadata.DisplayName.GetLocalizedLabelString();

			if (AttributeTypeEqualsOneOf(attributeMetadata, "lookup", "customer"))
			{
				return null;
			}

			if (AttributeTypeEqualsOneOf(attributeMetadata, "picklist"))
			{
				var picklistMetadata = attributeMetadata as PicklistAttributeMetadata;

				if (picklistMetadata == null)
				{
					return null;
				}

				var picklistValue = entity.GetAttributeValue<OptionSetValue>(attributeMetadata.LogicalName);

				if (picklistValue == null)
				{
					return null;
				}

				var option = picklistMetadata.OptionSet.Options.FirstOrDefault(o => o.Value != null && o.Value.Value == picklistValue.Value);

				if (option == null || option.Label == null || option.Label.UserLocalizedLabel == null)
				{
					return null;
				}

				new KeyValuePair<string, string>(label, option.Label.GetLocalizedLabelString());
			}

			var value = entity.GetAttributeValue<object>(attributeMetadata.LogicalName);

			return value == null ? null : new KeyValuePair<string, string>?(new KeyValuePair<string, string>(label, value.ToString()));
		}

		private static bool AttributeTypeEqualsOneOf(AttributeMetadata attributeMetadata, params string[] typeNames)
		{
			var attributeTypeName = attributeMetadata.AttributeType.Value.ToString();

			return typeNames.Any(name => string.Equals(attributeTypeName, name, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
