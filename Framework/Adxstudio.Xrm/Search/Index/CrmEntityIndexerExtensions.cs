/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Search.Index
{
	internal static class CrmEntityIndexerExtensions
	{
		public static FetchXml GetFetchXmlFilteredToSingleEntity(this ICrmEntityIndexer indexer, string fetchXml, OrganizationServiceContext dataContext, string entityLogicalName, Guid id)
		{
			var filteredFetchXml = XDocument.Parse(fetchXml);

			var entity = filteredFetchXml.XPathSelectElements("/fetch/entity")
				.Where(e => e.Attributes("name").Any(a => a.Value == entityLogicalName))
				.FirstOrDefault();

			if (entity == null)
			{
				throw new InvalidOperationException("Invalid FetchXML, unable to find entity element in FetchXML:\n\n{0}".FormatWith(filteredFetchXml));
			}

			var existingFilter = entity.XPathSelectElement("filter");

			var idFilter = new XElement("filter");

			idFilter.Add(new XAttribute("type", "and"));

			var condition = new XElement("condition");

			var primaryKey = GetPrimaryKeyField(indexer, dataContext, entityLogicalName);

			condition.Add(new XAttribute("attribute", primaryKey));
			condition.Add(new XAttribute("operator", "eq"));
			condition.Add(new XAttribute("value", id.ToString()));

			idFilter.Add(condition);

			if (existingFilter != null)
			{
				existingFilter.Remove();

				idFilter.Add(existingFilter);
			}

			entity.Add(idFilter);

			return new FetchXml(filteredFetchXml);
		}

		public static string GetPrimaryKeyField(this ICrmEntityIndexer indexer, OrganizationServiceContext dataContext, string entityLogicalName)
		{
			string primaryKey = null;

			var entityMetadata = dataContext.GetEntityMetadata(entityLogicalName, EntityFilters.Entity);

			if (entityMetadata != null)
			{
				primaryKey = entityMetadata.PrimaryIdAttribute;
			}

			if (string.IsNullOrEmpty(primaryKey))
			{
                throw new InvalidOperationException("Unable to retrieve the primary key field name for entity name {0}.".FormatWith(entityLogicalName));
			}

			return primaryKey;
		}

		public static int GetReturnTypeCode(this ICrmEntityIndexer indexer, OrganizationServiceContext dataContext, string entityLogicalName)
		{
			int? typeCode = null;

			var entityMetadata = dataContext.GetEntityMetadata(entityLogicalName, EntityFilters.Entity);

			if (entityMetadata != null && entityMetadata.ObjectTypeCode != null)
			{
				typeCode = entityMetadata.ObjectTypeCode.Value;
			}

			if (typeCode.HasValue)
			{
				return typeCode.Value;
			}

			throw new InvalidOperationException("Unable to retrieve the object type code for entity name {0}.".FormatWith(entityLogicalName));
		}
	}
}
