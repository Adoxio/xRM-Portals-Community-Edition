/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Issues
{
	internal static class OrganizationServiceContextExtensions
	{
		public static int FetchCount(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, Action<Action<string, string, string>> addFilterConditions, Action<Action<string, string, string>> addOrFilterConditions = null)
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

			if (addOrFilterConditions != null)
			{
				var orFilter = entity.Descendants("filter").Last();

				addOrFilterConditions(orFilter.AddFetchXmlFilterCondition);
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.First().GetAttributeAliasedValue<int>("count");
		}

		public static IDictionary<Guid, int> FetchCounts(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, string linkEntityLogicalName, string linkFromAttributeLogicalName, string linkToAttributeLogicalName, IEnumerable<Guid> linkEntitiesIds, Action<Action<string, string, string>> addFilterConditions, Action<Action<string, string, string>> addOrFilterConditions = null)
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

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			var results = ids.ToDictionary(id => id, id => 0);

			foreach (var result in response.EntityCollection.Entities)
			{
				var id = result.GetAttributeAliasedValue<Guid>("id");
				var count = result.GetAttributeAliasedValue<int>("count");

				results[id] = count;
			}

			return results;
		}

		public static IDictionary<Guid, int> FetchIssueCommentCounts(this OrganizationServiceContext serviceContext, IEnumerable<Guid> issueIds)
		{
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return FetchCounts(serviceContext, "feedback", "feedbackid", "adx_issue", "adx_issueid", "regardingobjectid", issueIds, addCondition =>
				{
					addCondition("adx_approved", "eq", "true");
					addCondition("statecode", "eq", "0");
				});
			}
			else
			{
				return new Dictionary<Guid, int>();
			}
		}

		public static IDictionary<Guid, Tuple<string, string>> FetchIssueCommentExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> commentIds)
		{
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
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

				var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
				{
					Query = new FetchExpression(fetchXml.ToString())
				});

				return ids.ToDictionary(id => id, id =>
				{
					var data = response.EntityCollection.Entities.FirstOrDefault(e => e.GetAttributeValue<Guid>("feedbackid") == id);

					if (data == null)
					{
						return new Tuple<string, string>(null, null);
					}

					var authorName = data.GetAttributeAliasedValue<string>("fullname", "author");

					var authorEmail = data.GetAttributeAliasedValue<string>("emailaddress1", "author");

					return new Tuple<string, string>(authorName, authorEmail);
				});
			}
			return new Dictionary<Guid, Tuple<string, string>>();
		}

		public static IDictionary<Guid, IssueExtendedData> FetchIssueExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> issueIds)
		{
			var ids = issueIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""adx_issue"">
						<attribute name=""adx_issueid"" />
						<filter type=""or"" />
						<link-entity name=""adx_issueforum"" from=""adx_issueforumid"" to=""adx_issueforumid"" alias=""issueforum"">
							<attribute name=""adx_name"" />
							<attribute name=""adx_partialurl"" />
							<attribute name=""adx_commentpolicy"" />
						</link-entity>
						<link-entity link-type=""outer"" name=""contact"" from=""contactid"" to=""adx_authorid"" alias=""author"">
							<attribute name=""fullname"" />
							<attribute name=""firstname"" />
							<attribute name=""lastname"" />
							<attribute name=""emailaddress1"" />
						</link-entity>
					</entity>
				</fetch>");

			var filter = fetchXml.Descendants("filter").First();

			foreach (var id in ids)
			{
				filter.AddFetchXmlFilterCondition("adx_issueid", "eq", id.ToString());
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return ids.ToDictionary(id => id, id =>
			{
				var data = response.EntityCollection.Entities.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_issueid") == id);

				if (data == null)
				{
					return IssueExtendedData.Default;
				}

				var authorName = Localization.LocalizeFullName(data.GetAttributeAliasedValue<string>("author.firstname"), data.GetAttributeAliasedValue<string>("author.lastname"));

				var authorEmail = data.GetAttributeAliasedValue<string>("emailaddress1", "author");

				var issueForumTitle = data.GetAttributeAliasedValue<string>("adx_name", "issueforum");

				var issueForumPartialUrl = data.GetAttributeAliasedValue<string>("adx_partialurl", "issueforum");

				var issueForumCommentPolicyValue = data.GetAttributeAliasedValue<int?>("adx_commentpolicy", "issueforum");

				var issueForumCommentPolicy = issueForumCommentPolicyValue.HasValue
					? (IssueForumCommentPolicy)Enum.ToObject(typeof(IssueForumCommentPolicy), issueForumCommentPolicyValue)
					: IssueExtendedData.Default.IssueForumCommentPolicy;

				return new IssueExtendedData(authorName, authorEmail, issueForumTitle, issueForumPartialUrl, issueForumCommentPolicy);
			});
		}
	}
}
