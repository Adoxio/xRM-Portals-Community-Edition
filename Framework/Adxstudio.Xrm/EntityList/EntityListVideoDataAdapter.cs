/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.EntityList
{
	public class EntityListVideoDataAdapter
	{
		public EntityListVideoDataAdapter(EntityReference video, IDataAdapterDependencies dependencies)
		{
			if (video == null) throw new ArgumentNullException("video");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Video = video;
			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected EntityReference Video { get; private set; }

		public Video SelectVideo()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var fetch = new Fetch
			{
				Version = "1.0",
				MappingType = MappingType.Logical,
				Entity = new FetchEntity
				{
					Name = Video.LogicalName,
					Attributes = new[]
					{
						new FetchAttribute("adx_title"),
						new FetchAttribute("adx_copy"),
						new FetchAttribute("adx_displaydate"),
						new FetchAttribute("adx_mediaembed"),
						new FetchAttribute("adx_mediaurl"),
					},
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("adx_videoid", ConditionOperator.Equal, Video.Id),
								new Condition("statecode", ConditionOperator.Equal, 0),
							}
						}
					},
					Links = new[]
					{
						new Link
						{
							Name = "adx_video_tag",
							FromAttribute = "adx_videoid",
							ToAttribute = "adx_videoid",
							Type = JoinOperator.LeftOuter,
							Links = new[]
							{
								new Link
								{
									Alias = "tag",
									Name = "adx_tag",
									FromAttribute = "adx_tagid",
									ToAttribute = "adx_tagid",
									Type = JoinOperator.LeftOuter,
									Attributes = new[]
									{
										new FetchAttribute("adx_name"),
									},
									Filters = new[]
									{
										new Filter
										{
											Conditions = new[]
											{
												new Condition("statecode", ConditionOperator.Equal, 0)
											}
										}
									}
								}
							}
						},
					},
				},
			};

			var entityGrouping = FetchEntities(serviceContext, fetch)
				.GroupBy(e => e.Id)
				.FirstOrDefault();

			if (entityGrouping == null)
			{
				return null;
			}

			var entity = entityGrouping.FirstOrDefault();

			if (entity == null)
			{
				return null;
			}

			var tags = entityGrouping.Select(e => e.GetAttributeAliasedValue<string>("adx_name", "tag")).OrderBy(tag => tag).ToList();

			return new Video
			{
				Title = entity.GetAttributeValue<string>("adx_title"),
				Copy = entity.GetAttributeValue<string>("adx_copy"),
				DisplayDate = entity.GetAttributeValue<DateTime?>("adx_displaydate"),
				MediaEmbed = entity.GetAttributeValue<string>("adx_mediaembed"),
				MediaUrl = entity.GetAttributeValue<string>("adx_mediaurl"),
				Tags = tags,
			};
		}

		protected virtual IEnumerable<Entity> FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			fetch.PageNumber = 1;

			while (true)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

				foreach (var entity in response.EntityCollection.Entities)
				{
					yield return entity;
				}

				if (!response.EntityCollection.MoreRecords || string.IsNullOrEmpty(response.EntityCollection.PagingCookie))
				{
					break;
				}

				fetch.PageNumber++;
				fetch.PagingCookie = response.EntityCollection.PagingCookie;
			}
		}
	}

	public class Video
	{
		public string Title { get; set; }

		public string Copy { get; set; }

		public DateTime? DisplayDate { get; set; }

		public string MediaEmbed { get; set; }

		public string MediaUrl { get; set; }

		public string CloudBlobUrl { get; set; }

		public IEnumerable<string> Tags { get; set; } 
	}
}
