/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// This DataAdapter is to be used with 'legacy' solutions before the VotesSum and VoteCount rollup fields
	/// </summary>
	/// <remarks>Ideas are returned ordered by the number of votes it has (highest first).</remarks>
	public class IdeaForumByHotDataAdapterPreRollup : IdeaForumByHotDataAdapter, IRollupFreeIdeaForumDataAdapter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IdeaForumByHotDataAdapterPreRollup" /> class.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaForumByHotDataAdapterPreRollup(Entity ideaForum, string portalName = null)
			: base(ideaForum, portalName)
		{

		}

		/// <summary>
		/// Returns ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		/// <returns>type: IEnumerable</returns>
		public override IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1)
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

			var includeUnapprovedIdeas = TryAssertIdeaPreviewPermission(serviceContext);

			var feedbackConditions = new List<Condition>
			{
				new Condition("statecode", ConditionOperator.Equal, 0),
				new Condition("rating", ConditionOperator.NotNull)
			};

			var linkEntityConditions = new List<Condition>
			{
				new Condition("statecode", ConditionOperator.Equal, 0),
				new Condition("adx_ideaforumid", ConditionOperator.Equal, IdeaForum.Id)
			};

			var pageInfo = Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows);

			var fetch = new Fetch
			{
				Aggregate = true,
				PageSize = pageInfo.Count,
				PageNumber = pageInfo.PageNumber,
				Entity = new FetchEntity()
				{
					Name = "feedback",
					Attributes = new List<FetchAttribute>
					{
						new FetchAttribute("feedbackid", "count", AggregateType.CountColumn),
					},
					Orders = new List<Order>
					{
						new Order
						{
							Alias = "count",
							Direction = OrderType.Descending,
						}
					},
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = feedbackConditions,
						}
					},
					Links = new List<Link>
					{
						new Link
						{
							Name = "adx_idea",
							FromAttribute = "adx_ideaid",
							ToAttribute = "regardingobjectid",
							Attributes = new List<FetchAttribute>
							{
								new FetchAttribute
								{
									Name = "adx_ideaid",
									Alias = "ideaid",
									GroupBy = true,
								}
							},
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = linkEntityConditions,
								}
							}
						}
					}
				}
			};

			if (MaxDate.HasValue)
			{
				linkEntityConditions.Add(new Condition("adx_date", ConditionOperator.LessThan, MaxDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)));
			}

			if (MinDate.HasValue)
			{
				linkEntityConditions.Add(new Condition("adx_date", ConditionOperator.GreaterThan, MinDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)));
			}

			if (!includeUnapprovedIdeas)
			{
				linkEntityConditions.Add(new Condition("adx_approved", ConditionOperator.Equal, "true"));
			}

			if (Status.HasValue)
			{
				linkEntityConditions.Add(new Condition("statuscode", ConditionOperator.Equal, (int)Status.Value));
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

			var query = response.EntityCollection.Entities.Select(e => e.GetAttributeAliasedValue<Guid>("ideaid"));

			var ideaIds = query.ToList();

			if (!ideaIds.Any())
			{
				return new IIdea[] { };
			}

			var ideaConditions = new List<Condition>();
			var ideaFetch = new Fetch
			{
				Entity = new FetchEntity
				{
					Name = "adx_idea",
					Attributes = FetchAttribute.All,
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.Or,
							Conditions = ideaConditions,
						}
					}
				}
			};

			ideaConditions.AddRange(ideaIds.Select(ideaId => new Condition("adx_ideaid", ConditionOperator.Equal, ideaId)));

			var ideasFetchResponse = (RetrieveMultipleResponse)serviceContext.Execute(ideaFetch.ToRetrieveMultipleRequest());

			var ideas = ideasFetchResponse.EntityCollection.Entities.OrderBy(idea => ideaIds.IndexOf(idea.Id));

			return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(ideas);
		}
	}
}
