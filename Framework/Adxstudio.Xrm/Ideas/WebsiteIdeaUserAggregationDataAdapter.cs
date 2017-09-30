/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get aggregated Idea data for an Adxstudio Portals Website and user.
	/// </summary>
	/// <remarks>Ideas, Comments, and Votes are returned chronologically by their submission date.</remarks>
	public class WebsiteIdeaUserAggregationDataAdapter : IWebsiteIdeaUserAggregationDataAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="userId">The unique identifier of the portal user to aggregate data for.</param>
		/// <param name="dependencies">The dependencies to use for getting data.</param>
		public WebsiteIdeaUserAggregationDataAdapter(Guid userId, IDataAdapterDependencies dependencies)
		{
			dependencies.ThrowOnNull("dependencies");

			var website = dependencies.GetWebsite();
			website.ThrowOnNull("dependencies", ResourceManager.GetString("Website_Reference_Retrieval_Exception"));
			website.AssertLogicalName("adx_website");

			Website = website;
			Dependencies = dependencies;
			UserId = userId;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="userId">The unique identifier of the portal user to aggregate data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public WebsiteIdeaUserAggregationDataAdapter(Guid userId, string portalName = null) : this(userId, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected Guid UserId { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Website { get; private set; }

		/// <summary>
		/// Returns comments that have been posted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		public IEnumerable<IIdeaIdeaCommentPair> SelectIdeaComments(int startRowIndex = 0, int maximumRows = -1)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new IdeaIdeaCommentPair[] { };
			}
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdeaIdeaCommentPair[] { };
			}

			if (startRowIndex % maximumRows != 0)
			{
				throw new ArgumentException("maximumRows must be a factor of startRowIndex");
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			XDocument fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""feedback"">
						<all-attributes />
						<order attribute=""adx_date"" descending=""false"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
					    </filter>
					</entity>
				</fetch>");


			var entity = fetchXml.Descendants("entity").First();

			var filter = entity.Descendants("filter").First();

			filter.AddFetchXmlFilterCondition("adx_authorid", "eq", UserId.ToString());

			entity.AddFetchXmlLinkEntity("adx_idea", "adx_ideaid", "regardingobjectid",
				addCondition => addCondition("statecode", "eq", "0"),
				addNestedLinkEntity => addNestedLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid",
					addCondition =>
					{
						addCondition("adx_websiteid", "eq", Website.Id.ToString());
						addCondition("statecode", "eq", "0");
					}));

			var linkEntity = entity.Descendants("link-entity").First();
			linkEntity.SetAttributeValue("alias", "idea");
			linkEntity.Add(new XElement("all-attributes"));

			IEnumerable<Entity> entities;

			if (maximumRows < 0)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
				{
					Query = new FetchExpression(fetchXml.ToString())
				});

				entities = response.EntityCollection.Entities.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read)).ToArray();
			}
			else
			{
				var paginator = new FetchXmlPostFilterPaginator(serviceContext, fetchXml, e => security.TryAssert(serviceContext, e, CrmEntityRight.Read), 2);

				entities = paginator.Select(startRowIndex, maximumRows).ToArray();
			}

			var ideas = CreateIdeaEntitiesFromAliasedValues(entities);

			return new IdeaIdeaCommentPairFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(ideas, entities);
		}

		/// <summary>
		/// Returns ideas that have been submitted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		public IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdea[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			var includeUnapprovedIdeas = Dependencies.GetPortalUser() != null && UserId == Dependencies.GetPortalUser().Id;

			var query = serviceContext.CreateQuery("adx_idea")
				.Join(serviceContext.CreateQuery("adx_ideaforum"), idea => idea.GetAttributeValue<EntityReference>("adx_ideaforumid").Id, ideaForum => ideaForum.GetAttributeValue<Guid>("adx_ideaforumid"), (idea, ideaForum) => new { Idea = idea, IdeaForum = ideaForum })
				.Where(a => a.IdeaForum.GetAttributeValue<EntityReference>("adx_websiteid") == Website)
				.Where(a => a.Idea.GetAttributeValue<EntityReference>("adx_ideaforumid") != null && a.Idea.GetAttributeValue<EntityReference>("adx_authorid") == new EntityReference("contact", UserId)
					&& a.Idea.GetAttributeValue<OptionSetValue>("statecode") != null && a.Idea.GetAttributeValue<OptionSetValue>("statecode").Value == 0);

			if (!includeUnapprovedIdeas)
			{
				query = query.Where(a => a.Idea.GetAttributeValue<bool?>("adx_approved") == true);
			}

			query = query.OrderBy(a => a.Idea.GetAttributeValue<DateTime?>("adx_date"));

			if (maximumRows < 0)
			{
				var entities = query.Select(a => a.Idea).ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
					.Skip(startRowIndex);

				return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(entities);
			}

			var pagedQuery = query.Select(a => a.Idea);

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => security.TryAssert(serviceContext, e, CrmEntityRight.Read),
				2);

			return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(paginator.Select(startRowIndex, maximumRows));
		}

		/// <summary>
		/// Returns votes that have been casted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		public IEnumerable<IIdeaIdeaVotePair> SelectIdeaVotes(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdeaIdeaVotePair[] { };
			}

			if (startRowIndex % maximumRows != 0)
			{
				throw new ArgumentException("maximumRows must be a factor of startRowIndex");
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""feedback"">
						<all-attributes />
						<order attribute=""createdon"" descending=""false"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""rating"" operator=""not-null"" />
					    </filter>
					</entity>
				</fetch>");

			var entity = fetchXml.Descendants("entity").First();

			var filter = entity.Descendants("filter").First();

			filter.AddFetchXmlFilterCondition("adx_voterid", "eq", UserId.ToString());

			entity.AddFetchXmlLinkEntity("adx_idea", "adx_ideaid", "regardingobjectid",
				addCondition => addCondition("statecode", "eq", "0"),
				addNestedLinkEntity => addNestedLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid",
					addCondition =>
					{
						addCondition("adx_websiteid", "eq", Website.Id.ToString());
						addCondition("statecode", "eq", "0");
					}));

			var linkEntity = entity.Descendants("link-entity").First();
			linkEntity.SetAttributeValue("alias", "idea");
			linkEntity.Add(new XElement("all-attributes"));

			IEnumerable<Entity> entities;

			if (maximumRows < 0)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
				{
					Query = new FetchExpression(fetchXml.ToString())
				});

				entities = response.EntityCollection.Entities.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read)).ToArray();
			}
			else
			{
				var paginator = new FetchXmlPostFilterPaginator(serviceContext, fetchXml, e => security.TryAssert(serviceContext, e, CrmEntityRight.Read), 2);

				entities = paginator.Select(startRowIndex, maximumRows).ToArray();
			}

			var ideas = CreateIdeaEntitiesFromAliasedValues(entities);

			return new IdeaIdeaVotePairFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(ideas, entities);
		}

		/// <summary>
		/// Returns the number of comments that have been posted for the website and user this adapter applies to.
		/// </summary>
		public int SelectIdeaCommentCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCount("feedback", "feedbackid",
				addCondition =>
				{
					addCondition("adx_authorid", "eq", UserId.ToString());
					addCondition("statecode", "eq", "0");
				},
				addLinkEntity => addLinkEntity("adx_idea", "regardingobjectid", "adx_ideaid",
					addCondition => addCondition("statecode", "eq", "0"),
					addNestedLinkEntity => addNestedLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid",
						addCondition =>
						{
							addCondition("adx_websiteid", "eq", Website.Id.ToString());
							addCondition("statecode", "eq", "0");
						})),
				addBinaryFilterConditions => addBinaryFilterConditions("comments", "not-null"));
		}

		/// <summary>
		/// Returns the number of ideas that have been submitted for the website and user this adapter applies to.
		/// </summary>
		public int SelectIdeaCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIdeas = Dependencies.GetPortalUser() != null && UserId == Dependencies.GetPortalUser().Id;

			return serviceContext.FetchCount("adx_idea", "adx_ideaid",
				addCondition =>
				{
					addCondition("adx_authorid", "eq", UserId.ToString());
					addCondition("statecode", "eq", "0");

					if (!includeUnapprovedIdeas)
					{
						addCondition("adx_approved", "eq", "true");
					}
				},
				addLinkEntity => addLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid",
					addCondition =>
					{
						addCondition("adx_websiteid", "eq", Website.Id.ToString());
						addCondition("statecode", "eq", "0");
					},
					null));
		}

		/// <summary>
		/// Returns the number of votes that have been casted for the website and user this adapter applies to.
		/// </summary>
		public int SelectIdeaVoteCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCount("feedback", "adx_ideavoteid",
				addCondition =>
				{
					addCondition("createdbycontactid", "eq", UserId.ToString());
					addCondition("statecode", "eq", "0");
				},
				addLinkEntity => addLinkEntity("adx_idea", "adx_ideaid", "regardingobjectid",
					addCondition => addCondition("statecode", "eq", "0"),
					addNestedLinkEntity => addNestedLinkEntity("adx_ideaforum", "adx_ideaforumid", "adx_ideaforumid",
						addCondition =>
						{
							addCondition("adx_websiteid", "eq", Website.Id.ToString());
							addCondition("statecode", "eq", "0");
						})),
				addBinaryFilterConditions => addBinaryFilterConditions("rating", "not-null"));
		}

		private static IEnumerable<Entity> CreateIdeaEntitiesFromAliasedValues(IEnumerable<Entity> results)
		{
			var ideas = results
				.Select(result =>
				{
					var idea = new Entity("adx_idea");

					foreach (var attribute in result.Attributes.Where(attribute => attribute.Value is AliasedValue && attribute.Key.StartsWith("idea.")))
					{
						idea[attribute.Key.Substring(5)] = ((AliasedValue)attribute.Value).Value;
					}

					idea.Id = idea.GetAttributeValue<Guid>("adx_ideaid");

					return idea;
				}).ToArray();

			var distinctIdeaIds = ideas.Select(e => e.Id).Distinct();
			var distinctIdeas = distinctIdeaIds.Select(id => ideas.First(idea => idea.Id == id));

			return distinctIdeas;
		}
	}
}
