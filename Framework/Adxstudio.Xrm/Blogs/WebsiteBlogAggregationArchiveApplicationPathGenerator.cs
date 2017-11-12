/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Blogs
{
	internal class WebsiteBlogAggregationArchiveApplicationPathGenerator : IBlogArchiveApplicationPathGenerator
	{
		private Tuple<ApplicationPath> _archivePathCache;

		public WebsiteBlogAggregationArchiveApplicationPathGenerator(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public ApplicationPath GetAuthorPath(Guid authorId, EntityReference blog = null)
		{
			var archivePath = GetAggregationArchiveApplicationPath();

			if (archivePath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}author/{2}/".FormatWith(
					archivePath.AppRelativePath,
					archivePath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					authorId));
		}

		public ApplicationPath GetMonthPath(DateTime month, EntityReference blog = null)
		{
			var archivePath = GetAggregationArchiveApplicationPath();

			if (archivePath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}{2:yyyy}/{2:MM}/".FormatWith(
					archivePath.AppRelativePath,
					archivePath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					month));
		}

		public ApplicationPath GetTagPath(string tag, EntityReference blog = null)
		{
			var archivePath = GetAggregationArchiveApplicationPath();

			if (archivePath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}tags/{2}".FormatWith(
					archivePath.AppRelativePath,
					archivePath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					HttpUtility.UrlPathEncode(tag)));
		}

		private ApplicationPath GetAggregationArchiveApplicationPath()
		{
			if (_archivePathCache != null)
			{
				return _archivePathCache.Item1;
			}

			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webpage")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id)
							}
						}
					},
					Links = new[]
					{
						new Link
						{
							Name = "adx_sitemarker",
							ToAttribute = "adx_webpageid",
							FromAttribute = "adx_pageid",
							Filters = new[]
							{
								new Filter
								{
									Conditions = new[]
									{
										new Condition("adx_pageid", ConditionOperator.NotNull),
										new Condition("adx_name", ConditionOperator.Equal, BlogSiteMapProvider.AggregationArchiveSiteMarkerName)
									}
								}
							}
						} 
					}
				}
			};

			var entity = serviceContext.RetrieveSingle(fetch);

			if (entity == null)
			{
				return null;
			}

			var urlProvider = Dependencies.GetUrlProvider();

			var archivePath = urlProvider.GetApplicationPath(serviceContext, entity);

			_archivePathCache = new Tuple<ApplicationPath>(archivePath);

			return archivePath;
		}
	}
}
