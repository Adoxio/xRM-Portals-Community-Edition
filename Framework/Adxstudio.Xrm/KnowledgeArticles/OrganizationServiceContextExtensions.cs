/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	internal static class OrganizationServiceContextExtensions
	{
		public static int FetchCount(
			this OrganizationServiceContext serviceContext,
			string entityLogicalName,
			string countAttributeLogicalName,
			Action<Action<string, string, string>> addFilterConditions,
			Action<Action<string, string, string>> addOrFilterConditions = null,
			Action<Action<string, string>> addBinaryFilterConditions = null)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""countcolumn"" distinct=""true"" alias=""count"" />
						<filter type=""and"" />
						<filter type=""or"" />
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();
			entity.SetAttributeValue("name", entityLogicalName);

			entity.Descendants("attribute").First().SetAttributeValue("name", countAttributeLogicalName);

			var andFilter = entity.Descendants("filter").First();

			addFilterConditions(andFilter.AddFetchXmlFilterCondition);
			if (addBinaryFilterConditions != null)
			{
				addBinaryFilterConditions(andFilter.AddFetchXmlFilterCondition);
			}

			if (addOrFilterConditions != null)
			{
				var orFilter = entity.Descendants("filter").Last();

				addOrFilterConditions(orFilter.AddFetchXmlFilterCondition);
			}

			var response = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()), false, false, RequestFlag.AllowStaleData);

			return response.GetAttributeAliasedValue<int>("count");
		}

		public static IDictionary<Guid, int> FetchCounts(
			this OrganizationServiceContext serviceContext,
			string entityLogicalName,
			string countAttributeLogicalName,
			string linkEntityLogicalName,
			string linkFromAttributeLogicalName,
			string linkToAttributeLogicalName, IEnumerable<Guid> linkEntitiesIds,
			Action<Action<string, string, string>> addFilterConditions,
			Action<Action<string, string, string>> addOrFilterConditions = null)
		{
			var ids = linkEntitiesIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""countcolumn"" alias=""count"" />
						<filter type=""and"" />
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

			addFilterConditions(filter.AddFetchXmlFilterCondition);

			if (addOrFilterConditions != null)
			{
				var orFilter = new XElement("filter");
				orFilter.SetAttributeValue("type", "or");

				addOrFilterConditions(orFilter.AddFetchXmlFilterCondition);

				filter.AddAfterSelf(orFilter);
			}

			var linkEntity = entity.Descendants("link-entity").First();
			linkEntity.SetAttributeValue("name", linkEntityLogicalName);
			linkEntity.SetAttributeValue("from", linkFromAttributeLogicalName);
			linkEntity.SetAttributeValue("to", linkToAttributeLogicalName);

			var linkEntityAttribute = linkEntity.Descendants("attribute").First();
			linkEntityAttribute.SetAttributeValue("name", linkFromAttributeLogicalName);

			var linkEntityFilter = linkEntity.Descendants("filter").First();

			foreach (var id in ids)
			{
				linkEntityFilter.AddFetchXmlFilterCondition(linkFromAttributeLogicalName, "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()), RequestFlag.AllowStaleData);

			var results = ids.ToDictionary(id => id, id => 0);

			foreach (var result in response.Entities)
			{
				var id = result.GetAttributeAliasedValue<Guid>("id");
				var count = result.GetAttributeAliasedValue<int>("count");

				results[id] = count;
			}

			return results;
		}

		public static IDictionary<Guid, Tuple<string, string>> FetchArticleCommentExtendedData(
			this OrganizationServiceContext serviceContext, IEnumerable<Guid> commentIds)
		{
			var ids = commentIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""feedback"">
						<attribute name=""feedbackid"" />
						<filter type=""or"" />
						<link-entity name=""contact"" from=""contactid"" to=""createdbycontact"" alias=""author"">
							<attribute name=""fullname"" />
							<attribute name=""emailaddress1"" />
						</link-entity>
					</entity>
				</fetch>");

			var filter = fetchXml.Descendants("filter").First();

			foreach (var id in ids)
			{
				filter.AddFetchXmlFilterCondition("feedbackid", "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return ids.ToDictionary(id => id, id =>
			{
				var data = response.Entities.FirstOrDefault(e => e.GetAttributeValue<Guid>("feedbackid") == id);

				if (data == null)
				{
					return new Tuple<string, string>(null, null);
				}

				var authorName = data.GetAttributeAliasedValue<string>("fullname", "author");

				var authorEmail = data.GetAttributeAliasedValue<string>("emailaddress1", "author");

				return new Tuple<string, string>(authorName, authorEmail);
			});
		}

		public static IDictionary<Guid, int> FetchArticleCommentCounts(this OrganizationServiceContext serviceContext,
			IEnumerable<Guid> articleIds)
		{

			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return null;
			}

			return FetchCounts(serviceContext, "feedback", "feedbackid", "knowledgearticle", "knowledgearticleid",
				"regardingobjectid",
				articleIds, addCondition =>
				{
					addCondition("statecode", "eq", "0");
				});
		}
	}
}
