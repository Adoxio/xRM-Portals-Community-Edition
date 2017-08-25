/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Blogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Cms.Security;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;

	internal class BlogSecurityProvider : ContentMapAccessProvider
	{
		protected string PortalName { get; private set; }

		protected CacheSupportingCrmEntitySecurityProvider WebPageSecurityProvider { get; private set; }

		public BlogSecurityProvider(CacheSupportingCrmEntitySecurityProvider webPageSecurityProvider, HttpContext context, string portalName = null)
			: this(context)
		{
			if (webPageSecurityProvider == null)
			{
				throw new ArgumentNullException("webPageSecurityProvider");
			}

			this.WebPageSecurityProvider = webPageSecurityProvider;
			this.PortalName = portalName;
		}

		/// <summary> Initializes a new instance of the <see cref="BlogSecurityProvider"/> class. </summary>
		/// <param name="context"> The context. </param>
		public BlogSecurityProvider(HttpContext context)
			: this(context != null ? context.GetContentMapProvider() : AdxstudioCrmConfigurationManager.CreateContentMapProvider())
		{
		}

		/// <summary> Initializes a new instance of the <see cref="BlogSecurityProvider"/> class. </summary>
		/// <param name="contentMapProvider"> The content map provider. </param>
		public BlogSecurityProvider(IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
		}

		protected override bool TryAssert(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName == "adx_blog")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_blog ({1}).", right, entity.Id));

				return this.TryAssertBlog(serviceContext, entity, right, dependencies, map);
			}

			if (entity.LogicalName == "adx_blogpost")
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_blogpost ({1}).", right, entity.Id));
				dependencies.AddEntityDependency(entity);

				return this.TryAssertBlogPost(serviceContext, entity, right, dependencies);
			}

			if (entity.LogicalName == "adx_blogpostcomment")
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_blogpostcomment ({1}).", right, entity.Id));
				dependencies.AddEntityDependency(entity);

				return this.TryAssertBlogPostComment(serviceContext, entity, right, dependencies);
			}

			throw new NotSupportedException("Entities of type {0} are not supported by this provider.".FormatWith(entity.LogicalName));
		}

		private bool TryAssertBlog(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			var pageReference = entity.GetAttributeValue<EntityReference>("adx_parentpageid");
			if (pageReference == null)
			{
				return false;
			}

			var parentPage = serviceContext.RetrieveSingle(
				"adx_webpage",
				new[] { "adx_name" },
				new Condition("adx_webpageid", ConditionOperator.Equal, pageReference.Id));

			if (right == CrmEntityRight.Read)
			{
				return parentPage != null && this.WebPageSecurityProvider.TryAssert(serviceContext, parentPage, right, dependencies);
			}

			if (!Roles.Enabled)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Roles are not enabled for this application. Denying Change.");

				return false;
			}

			IEnumerable<Entity> authorRoles = new List<Entity>();
			EntityNode blogNode;
			if (!map.TryGetValue(entity, out blogNode))
			{
				return false;
			}

			if (blogNode is BlogNode)
			{
				authorRoles = ((BlogNode)blogNode).WebRoles.Select(wr => wr.ToEntity());
			}

			if (!authorRoles.Any())
			{
				return false;
			}

			dependencies.AddEntityDependencies(authorRoles);

			var userRoles = this.GetUserRoles();

			return authorRoles.Select(e => e.GetAttributeValue<string>("adx_name")).Intersect(userRoles, StringComparer.InvariantCulture).Any()
				|| (parentPage != null && this.WebPageSecurityProvider.TryAssert(serviceContext, parentPage, right, dependencies));
		}

		private bool TryAssertBlogPost(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			var blogReference = entity.GetAttributeValue<EntityReference>("adx_blogid");

			if (blogReference == null)
			{
				return false;
			}

			var blog = serviceContext.RetrieveSingle(
				"adx_blog",
				new[] { "adx_parentpageid", "adx_websiteid" },
				new Condition("adx_blogid", ConditionOperator.Equal, blogReference.Id));

			if (blog == null)
			{
				return false;
			}

			var published = entity.GetAttributeValue<bool?>("adx_published").GetValueOrDefault(false);

			return this.TryAssert(
				serviceContext,
				blog,
				(right == CrmEntityRight.Read && published ? CrmEntityRight.Read : CrmEntityRight.Change),
				dependencies);
		}

		private bool TryAssertBlogPostComment(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			var blogPostReference = entity.GetAttributeValue<EntityReference>("adx_blogpostid");

			if (blogPostReference == null)
			{
				return false;
			}

			var blogPost = serviceContext.RetrieveSingle(
				"adx_blogpost",
				new[] { "adx_blogid" },
				new Condition("adx_blogpostid", ConditionOperator.Equal, blogPostReference.Id));

			if (blogPost == null)
			{
				return false;
			}

			var approved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

			return this.TryAssert(
				serviceContext,
				blogPost,
				(right == CrmEntityRight.Read && approved ? CrmEntityRight.Read : CrmEntityRight.Change),
				dependencies);
		}
	}
}
