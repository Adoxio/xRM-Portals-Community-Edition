/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Services;

namespace Adxstudio.Xrm.Ideas
{
	internal static class OrganizationServiceContextExtensions
	{
		public static int FetchCount(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, Action<Action<string, string, string>> addFilterConditions,
			Action<Action<string, string, string, Action<Action<string, string, string>>, Action<Action<string, string, string, Action<Action<string, string, string>>>>>> addLinkEntities = null, Action<Action<string, string>> addBinaryFilterConditions = null)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity>
						<attribute aggregate=""countcolumn"" distinct=""true"" alias=""count"" />
						<filter type=""and"" />
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();
			entity.SetAttributeValue("name", entityLogicalName);

			entity.Descendants("attribute").First().SetAttributeValue("name", countAttributeLogicalName);

			var filter = entity.Descendants("filter").First();

			addFilterConditions(filter.AddFetchXmlFilterCondition);
			if (addBinaryFilterConditions != null)
			{
				addBinaryFilterConditions(filter.AddFetchXmlFilterCondition);
			}


			if (addLinkEntities != null)
			{
				addLinkEntities(entity.AddFetchXmlLinkEntity);
			}

			var response = serviceContext.RetrieveSingle(Fetch.Parse(fetchXml.ToString()));

			return response.GetAttributeAliasedValue<int>("count");
		}

		public static IDictionary<Guid, int> FetchCounts(this OrganizationServiceContext serviceContext, string entityLogicalName, string countAttributeLogicalName, string linkEntityLogicalName, string linkFromAttributeLogicalName, string linkToAttributeLogicalName, IEnumerable<Guid> linkEntitiesIds, Action<Action<string, string, string>> addFilterConditions, Action<Action<string, string, string>> addOrFilterConditions = null, Action<Action<string, string>> addBinaryFilterConditions = null)
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

			if (addBinaryFilterConditions != null)
			{
				addBinaryFilterConditions(filter.AddFetchXmlFilterCondition);
			}

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

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			var results = ids.ToDictionary(id => id, id => 0);

			foreach (var result in response.Entities)
			{
				var id = result.GetAttributeAliasedValue<Guid>("id");
				var count = result.GetAttributeAliasedValue<int>("count");

				results[id] = count;
			}

			return results;
		}

		public static IDictionary<Guid, int> FetchIdeaCommentCounts(this OrganizationServiceContext serviceContext, IEnumerable<Guid> ideaIds)
		{
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return FetchCounts(serviceContext, "feedback", "feedbackid", "adx_idea", "adx_ideaid", "regardingobjectid", ideaIds,
				addCondition =>
				{
					addCondition("adx_approved", "eq", "true");
					addCondition("statecode", "eq", "0");
				},
				null,
				addBinaryFilterConditions => addBinaryFilterConditions("comments", "not-null"));
			}
			else
			{
				return new Dictionary<Guid, int>();
			}
		}

		public static IDictionary<Guid, Tuple<string, string>> FetchIdeaCommentExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> commentIds)
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
							<attribute name=""firstname"" />
							<attribute name=""lastname"" />
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


					var authorName = Localization.LocalizeFullName(data.GetAttributeAliasedValue<string>("author.firstname"), data.GetAttributeAliasedValue<string>("author.lastname"));

					var authorEmail = data.GetAttributeAliasedValue<string>("emailaddress1", "author");

					return new Tuple<string, string>(authorName, authorEmail);
				});
			}
			return new Dictionary<Guid, Tuple<string, string>>();
		}

		public static IDictionary<Guid, IdeaExtendedData> FetchIdeaExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> ideaIds)
		{
			var ids = ideaIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""adx_idea"">
						<attribute name=""adx_ideaid"" />
						<filter type=""or"" />
						<link-entity name=""adx_ideaforum"" from=""adx_ideaforumid"" to=""adx_ideaforumid"" alias=""ideaforum"">
							<attribute name=""adx_name"" />
							<attribute name=""adx_partialurl"" />
							<attribute name=""adx_commentpolicy"" />
							<attribute name=""adx_votingpolicy"" />
							<attribute name=""adx_votingtype"" />
							<attribute name=""adx_votesperidea"" />
							<attribute name=""adx_votesperuser"" />
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
				filter.AddFetchXmlFilterCondition("adx_ideaid", "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			return ids.ToDictionary(id => id, id =>
			{
				var data = response.Entities.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_ideaid") == id);

				if (data == null)
				{
					return IdeaExtendedData.Default;
				}

				var authorName = Localization.LocalizeFullName(data.GetAttributeAliasedValue<string>("author.firstname"), data.GetAttributeAliasedValue<string>("author.lastname"));

				var authorEmail = data.GetAttributeAliasedValue<string>("emailaddress1", "author");

				var ideaForumTitle = data.GetAttributeAliasedValue<string>("adx_name", "ideaforum");

				var ideaForumPartialUrl = data.GetAttributeAliasedValue<string>("adx_partialurl", "ideaforum");

				var ideaForumCommentPolicyValue = data.GetAttributeAliasedValue<int?>("adx_commentpolicy", "ideaforum");

				var ideaForumCommentPolicy = ideaForumCommentPolicyValue.HasValue
					? (IdeaForumCommentPolicy)Enum.ToObject(typeof(IdeaForumCommentPolicy), ideaForumCommentPolicyValue)
					: IdeaExtendedData.Default.IdeaForumCommentPolicy;

				var ideaForumVotingPolicyValue = data.GetAttributeAliasedValue<int?>("adx_votingpolicy", "ideaforum");

				var ideaForumVotingPolicy = ideaForumVotingPolicyValue.HasValue
					? (IdeaForumVotingPolicy)Enum.ToObject(typeof(IdeaForumVotingPolicy), ideaForumVotingPolicyValue)
					: IdeaExtendedData.Default.IdeaForumVotingPolicy;

				var ideaForumVotingTypeValue = data.GetAttributeAliasedValue<int?>("adx_votingtype", "ideaforum");

				var ideaForumVotingType = ideaForumVotingTypeValue.HasValue
					? (IdeaForumVotingType)Enum.ToObject(typeof(IdeaForumVotingType), ideaForumVotingTypeValue)
					: IdeaExtendedData.Default.IdeaForumVotingType;

				var ideaForumVotesPerIdeaValue = data.GetAttributeAliasedValue<int?>("adx_votesperidea", "ideaforum");

				var ideaForumVotesPerIdea = ideaForumVotesPerIdeaValue.HasValue
					? ideaForumVotesPerIdeaValue.Value
					: IdeaExtendedData.Default.IdeaForumVotesPerIdea;

				var ideaForumVotesPerUser = data.GetAttributeAliasedValue<int?>("adx_votesperuser", "ideaforum");

				return new IdeaExtendedData(authorName, authorEmail, ideaForumTitle, ideaForumPartialUrl, ideaForumCommentPolicy, ideaForumVotingPolicy, ideaForumVotingType, ideaForumVotesPerIdea, ideaForumVotesPerUser);
			});
		}

		public static Dictionary<Guid, int> FetchIdeaForumActiveVoteCountsForUser(this OrganizationServiceContext serviceContext, IEnumerable<Guid> ideaForumIds, HttpContextBase httpContext, EntityReference portalUser)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new Dictionary<Guid, int>();
			}

			var ids = ideaForumIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""rating"" alias=""votesum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<filter type=""or"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""rating"" operator=""not-null"" />
					    </filter>
						<link-entity name=""adx_idea"" from=""adx_ideaid"" to=""regardingobjectid"">
							<attribute name=""adx_ideaforumid"" alias=""ideaforumid"" groupby=""true"" />
							<filter type=""or"" />
							<filter type=""and"">
								<condition attribute=""statuscode"" operator=""eq"" value=""1"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();

			var orFilter = entity.Descendants("filter").First();

			if (!string.IsNullOrEmpty(httpContext.Request.AnonymousID))
			{
				orFilter.AddFetchXmlFilterCondition("createdbycontact", "eq", httpContext.Request.AnonymousID);
			}

			if (portalUser != null)
			{
				orFilter.AddFetchXmlFilterCondition("createdbycontact", "eq", portalUser.Id.ToString());
			}
			else
			{
				//author url?
				//orFilter.AddFetchXmlFilterCondition("adx_createdbyipaddress", "eq", httpContext.Request.UserHostAddress);
			}

			var ideaForumFilter = entity.Descendants("link-entity").First().Descendants("filter").First();

			foreach (var id in ids)
			{
				ideaForumFilter.AddFetchXmlFilterCondition("adx_ideaforumid", "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			var results = ids.ToDictionary(
				id => id,
				id =>
				{
					var totalVotes = response.Entities
						.Where(result => result.GetAttributeAliasedValue<Guid>("ideaforumid") == id)
						.Sum(result => Math.Abs(result.GetAttributeAliasedValue<int>("votesum")));

					return totalVotes;
				});

			return results;
		}

		public static Dictionary<Guid, Tuple<int, int>> FetchIdeaVoteCountsForUser(this OrganizationServiceContext serviceContext, IEnumerable<Entity> ideaEntities, HttpContextBase httpContext, EntityReference portalUser)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new Dictionary<Guid, Tuple<int, int>>();
			}

			var ideas = ideaEntities.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""rating"" alias=""votesum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<filter type=""or"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""rating"" operator=""not-null"" />
					    </filter>
						<link-entity name=""adx_idea"" from=""adx_ideaid"" to=""regardingobjectid"">
							<attribute name=""adx_ideaforumid"" alias=""ideaforumid"" groupby=""true"" />
							<attribute name=""adx_ideaid"" alias=""ideaid"" groupby=""true"" />
							<attribute name=""statuscode"" alias=""statuscode"" groupby=""true"" />
							<filter type=""or"" />
							<filter type=""and"">
								<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();

			var orFilter = entity.Descendants("filter").First();

			if (!string.IsNullOrEmpty(httpContext.Request.AnonymousID))
			{
				orFilter.AddFetchXmlFilterCondition("createdbycontact", "eq", httpContext.Request.AnonymousID);
			}

			if (portalUser != null)
			{
				orFilter.AddFetchXmlFilterCondition("createdbycontact", "eq", portalUser.Id.ToString());
			}
			else
			{
				// author_url?
				//orFilter.AddFetchXmlFilterCondition("adx_createdbyipaddress", "eq", httpContext.Request.UserHostAddress);
			}

			var ideaForumFilter = entity.Descendants("link-entity").First().Descendants("filter").First();

			foreach (var id in ideas.Where(idea => idea.GetAttributeValue<EntityReference>("adx_ideaforumid") != null).Select(idea => idea.GetAttributeValue<EntityReference>("adx_ideaforumid").Id).Distinct())
			{
				ideaForumFilter.AddFetchXmlFilterCondition("adx_ideaforumid", "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			var ideaForumActiveVoteCounts = response.Entities
				.Where(result => result.GetAttributeAliasedValue<int>("statuscode") == 1)
				.GroupBy(
					result => result.GetAttributeAliasedValue<Guid>("ideaforumid"),
					result => Math.Abs(result.GetAttributeAliasedValue<int>("votesum")))
				.ToDictionary(g => g.Key, ints => ints.Sum());

			var results = ideas.ToDictionary(
				idea => idea.Id,
				idea =>
				{
					int ideaForumActiveVoteCountValue;
					var ideaForumActiveVoteCount = idea.GetAttributeValue<EntityReference>("adx_ideaforumid") == null ? 0 : ideaForumActiveVoteCounts.TryGetValue(idea.GetAttributeValue<EntityReference>("adx_ideaforumid").Id, out ideaForumActiveVoteCountValue)
						? ideaForumActiveVoteCountValue
						: 0;

					var ideaVoteCount = response.Entities
						.Where(result => result.GetAttributeAliasedValue<Guid>("ideaid") == idea.Id)
						.Sum(result => Math.Abs(result.GetAttributeAliasedValue<int>("votesum")));

					return new Tuple<int, int>(ideaForumActiveVoteCount, ideaVoteCount);
				});

			return results;
		}

		public static Dictionary<Guid, Tuple<int, int, int>> FetchIdeaVoteCounts(this OrganizationServiceContext serviceContext, IEnumerable<Guid> ideaIds)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new Dictionary<Guid, Tuple<int, int, int>>();
			}

			var ids = ideaIds.ToArray();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""feedbackid"" alias=""totalvoters"" aggregate=""countcolumn"" />
						<attribute name=""rating"" alias=""votesum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""rating"" operator=""not-null"" />
					    </filter>
						<link-entity name=""adx_idea"" from=""adx_ideaid"" to=""regardingobjectid"">
							<attribute name=""adx_ideaid"" alias=""ideaid"" groupby=""true"" />
							<filter type=""or"" />
							<filter type=""and"">
								<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();

			var linkEntity = entity.Descendants("link-entity").First();

			var linkEntityFilter = linkEntity.Descendants("filter").First();

			foreach (var id in ids)
			{
				linkEntityFilter.AddFetchXmlFilterCondition("adx_ideaid", "eq", id.ToString());
			}

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(Fetch.Parse(fetchXml.ToString()));

			var results = ids.ToDictionary(
				id => id,
				id =>
				{
					var aggregates = response.Entities
						.Where(result => result.GetAttributeAliasedValue<Guid>("ideaid") == id);

					var upVoteSum = aggregates.Where(result => result.GetAttributeAliasedValue<int>("votesum") > 0)
						.Sum(result => result.GetAttributeAliasedValue<int>("votesum"));

					var downVoteSum = Math.Abs(aggregates.Where(result => result.GetAttributeAliasedValue<int>("votesum") < 0)
						.Sum(result => result.GetAttributeAliasedValue<int>("votesum")));

					var totalVoters = aggregates.Sum(result => result.GetAttributeAliasedValue<int>("totalvoters"));

					return new Tuple<int, int, int>(upVoteSum, downVoteSum, totalVoters);
				});

			return results;
		}
	}
}
