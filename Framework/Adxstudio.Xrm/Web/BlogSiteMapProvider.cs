/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// A <see cref="SiteMapProvider"/> for navigating 'adx_blog' <see cref="Entity"/> hierarchies.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <siteMap enabled="true">
	///    <providers>
	///     <add
	///      name="Blogs"
	///      type="Adxstudio.Xrm.Web.BlogSiteMapProvider"
	///      securityTrimmingEnabled="true"
	///      portalName="Xrm" [Microsoft.Xrm.Portal.Configuration.PortalContextElement]
	///     />
	///    </providers>
	///   </siteMap>
	///  </system.web>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class BlogSiteMapProvider : CrmSiteMapProviderBase, ISolutionDependent
	{
		public IEnumerable<string> RequiredSolutions
		{
			get { return new[] { "MicrosoftBlogs" }; }
		}
		public const string AggregationArchiveSiteMarkerName = "Blog Archive";

		private const string AuthorArchiveNodeAttributeKey = "Adxstudio.Xrm.Blogs.Archive.Author";
		private const string MonthArchiveNodeAttributeKey = "Adxstudio.Xrm.Blogs.Archive.Month";
		private const string TagArchiveNodeAttributeKey = "Adxstudio.Xrm.Blogs.Archive.Tag";

		private static readonly Regex AuthorArchivePathRegex = new Regex(@"^.*/(?<blog>[^/]+)/author/(?<author>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})/$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		private static readonly Regex MonthArchivePathRegex = new Regex(@"^.*/(?<blog>[^/]+)/(?<year>\d{4})/(?<month>\d{2})/$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		private static readonly Regex PathRegex = new Regex("^(.*/)(?<right>[^/]+)/$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		private static readonly Regex TagArchivePathRegex = new Regex(@"^.*/(?<blog>[^/]+)/tags/(?<tag>[^/]+)$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		private static readonly Regex WebFilePathRegex = new Regex("^(?<post>.*/)(?<file>[^/]+)$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.RightToLeft);

		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			TraceInfo("FindSiteMapNode({0})", rawUrl);

			var portal = PortalContext;
			var serviceContext = portal.ServiceContext;
			var website = portal.Website.ToEntityReference();

			var applicationPath = ApplicationPath.Parse(rawUrl);
			var path = new UrlBuilder(applicationPath.PartialPath).Path;

			CrmSiteMapNode node;

			if (TryGetAuthorArchiveNode(serviceContext, website, path, out node)
				|| TryGetMonthArchiveNode(serviceContext, website, path, out node))
			{
				return ReturnNodeIfAccessible(node, GetAccessDeniedNode);
			}

			if (TryGetBlogPostNodeById(serviceContext, website, path, out node)
				|| TryGetBlogNodeByPartialUrl(serviceContext, website, path, out node)
				|| TryGetBlogPostNodeByPartialUrl(serviceContext, website, path, out node))
			{
				return node;
			}

			if (TryGetTagArchiveNode(serviceContext, website, path, out node)
				|| TryGetWebFileNode(serviceContext, website, path, out node))
			{
				return ReturnNodeIfAccessible(node, GetAccessDeniedNode);
			}

			return null;
		}

		public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
		{
			TraceInfo("GetChildNodes({0})", node.Key);

			var children = new SiteMapNodeCollection();

			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null || !entityNode.HasCrmEntityName("adx_webpage"))
			{
				return children;
			}

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var entity = context.MergeClone(entityNode.Entity);

			var blogs = FindBlogs(context, website, entity);

			foreach (var blog in blogs)
			{
				var blogNode = GetBlogNode(context, blog);

				if (ChildNodeValidator.Validate(context, blogNode))
				{
					children.Add(blogNode);
				}
			}

			return children;
		}

		public override SiteMapNode GetParentNode(SiteMapNode node)
		{
			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null)
			{
				return null;
			}

			var portal = PortalContext;
			var serviceContext = portal.ServiceContext;

			var entity = serviceContext.MergeClone(entityNode.Entity);

			if (entityNode.HasCrmEntityName("adx_blog"))
			{
				DateTime monthArchiveDate;
				var authorArchive = entityNode[AuthorArchiveNodeAttributeKey];
				var tagArchive = entityNode[TagArchiveNodeAttributeKey];

				if (TryGetMonthArchiveNodeAttribute(node, out monthArchiveDate) || !string.IsNullOrEmpty(authorArchive) || !string.IsNullOrEmpty(tagArchive))
				{
					var blogNode = GetBlogNode(serviceContext, entity);

					return NodeValidator.Validate(serviceContext, blogNode) ? blogNode : null;
				}

				var page = entity.GetRelatedEntity(serviceContext, "adx_webpage_blog");

				return page == null ? null : SiteMap.Provider.FindSiteMapNode(serviceContext.GetUrl(page));
			}

			if (entityNode.HasCrmEntityName("adx_blogpost"))
			{
				var blog = entity.GetRelatedEntity(serviceContext, "adx_blog_blogpost");

				if (blog == null)
				{
					return null;
				}

				var blogNode = GetBlogNode(serviceContext, blog);

				return NodeValidator.Validate(serviceContext, blogNode) ? blogNode : null;
			}

			if (entityNode.HasCrmEntityName("adx_webfile"))
			{
				var blogPost = entity.GetRelatedEntity(serviceContext, "adx_blogpost_webfile");

				if (blogPost == null)
				{
					return null;
				}

				var blogPostNode = GetBlogPostNode(serviceContext, blogPost);

				return NodeValidator.Validate(serviceContext, blogPostNode) ? blogPostNode : null;
			}

			if (entityNode.HasCrmEntityName("adx_webpage") && node["IsAggregationArchiveNode"] == "true")
			{
				return SiteMap.Provider.FindSiteMapNode(serviceContext.GetUrl(entity));
			}

			return null;
		}

		protected virtual IEnumerable<Entity> FindBlogs(OrganizationServiceContext context, Entity website, Entity webpage)
		{
			var blogsInCurrentWebsite = context.CreateQuery("adx_blog")
				.Where(b => b.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()).ToArray();

			// Since blogs can only be children of language-root web pages, if the given web page is not a root, then use its root for query instead.
			EntityReference rootWebPage = webpage.ToEntityReference();
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled && !webpage.GetAttributeValue<bool>("adx_isroot"))
			{
				rootWebPage = webpage.GetAttributeValue<EntityReference>("adx_rootwebpageid");
			}

			var blogs = blogsInCurrentWebsite
				.Where(e => Equals(e.GetAttributeValue<EntityReference>("adx_parentpageid"), rootWebPage))
				.OrderBy(e => e.GetAttributeValue<string>("adx_name"));
			
			// Only find blogs that match the current language. 
			var blogsInCurrentLanguage = contextLanguageInfo.IsCrmMultiLanguageEnabled
				? blogs.Where(blog => blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null || blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id)
				: blogs;

			return blogsInCurrentLanguage;
		}

		protected CrmSiteMapNode GetBlogNode(OrganizationServiceContext serviceContext, Entity entity)
		{
			entity.AssertEntityName("adx_blog");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var blog = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetBlog(serviceContext, website, entity.Id);

			if (blog == null)
			{
				return null;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, blog);
			var pageTemplate = blog.GetRelatedEntity(serviceContext, "adx_pagetemplate_blog_home");

			// apply a detached clone of the entity since the SiteMapNode is out of the scope of the current OrganizationServiceContext
			var blogClone = blog.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				url,
				url,
				blog.GetAttributeValue<string>("adx_name"),
				blog.GetAttributeValue<string>("adx_summary"),
				GetRewriteUrl(pageTemplate, out webTemplateId),
				blog.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				blogClone);

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected string GetBlogArchiveRewriteUrl(OrganizationServiceContext serviceContext, Entity entity, out string webTemplateId)
		{
			entity.AssertEntityName("adx_blog");

			var pageTemplate = entity.GetRelatedEntity(serviceContext, "adx_pagetemplate_blog_archive");

			return GetRewriteUrl(pageTemplate, out webTemplateId);
		}

		protected CrmSiteMapNode GetBlogAuthorArchiveNode(OrganizationServiceContext serviceContext, Entity entity, Guid authorId)
		{
			entity.AssertEntityName("adx_blog");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var blog = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetBlog(serviceContext, website, entity.Id);

			if (blog == null)
			{
				return null;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, blog);
			var authorUrl = "{0}{1}author/{2}/".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", authorId);
			var blogClone = blog.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				authorUrl,
				authorUrl,
				GetBlogAuthorName(serviceContext, authorId),
				blog.GetAttributeValue<string>("adx_summary"),
				GetBlogArchiveRewriteUrl(serviceContext, entity, out webTemplateId),
				blog.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				blogClone);

			node[AuthorArchiveNodeAttributeKey] = authorId.ToString();

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogMonthArchiveNode(OrganizationServiceContext serviceContext, Entity entity, DateTime month)
		{
			entity.AssertEntityName("adx_blog");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var blog = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetBlog(serviceContext, website, entity.Id);

			if (blog == null)
			{
				return null;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, blog);
			var archiveUrl = "{0}{1}{2:yyyy}/{2:MM}/".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", month);
			var blogClone = blog.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				archiveUrl,
				archiveUrl,
				month.ToString("y", CultureInfo.CurrentCulture),
				blog.GetAttributeValue<string>("adx_summary"),
				GetBlogArchiveRewriteUrl(serviceContext, entity, out webTemplateId),
				blog.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				blogClone);

			node[MonthArchiveNodeAttributeKey] = month.ToString("o", CultureInfo.InvariantCulture);

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogTagArchiveNode(OrganizationServiceContext serviceContext, Entity entity, string tagSlug)
		{
			entity.AssertEntityName("adx_blog");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var blog = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetBlog(serviceContext, website, entity.Id);

			if (blog == null)
			{
				return null;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, blog);
			var tagUrl = "{0}{1}{2}".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", tagSlug);
			var tag = HttpUtility.UrlDecode(tagSlug).Trim();
			var blogClone = blog.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				tagUrl,
				tagUrl,
				tag,
				blog.GetAttributeValue<string>("adx_summary"),
				GetBlogArchiveRewriteUrl(serviceContext, entity, out webTemplateId),
				blog.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				blogClone);

			node[TagArchiveNodeAttributeKey] = tag;

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogAggregationAuthorArchiveNode(OrganizationServiceContext serviceContext, Entity entity, Guid authorId)
		{
			entity.AssertEntityName("adx_webpage");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var page = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetPage(serviceContext, website, entity.Id);

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, page);
			var archiveUrl = "{0}{1}author/{2}/".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", authorId);

			var pageTemplate = page.GetRelatedEntity(serviceContext, "adx_pagetemplate_webpage");
			var pageClone = page.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				archiveUrl,
				archiveUrl,
				GetBlogAuthorName(serviceContext, authorId),
				page.GetAttributeValue<string>("adx_summary"),
				GetRewriteUrl(pageTemplate, out webTemplateId),
				page.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				pageClone);

			node[AuthorArchiveNodeAttributeKey] = authorId.ToString();
			node["IsAggregationArchiveNode"] = "true";

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogAggregationMonthArchiveNode(OrganizationServiceContext serviceContext, Entity entity, DateTime month)
		{
			entity.AssertEntityName("adx_webpage");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var page = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetPage(serviceContext, website, entity.Id);

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, page);
			var archiveUrl = "{0}{1}{2:yyyy}/{2:MM}/".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", month);

			var pageTemplate = page.GetRelatedEntity(serviceContext, "adx_pagetemplate_webpage");
			var pageClone = page.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				archiveUrl,
				archiveUrl,
				month.ToString("y", CultureInfo.CurrentCulture),
				page.GetAttributeValue<string>("adx_summary"),
				GetRewriteUrl(pageTemplate, out webTemplateId),
				page.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				pageClone);

			node[MonthArchiveNodeAttributeKey] = month.ToString("o", CultureInfo.InvariantCulture);
			node["IsAggregationArchiveNode"] = "true";

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogAggregationTagArchiveNode(OrganizationServiceContext serviceContext, Entity entity, string tagSlug)
		{
			entity.AssertEntityName("adx_webpage");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var page = serviceContext.IsAttached(entity) && Equals(entity.GetAttributeValue<EntityReference>("adx_websiteid"), website)
				? entity
				: GetPage(serviceContext, website, entity.Id);

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, page);
			var tagUrl = "{0}{1}{2}".FormatWith(url, url.EndsWith("/") ? string.Empty : "/", tagSlug);
			var tag = HttpUtility.UrlDecode(tagSlug).Trim();

			var pageTemplate = page.GetRelatedEntity(serviceContext, "adx_pagetemplate_webpage");
			var pageClone = page.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				tagUrl,
				tagUrl,
				tag,
				page.GetAttributeValue<string>("adx_summary"),
				GetRewriteUrl(pageTemplate, out webTemplateId),
				page.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				pageClone);

			node[TagArchiveNodeAttributeKey] = tag;
			node["IsAggregationArchiveNode"] = "true";

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetBlogPostNode(OrganizationServiceContext serviceContext, Entity entity)
		{
			entity.AssertEntityName("adx_blogpost");

			var portal = PortalContext;
			var website = portal.Website.ToEntityReference();

			var post = serviceContext.IsAttached(entity)
				? entity
				: GetBlogPost(serviceContext, entity.Id);

			if (post == null || post.GetAttributeValue<EntityReference>("adx_blogid") == null)
			{
				return null;
			}

			var pageTemplateQuery = from p in serviceContext.CreateQuery("adx_pagetemplate")
				join b in serviceContext.CreateQuery("adx_blog") on p.GetAttributeValue<Guid>("adx_pagetemplateid") equals b.GetAttributeValue<EntityReference>("adx_blogpostpagetemplateid").Id
				where b.GetAttributeValue<EntityReference>("adx_blogpostpagetemplateid") != null && b.GetAttributeValue<Guid>("adx_blogid") == post.GetAttributeValue<EntityReference>("adx_blogid").Id
				where p.GetAttributeValue<EntityReference>("adx_websiteid") == website
				select p;

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, post);
			var pageTemplate = pageTemplateQuery.FirstOrDefault();
			var postClone = post.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				url,
				url,
				post.GetAttributeValue<string>("adx_name"),
				post.GetAttributeValue<string>("adx_summary"),
				GetRewriteUrl(pageTemplate, out webTemplateId),
				post.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				postClone);

			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected override SiteMapNode GetRootNodeCore()
		{
			TraceInfo("GetRootNodeCore()");

			return null;
		}

		protected override CrmSiteMapNode GetNode(OrganizationServiceContext context, Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var entityName = entity.LogicalName;

			if (entityName == "adx_blog")
			{
				return GetBlogNode(context, entity);
			}

			if (entityName == "adx_blogpost")
			{
				return GetBlogPostNode(context, entity);
			}

			throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
		}

		protected virtual bool EntityHasPath(OrganizationServiceContext context, Entity entity, string path)
		{
			var entityPath = OrganizationServiceContextExtensions.GetApplicationPath(context, entity);

			if (entityPath == null)
			{
				return false;
			}

			var resultPath = entityPath.PartialPath;

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled && ContextLanguageInfo.DisplayLanguageCodeInUrl)
			{
				resultPath = contextLanguageInfo.StripLanguageCodeFromAbsolutePath(entityPath.PartialPath);
			}

			return string.Equals(path, resultPath);
		}

		protected virtual bool TryGetBlogNodeByPartialUrl(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = PathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			var filter = new Filter
			{
				Conditions = new[]
				{
					new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
					new Condition("adx_partialurl", ConditionOperator.Equal, pathMatch.Groups["right"].Value)
				}
			};

			var languageInfo = HttpContext.Current.GetContextLanguageInfo();
			var mlpFilter = new Filter();
			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				mlpFilter.Type = LogicalOperator.Or;
				mlpFilter.Conditions = new[]
				{
					new Condition("adx_websitelanguageid", ConditionOperator.Null),
					new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id)
				};
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_blog")
				{
					Filters = new[] { filter, mlpFilter }
				}
			};

			var blogs = serviceContext.RetrieveMultiple(fetch);
			var blogWithMatchingPath = blogs.Entities.FirstOrDefault(e => this.EntityHasPath(serviceContext, e, path));

			if (blogWithMatchingPath != null)
			{
				node = this.GetAccessibleNodeOrAccessDeniedNode(serviceContext, blogWithMatchingPath);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetBlogPostNodeById(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = PathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			Guid postId;

			// If the right-most path segment is a Guid, try look up a post by that ID. Posts can have
			// their partial URL be a their ID, as the adx_partialurl attribute is not required.
			if (!Guid.TryParse(pathMatch.Groups["right"].Value, out postId))
			{
				return false;
			}

			var filter = new Filter
			{
				Conditions = new[]
				{
					new Condition("adx_websiteid", ConditionOperator.Equal, website.Id)
				}
			};

			var languageInfo = HttpContext.Current.GetContextLanguageInfo();
			var mlpFilter = new Filter();
			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				mlpFilter.Type = LogicalOperator.Or;
				mlpFilter.Conditions = new[]
				{
					new Condition("adx_websitelanguageid", ConditionOperator.Null),
					new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id)
				};
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_blogpost")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_blogid", ConditionOperator.NotNull),
								new Condition("adx_blogpostid", ConditionOperator.Equal, postId)
							}
						}
					},
					Links = new[]
					{
						new Link
						{
							Name = "adx_blog",
							ToAttribute = "adx_blogid",
							FromAttribute = "adx_blogid",
							Filters = new[] { filter, mlpFilter }
						}
					}
				}
			};

			var posts = serviceContext.RetrieveMultiple(fetch);
			var postWithMatchingPath = posts.Entities.FirstOrDefault(e => this.EntityHasPath(serviceContext, e, path));

			if (postWithMatchingPath != null)
			{
				node = this.GetAccessibleNodeOrAccessDeniedNode(serviceContext, postWithMatchingPath);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetBlogPostByPartialUrl(OrganizationServiceContext serviceContext, EntityReference website, string path, out Entity blogPost)
		{
			blogPost = null;

			var pathMatch = PathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			var filter = new Filter 
			{
				Conditions = new[]
				{
					new Condition("adx_websiteid", ConditionOperator.Equal, website.Id)
				}
			};

			var languageInfo = HttpContext.Current.GetContextLanguageInfo();
			var mlpFilter = new Filter();
			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				mlpFilter.Type = LogicalOperator.Or;
				mlpFilter.Conditions = new[]
				{
					new Condition("adx_websitelanguageid", ConditionOperator.Null),
					new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id)
				};
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_blogpost")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_blogid", ConditionOperator.NotNull),
								new Condition("adx_partialurl", ConditionOperator.Equal, pathMatch.Groups["right"].Value)
							}
						}
					},
					Links = new[]
					{
						new Link
						{
							Name = "adx_blog",
							ToAttribute = "adx_blogid",
							FromAttribute = "adx_blogid",
							Filters = new[] { filter, mlpFilter }
						}
					}
				}
			};

			var posts = serviceContext.RetrieveMultiple(fetch);
			blogPost = posts.Entities.FirstOrDefault(e => this.EntityHasPath(serviceContext, e, path));

			return blogPost != null;
		}

		protected virtual bool TryGetBlogPostNodeByPartialUrl(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			Entity postWithMatchingPath;

			if (TryGetBlogPostByPartialUrl(serviceContext, website, path, out postWithMatchingPath))
			{
				node = GetAccessibleNodeOrAccessDeniedNode(serviceContext, postWithMatchingPath);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetAuthorArchiveNode(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = AuthorArchivePathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			var archiveRootPath = Regex.Replace(path, @"author/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/$", string.Empty);

			var blogAggregationArchivePageQuery = from page in serviceContext.CreateQuery("adx_webpage")
				join siteMarker in serviceContext.CreateQuery("adx_sitemarker") on page.GetAttributeValue<Guid>("adx_webpageid") equals siteMarker.GetAttributeValue<EntityReference>("adx_pageid").Id
				where siteMarker.GetAttributeValue<EntityReference>("adx_pageid") != null && siteMarker.GetAttributeValue<string>("adx_name") == AggregationArchiveSiteMarkerName
				where page.GetAttributeValue<EntityReference>("adx_websiteid") == website
				select page;

			var blogAggregationArchivePageMatch = blogAggregationArchivePageQuery.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			Guid authorId;

			if (blogAggregationArchivePageMatch != null && Guid.TryParse(pathMatch.Groups["author"].Value, out authorId))
			{
				node = GetBlogAggregationAuthorArchiveNode(serviceContext, blogAggregationArchivePageMatch, authorId);

				return true;
			}

			var blogsByAuthorArchivePathMatch = from blog in serviceContext.CreateQuery("adx_blog")
				where blog.GetAttributeValue<EntityReference>("adx_websiteid") == website
				where blog.GetAttributeValue<string>("adx_partialurl") == pathMatch.Groups["blog"].Value
				select blog;

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				blogsByAuthorArchivePathMatch = blogsByAuthorArchivePathMatch.Where(
					blog => blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null ||
						blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id);
			}

			var blogByAuthorArchivePathMatch = blogsByAuthorArchivePathMatch.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			if (blogByAuthorArchivePathMatch != null && Guid.TryParse(pathMatch.Groups["author"].Value, out authorId))
			{
				node = GetBlogAuthorArchiveNode(serviceContext, blogByAuthorArchivePathMatch, authorId);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetMonthArchiveNode(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = MonthArchivePathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			DateTime date;

			try
			{
				date = new DateTime(
					int.Parse(pathMatch.Groups["year"].Value),
					int.Parse(pathMatch.Groups["month"].Value),
					1, 0, 0, 0,
					DateTimeKind.Utc);
			}
			catch
			{
				return false;
			}

			var archiveRootPath = Regex.Replace(path, @"\d{4}/\d{2}/$", string.Empty);

			var blogAggregationArchivePageQuery = from page in serviceContext.CreateQuery("adx_webpage")
				join siteMarker in serviceContext.CreateQuery("adx_sitemarker") on page.GetAttributeValue<Guid>("adx_webpageid") equals siteMarker.GetAttributeValue<EntityReference>("adx_pageid").Id
				where siteMarker.GetAttributeValue<EntityReference>("adx_pageid") != null && siteMarker.GetAttributeValue<string>("adx_name") == AggregationArchiveSiteMarkerName
				where page.GetAttributeValue<EntityReference>("adx_websiteid") == website
				select page;

			var blogAggregationArchivePageMatch = blogAggregationArchivePageQuery.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			if (blogAggregationArchivePageMatch != null)
			{
				node = GetBlogAggregationMonthArchiveNode(serviceContext, blogAggregationArchivePageMatch, date);

				return true;
			}

			var blogsByMonthArchivePathMatch = from blog in serviceContext.CreateQuery("adx_blog")
				where blog.GetAttributeValue<EntityReference>("adx_websiteid") == website
				where blog.GetAttributeValue<string>("adx_partialurl") == pathMatch.Groups["blog"].Value
				select blog;

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				blogsByMonthArchivePathMatch = blogsByMonthArchivePathMatch.Where(
					blog => blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null ||
						blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id);
			}

			var blogByMonthArchivePath = blogsByMonthArchivePathMatch.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			if (blogByMonthArchivePath != null)
			{
				node = GetBlogMonthArchiveNode(serviceContext, blogByMonthArchivePath, date);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetTagArchiveNode(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = TagArchivePathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			var archiveRootPath = Regex.Replace(path, @"tags/[^/]+$", string.Empty);
			var tagSlug = pathMatch.Groups["tag"].Value;

			var blogAggregationArchivePageQuery = from page in serviceContext.CreateQuery("adx_webpage")
				join siteMarker in serviceContext.CreateQuery("adx_sitemarker") on page.GetAttributeValue<Guid>("adx_webpageid") equals siteMarker.GetAttributeValue<EntityReference>("adx_pageid").Id
				where siteMarker.GetAttributeValue<EntityReference>("adx_pageid") != null && siteMarker.GetAttributeValue<string>("adx_name") == AggregationArchiveSiteMarkerName
				where page.GetAttributeValue<EntityReference>("adx_websiteid") == website
				select page;

			var blogAggregationArchivePageMatch = blogAggregationArchivePageQuery.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			if (blogAggregationArchivePageMatch != null)
			{
				node = GetBlogAggregationTagArchiveNode(serviceContext, blogAggregationArchivePageMatch, tagSlug);

				return true;
			}

			var blogsByTagArchivePathMatch = from blog in serviceContext.CreateQuery("adx_blog")
				where blog.GetAttributeValue<EntityReference>("adx_websiteid") == website
				where blog.GetAttributeValue<string>("adx_partialurl") == pathMatch.Groups["blog"].Value
				select blog;

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				blogsByTagArchivePathMatch = blogsByTagArchivePathMatch.Where(
					blog => blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null ||
						blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id);
			}

			var blogByTagArchivePath = blogsByTagArchivePathMatch.ToArray().FirstOrDefault(e => EntityHasPath(serviceContext, e, archiveRootPath));

			if (blogByTagArchivePath != null)
			{
				node = GetBlogTagArchiveNode(serviceContext, blogByTagArchivePath, tagSlug);

				return true;
			}

			return false;
		}

		protected virtual bool TryGetWebFileNode(OrganizationServiceContext serviceContext, EntityReference website, string path, out CrmSiteMapNode node)
		{
			node = null;

			var pathMatch = WebFilePathRegex.Match(path);

			if (!pathMatch.Success)
			{
				return false;
			}

			var contentMapProvider = HttpContext.Current.GetContentMapProvider();
			IDictionary<EntityReference, EntityNode> webfiles = new Dictionary<EntityReference, EntityNode>();
			contentMapProvider.Using(map => map.TryGetValue("adx_webfile", out webfiles));
			var files =
				webfiles.Values.Cast<WebFileNode>()
					.Where(wf => wf.BlogPost != null && wf.PartialUrl.Equals(pathMatch.Groups["file"].Value) && wf.StateCode == 0);

			if (!files.Any())
			{
				return false;
			}

			Entity blogPost;

			if (TryGetBlogPostByPartialUrl(serviceContext, website, pathMatch.Groups["post"].Value, out blogPost))
			{
				var file = files.FirstOrDefault(f =>
				{
					var blogPostReference = f.BlogPost;

					return blogPostReference != null && blogPostReference.Equals(blogPost.ToEntityReference());
				});

				if (file != null)
				{
					var entity = serviceContext.MergeClone(file.ToEntity());
					node = GetWebFileNode(serviceContext, entity, HttpStatusCode.OK);

					return true;
				}
			}

			return false;
		}

		private CrmSiteMapNode GetWebFileNode(OrganizationServiceContext serviceContext, Entity file, HttpStatusCode statusCode)
		{
			if (file == null)
			{
				return null;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, file);
			var name = file.GetAttributeValue<string>("adx_name");
			var summary = file.GetAttributeValue<string>("adx_summary");

			var fileAttachmentProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityFileAttachmentProvider>();

			var attachmentInfo = fileAttachmentProvider.GetAttachmentInfo(serviceContext, file).FirstOrDefault();

			// apply a detached clone of the entity since the SiteMapNode is out of the scope of the current OrganizationServiceContext
			var fileClone = file.Clone(false);

			// If there's no file attached to the webfile, return a NotFound node with no rewrite path.
			if (attachmentInfo == null)
			{
				return new CrmSiteMapNode(
					this,
					url,
					url,
					name,
					summary,
					null,
					file.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
					fileClone,
					HttpStatusCode.NotFound);
			}

			return new CrmSiteMapNode(
				this,
				url,
				url,
				name,
				summary,
				attachmentInfo.Url,
				attachmentInfo.LastModified.GetValueOrDefault(DateTime.UtcNow),
				file,
				statusCode);
		}

		private static Entity GetBlog(OrganizationServiceContext serviceContext, EntityReference website, Guid id)
		{
			return serviceContext.CreateQuery("adx_blog")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogid") == id && e.GetAttributeValue<EntityReference>("adx_websiteid") == website);
		}

		private static string GetBlogAuthorName(OrganizationServiceContext serviceContext, Guid authorId)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity("contact")
				{
					Attributes = new List<FetchAttribute> { new FetchAttribute("fullname") },
					Filters = new List<Filter> { new Filter { Conditions = new[] { new Condition("contactid", ConditionOperator.Equal, authorId) } } }
				}
			};

			var contact = serviceContext.RetrieveSingle(fetch);

			return contact == null ? "?" : contact.GetAttributeValue<string>("fullname");
		}

		private static Entity GetBlogPost(OrganizationServiceContext serviceContext, Guid id)
		{
			return serviceContext.CreateQuery("adx_blogpost").FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogpostid") == id);
		}

		private static Entity GetPage(OrganizationServiceContext serviceContext, EntityReference website, Guid id)
		{
			return serviceContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == id && e.GetAttributeValue<EntityReference>("adx_websiteid") == website);
		}

		public static bool TryGetAuthorArchiveNodeAttribute(SiteMapNode node, out Guid authorId)
		{
			authorId = default(Guid);

			return node != null && Guid.TryParse(node[AuthorArchiveNodeAttributeKey], out authorId);
		}

		public static bool TryGetMonthArchiveNodeAttribute(SiteMapNode node, out DateTime month)
		{
			month = default(DateTime);

			return node != null && DateTime.TryParseExact(node[MonthArchiveNodeAttributeKey], "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out month);
		}

		public static bool TryGetTagArchiveNodeAttribute(SiteMapNode node, out string tag)
		{
			if (node == null)
			{
				tag = null;

				return false;
			}

			tag = node[TagArchiveNodeAttributeKey];

			return !string.IsNullOrWhiteSpace(tag);
		}

		private string GetRewriteUrl(Entity pageTemplate, out string webTemplateId)
		{
			webTemplateId = null;

			if (pageTemplate == null)
			{
				return null;
			}

			if (TryGetWebTemplateId(pageTemplate, out webTemplateId))
			{
				return pageTemplate.GetAttributeValue<bool?>("adx_usewebsiteheaderandfooter").GetValueOrDefault(true)
					? "~/Pages/WebTemplate.aspx"
					: "~/Pages/WebTemplateNoMaster.aspx";
			}

			return pageTemplate.GetAttributeValue<string>("adx_rewriteurl");
		}

		private bool TryGetWebTemplateId(Entity pageTemplate, out string webTemplateId)
		{
			webTemplateId = null;

			if (pageTemplate == null)
			{
				return false;
			}

			var type = pageTemplate.GetAttributeValue<OptionSetValue>("adx_type");
			var webTemplate = pageTemplate.GetAttributeValue<EntityReference>("adx_webtemplateid");

			if (type == null || type.Value != (int)PageTemplateNode.TemplateType.WebTemplate || webTemplate == null)
			{
				return false;
			}
				
			webTemplateId = webTemplate.Id.ToString();

			return true;
		}
	}
}
