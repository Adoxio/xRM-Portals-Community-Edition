/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Data
{
	internal static class OrganizationServiceContextExtensions
	{
		public static int FetchCount(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, Action<Action<string, string, string>> addFilterConditions = null)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""count"" alias=""count"" />
						<filter/>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();
			entity.SetAttributeValue("name", entityLogicalName);

			entity.Descendants("attribute").First().SetAttributeValue("name", countAttributeLogicalName);

			var filter = entity.Descendants("filter").First();
			filter.SetAttributeValue("type", "and");

			if (addFilterConditions != null)
			{
				addFilterConditions(filter.AddFetchXmlFilterCondition);
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return (int)response.EntityCollection.Entities.First().GetAttributeValue<AliasedValue>("count").Value;
		}

		public static int FetchCount(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, string linkEntityLogicalName, string linkFromAttributeLogicalName, string linkToAttributeLogicalName, Action<Action<string, string, string>> addFilterConditions = null)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""count"" alias=""count"" />
						<link-entity>
							<filter />
						</link-entity>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();
			entity.SetAttributeValue("name", entityLogicalName);

			entity.Descendants("attribute").First().SetAttributeValue("name", countAttributeLogicalName);

			var linkEntity = entity.Descendants("link-entity").First();
			linkEntity.SetAttributeValue("name", linkEntityLogicalName);
			linkEntity.SetAttributeValue("from", linkFromAttributeLogicalName);
			linkEntity.SetAttributeValue("to", linkToAttributeLogicalName);

			var filter = linkEntity.Descendants("filter").First();
			filter.SetAttributeValue("type", "and");

			if (addFilterConditions != null)
			{
				addFilterConditions(filter.AddFetchXmlFilterCondition);
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return (int)response.EntityCollection.Entities.First().GetAttributeValue<AliasedValue>("count").Value;
		}

		public static IDictionary<Guid, int> FetchCounts(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, string linkEntityLogicalName, string linkFromAttributeLogicalName, string linkToAttributeLogicalName, IEnumerable<Guid> linkEntitiesIds, Action<Action<string, string, string>> addFilterConditions = null)
		{
			if (!linkEntitiesIds.Any())
			{
				return new Dictionary<Guid, int>();
			}

			var ids = linkEntitiesIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""countcolumn"" alias=""count"" />
						<filter/>
						<link-entity>
							<attribute alias=""id"" groupby=""true"" />
							<filter type=""or"" />
						</link-entity>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();
			entity.SetAttributeValue("name", entityLogicalName);

			entity.Descendants("attribute").First().SetAttributeValue("name", countAttributeLogicalName);

			var filter = entity.Descendants("filter").First();
			filter.SetAttributeValue("type", "and");

			if (addFilterConditions != null)
			{
				addFilterConditions(filter.AddFetchXmlFilterCondition);
			}

			var linkEntity = entity.Descendants("link-entity").First();
			linkEntity.SetAttributeValue("name", linkEntityLogicalName);
			linkEntity.SetAttributeValue("from", linkFromAttributeLogicalName);
			linkEntity.SetAttributeValue("to", linkToAttributeLogicalName);

			var linkEntityAttribute = linkEntity.Descendants("attribute").First();
			linkEntityAttribute.SetAttributeValue("name", linkFromAttributeLogicalName);

			var linkEntityFilter = linkEntity.Descendants("filter").First();
			
			linkEntityFilter.AddFetchXmlFilterInCondition(linkFromAttributeLogicalName, ids.Select(id => id.ToString()));

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			var results = ids.ToDictionary(id => id, id => 0);

			foreach (var result in response.EntityCollection.Entities)
			{
				var id = (Guid)result.GetAttributeValue<AliasedValue>("id").Value;
				var count = (int)result.GetAttributeValue<AliasedValue>("count").Value;

				results[id] = count;
			}

			return results;
		}
	}
}
