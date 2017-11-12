/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Tagging;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Blogs
{
	public class WebsiteBlogAggregationDataAdapter : IBlogAggregationDataAdapter
	{
		public WebsiteBlogAggregationDataAdapter(IDataAdapterDependencies dependencies) 
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Website = dependencies.GetWebsite();

			if (Website == null)
			{
				throw new ArgumentException("Unable to get website reference.", "dependencies");
			}

			Dependencies = dependencies;
		}

		public WebsiteBlogAggregationDataAdapter(IDataAdapterDependencies dependencies,
			Func<OrganizationServiceContext, IQueryable<Entity>> selectBlogEntities = null,
			Func<OrganizationServiceContext, IQueryable<Entity>> selectBlogPostEntities = null, 
			string portalName = null) : this(dependencies)
		{
			if (selectBlogEntities != null) { SelectBlogEntities = selectBlogEntities; }
			if (selectBlogPostEntities != null) { SelectBlogPostEntities = selectBlogPostEntities; }

		}
		
		private Func<OrganizationServiceContext, IQueryable<Entity>> _selectBlogEntities;

		private Func<OrganizationServiceContext, IQueryable<Entity>> _selectBlogPostEntities;

		protected List<string> ExcludeList; 

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Website { get; private set; }

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectBlogEntities 
		{
			get
			{
				if (_selectBlogEntities == null)
				{
					var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
					if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
					{
						// If multi-language is enabled, only select blogs that are language-agnostic or match the current language.
						_selectBlogEntities = serviceContext => serviceContext.CreateQuery("adx_blog")
							.Where(blog => blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website && 
								(blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null ||
								blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id));
					}
					else
					{
						_selectBlogEntities = serviceContext => serviceContext.CreateQuery("adx_blog")
							.Where(blog => blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website);
					}
				}
				return _selectBlogEntities;
			}
			private set { _selectBlogEntities = value; }
		}

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectBlogPostEntities
		{
			get
			{
				if (_selectBlogPostEntities == null)
				{
					_selectBlogPostEntities = serviceContext => serviceContext.GetAllBlogPostsInWebsite(Website.Id);
				}
				return _selectBlogPostEntities;
			}
			private set { _selectBlogPostEntities = value; }
		}


		public virtual IEnumerable<IBlog> SelectBlogs()
		{
			return SelectBlogs(0);
		}

		public virtual IEnumerable<IBlog> SelectBlogs(int startRowIndex, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IBlog[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();
			var urlProvider = Dependencies.GetUrlProvider();

			var query = SelectBlogEntities(serviceContext).ToList().OrderBy(blog => blog.GetAttributeValue<string>("adx_name"));

			if (maximumRows < 0)
			{
				return query.ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
					.Skip(startRowIndex)
					.Select(e => new Blog(e, urlProvider.GetApplicationPath(serviceContext, e), Dependencies.GetBlogFeedPath(e.Id)))
					.ToArray();
			}

			var pagedQuery = query;

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => security.TryAssert(serviceContext, e, CrmEntityRight.Read),
				2);

			return paginator.Select(startRowIndex, maximumRows)
				.Select(e => new Blog(e, urlProvider.GetApplicationPath(serviceContext, e), Dependencies.GetBlogFeedPath(e.Id))).ToArray();
		}

		public virtual int SelectBlogCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCount("adx_blog", "adx_blogid", addCondition => addCondition("adx_websiteid", "eq", Website.Id.ToString()));
		}

		public IBlog Select()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();
			var website = Dependencies.GetWebsite();
			var portalOrgService = Dependencies.GetRequestContext().HttpContext.GetOrganizationService();

			Entity page;

			var entity = TryGetPageBySiteMarker(portalOrgService, website, "Blog Home", out page)
				? page
				: TryGetPageBySiteMarker(portalOrgService, website, "Home", out page)
					? page
					: null;

			if (entity == null || !security.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}

			var urlProvider = Dependencies.GetUrlProvider();

			var path = urlProvider.GetApplicationPath(serviceContext, entity);

			return path == null ? null : new BlogAggregation(entity, path, Dependencies.GetBlogAggregationFeedPath());
		}

		public IBlog Select(Guid blogId)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", blogId));

			var blog = Select(e => e.GetAttributeValue<Guid>("adx_blogid") == blogId);

			if (blog == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", blogId));

			return blog;
		}

		public IBlog Select(string blogName)
		{
			if (string.IsNullOrEmpty(blogName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var blog = Select(e => e.GetAttributeValue<string>("adx_name") == blogName);

			if (blog == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return blog;
		}

		protected virtual IBlog Select(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();


			var publishingStateAccessProvider = new PublishingStateAccessProvider(Dependencies.GetRequestContext().HttpContext);

			// Bulk-load all ad entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_blog")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.ToArray();

			var entity = allEntities.FirstOrDefault(e =>
				match(e)
				&& IsActive(e)
				&& publishingStateAccessProvider.TryAssert(serviceContext, e));

			if (entity == null)
			{
				return null;
			}

			var securityProvider = Dependencies.GetSecurityProvider();
			var urlProvider = Dependencies.GetUrlProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Not Found", entity.Id));

				return null;
			}

			var blog = new Blog(entity, urlProvider.GetApplicationPath(serviceContext, entity), Dependencies.GetBlogFeedPath(entity.Id));

			return blog;

		}

		public IEnumerable<IBlogPost> SelectPosts()
		{
			return SelectPosts(0);
		}

		public virtual IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IBlogPost[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();
			var urlProvider = Dependencies.GetUrlProvider();

			var query = SelectBlogPostEntities(serviceContext);

			var blogPostFactory = new BlogPostFactory(serviceContext, urlProvider, Website, new WebsiteBlogAggregationArchiveApplicationPathGenerator(Dependencies));
			var blogReadPermissionCache = new Dictionary<Guid, bool>();

			if (maximumRows < 0)
			{
				return blogPostFactory.Create(query.ToArray()
					.Where(e => TryAssertBlogPostRight(serviceContext, security, e, CrmEntityRight.Read, blogReadPermissionCache))
					.Skip(startRowIndex));
			}

			var pagedQuery = query;

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => TryAssertBlogPostRight(serviceContext, security, e, CrmEntityRight.Read, blogReadPermissionCache),
				2);

			return blogPostFactory.Create(paginator.Select(startRowIndex, maximumRows));
		}
		
		public virtual int SelectPostCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchBlogPostCountForWebsite(Website.Id, addCondition => { });
		}

		public IEnumerable<IBlogArchiveMonth> SelectArchiveMonths()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var counts = serviceContext.FetchBlogPostCountsGroupedByMonthInWebsite(Website.Id);
			var archivePathGenerator = new WebsiteBlogAggregationArchiveApplicationPathGenerator(Dependencies);

			return counts.Select(c =>
			{
				var month = new DateTime(c.Item1, c.Item2, 1, 0, 0, 0, DateTimeKind.Utc);

				return new BlogArchiveMonth(month, c.Item3, archivePathGenerator.GetMonthPath(month));
			}).OrderByDescending(e => e.Month);
		}

		public IEnumerable<IBlogPostWeightedTag> SelectWeightedTags(int weights)
		{
			var serviceContext = Dependencies.GetServiceContext();

			var infos = serviceContext.FetchBlogPostTagCountsInWebsite(Website.Id)
				.Select(c => new BlogPostTagInfo(c.Item1, c.Item2));

			var tagCloudData = new TagCloudData(weights, TagInfo.TagComparer, infos);
			var archivePathGenerator = new WebsiteBlogAggregationArchiveApplicationPathGenerator(Dependencies);

			return tagCloudData.Select(e => new BlogPostWeightedTag(e.Name, archivePathGenerator.GetTagPath(e.Name), e.TaggedItemCount, e.Weight));
		}

		protected virtual bool TryAssertBlogPostRight(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, Entity blogPost, CrmEntityRight right, IDictionary<Guid, bool> blogPermissionCache)
		{
			if (blogPost == null)
			{
				throw new ArgumentNullException("blogPost");
			}

			if (blogPost.LogicalName != "adx_blogpost")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blogPost.LogicalName), "blogPost");
			}

			var blogReference = blogPost.GetAttributeValue<EntityReference>("adx_blogid");

			if (blogReference == null)
			{
				throw new ArgumentException(string.Format("Value must have entity reference attribute {0}.", "adx_blogid"), "blogPost");
			}

			bool cachedResult;

			if (blogPermissionCache.TryGetValue(blogReference.Id, out cachedResult))
			{
				return cachedResult;
			}
			
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_blog")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_blogid", ConditionOperator.Equal, blogReference.Id),
								new Condition("statecode", ConditionOperator.Equal, 0)
							}
						}
					}
				}
			};

			var blog = serviceContext.RetrieveSingle(fetch);
			var result = securityProvider.TryAssert(serviceContext, blog, right);
			blogPermissionCache[blogReference.Id] = result;

			return result;
		}

		private static bool TryGetPageBySiteMarker(IOrganizationService portalOrgService, EntityReference website, string siteMarker, out Entity page)
		{
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
										new Condition("adx_name", ConditionOperator.Equal, siteMarker),
										new Condition("adx_websiteid", ConditionOperator.Equal, website.Id) 
									}
								}
							}
						}
					}
				}
			};

			page = portalOrgService.RetrieveSingle(fetch);

			return page != null;
		}

		private static bool IsActive(Entity entity)
		{
			if (entity == null)
			{
				return false;
			}

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			return statecode != null && statecode.Value == 0;
		}
	}
}
