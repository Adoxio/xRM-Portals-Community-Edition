/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk.Client;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// A <see cref="SiteMapProvider"/> for navigating 'adx_communityforum' <see cref="Entity"/> hierarchies.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <siteMap enabled="true" defaultProvider="Forums">
	///    <providers>
	///     <add
	///      name="Forums"
	///      type="Adxstudio.Xrm.Web.ForumSiteMapProvider"
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
	public class ForumSiteMapProvider : CrmSiteMapProviderBase, ISolutionDependent
	{
		public IEnumerable<string> RequiredSolutions
		{
			get { return new[] { "MicrosoftForums" }; }
		}

		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			TraceInfo("FindSiteMapNode({0})", rawUrl);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var applicationPath = ApplicationPath.Parse(rawUrl);
			var path = new UrlBuilder(applicationPath.PartialPath).Path;

			// match on a regular webpage path followed by a thread Id segment

			const string pattern = @"(.+)/(\{?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\}?)";
			var match = Regex.Match(path, pattern, RegexOptions.IgnoreCase);
			var isThreadUrl = match.Success && match.Groups.Count == 3;

			var forumPath = isThreadUrl
				? match.Groups[1].Value
				: path;

			var forum = FindForum(context, website, forumPath);

			if (forum == null)
			{
				return  null;
			}

			Entity foundThread = null;

			if (isThreadUrl)
			{
				var threadId = new Guid(match.Groups[2].Value);

				var forumThreads = context.CreateQuery("adx_communityforumthread")
					.Where(e => e.GetAttributeValue<EntityReference>("adx_forumid") == forum.ToEntityReference())
					.Where(e => e.GetAttributeValue<Guid>("adx_communityforumthreadid") == threadId);

				foundThread = forumThreads.FirstOrDefault();
			}

			if (foundThread == null)
			{
				return GetAccessibleNodeOrAccessDeniedNode(context, forum);
			}

			return GetAccessibleNodeOrAccessDeniedNode(context, foundThread);
		}

		protected virtual Entity FindForum(OrganizationServiceContext context, Entity website, string path)
		{
			var filter = new Filter
			{
				Conditions = new[]
				{
					new Condition("adx_websiteid", ConditionOperator.Equal, website.Id)
				}
			};

			// Only consider forum if it is in current language or language-agnostic
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
				Entity = new FetchEntity("adx_communityforum")
				{
					Filters = new[] { filter, mlpFilter }
				}
			};

			var forumsInCurrentWebsite = context.RetrieveMultiple(fetch);
			var forumInCurrentWebsite = forumsInCurrentWebsite.Entities.FirstOrDefault(e => this.EntityHasPath(context, e, path));

			return forumInCurrentWebsite;
		}

		protected virtual IEnumerable<Entity> FindForums(OrganizationServiceContext context, Entity website, Entity webpage)
		{
			var forumsInCurrentWebsite = context.CreateQuery("adx_communityforum")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference());

			EntityReference rootPage = webpage.ToEntityReference();

			ContextLanguageInfo languageInfo = HttpContext.Current.GetContextLanguageInfo();

			IEnumerable<Entity> forums = null;

			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				if (!webpage.GetAttributeAliasedValue<bool>("adx_isroot") && webpage.GetAttributeAliasedValue<EntityReference>("adx_rootwebpageid") != null)
				{
					rootPage = webpage.GetAttributeAliasedValue<EntityReference>("adx_rootwebpageid");
				}

				forums = forumsInCurrentWebsite
					.Where(e => e.GetAttributeValue<EntityReference>("adx_parentpageid") == rootPage
					&& (e.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null
					|| e.GetAttributeValue<EntityReference>("adx_websitelanguageid") == languageInfo.ContextLanguage.EntityReference));
			}
			else
			{
				forums = forumsInCurrentWebsite
					.Where(e => e.GetAttributeValue<EntityReference>("adx_parentpageid") == webpage.ToEntityReference());
			} 

			return forums.ToList();
		}

		public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
		{
			TraceInfo("GetChildNodes({0})", node.Key);

			var children = new SiteMapNodeCollection();

			var crmNode = node as CrmSiteMapNode;

			if (crmNode == null || !crmNode.HasCrmEntityName("adx_webpage"))
			{
				return children;
			}

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var entity = context.MergeClone(crmNode.Entity);

			var childForums = FindForums(context, website, entity);

			if (childForums == null)
			{
				return children;
			}

			foreach (var childForum in childForums)
			{
				var childNode = GetForumNode(context, childForum);

				if (ChildNodeValidator.Validate(context, childNode))
				{
					children.Add(childNode);
				}
			}

			return children;
		}

		public override SiteMapNode GetParentNode(SiteMapNode node)
		{
			var crmNode = node as CrmSiteMapNode;

			if (crmNode == null)
			{
				return null;
			}

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var entity = context.MergeClone(crmNode.Entity);

			if (crmNode.HasCrmEntityName("adx_communityforum"))
			{
				var parentPage = entity.GetRelatedEntity(context, "adx_webpage_communityforum");

				// If this association doesn't exist, this is an old schema, return the "Forums" page as the parent.
				if (parentPage == null)
				{
					var forumsRootPage = OrganizationServiceContextExtensions.GetPageBySiteMarkerName(context, website, "Forums");

					return SiteMap.Provider.FindSiteMapNode(context.GetUrl(forumsRootPage));
				}

				return SiteMap.Provider.FindSiteMapNode(context.GetUrl(parentPage));
			}

			if (crmNode.HasCrmEntityName("adx_communityforumthread"))
			{
				var parentForum = entity.GetRelatedEntity(context, "adx_communityforum_communityforumthread");

				if (parentForum == null)
				{
					return null;
				}

				var parentForumNode = GetForumNode(context, parentForum);

				return NodeValidator.Validate(context, parentForumNode) ? parentForumNode : null;
			}

			return null;
		}

		protected CrmSiteMapNode GetForumNode(OrganizationServiceContext context, Entity forum)
		{
			forum.AssertEntityName("adx_communityforum");
			
			var website = HttpContext.Current.GetWebsite().Entity;

			var forumId = forum.GetAttributeValue<Guid>("adx_communityforumid");

			var forumsInCurrentWebsite = context.RetrieveMultiple(
				"adx_communityforum",
				new string[] { },
				new[] { new Condition("adx_websiteid", ConditionOperator.Equal, website.Id) });

			var webForum = forumsInCurrentWebsite.Entities
				.SingleOrDefault(f => f.GetAttributeValue<Guid>("adx_communityforumid") == forumId);

			// apply a detached clone of the entity since the SiteMapNode is out of the scope of the current OrganizationServiceContext
			var webForumClone = webForum.Clone(false);

			string webTemplateId;

			var url = context.GetUrl(forum);

			var node = new CrmSiteMapNode(
				this,
				url,
				url,
				forum.GetAttributeValue<string>("adx_name"),
				forum.GetAttributeValue<string>("adx_description"),
				GetForumPageTemplatePath(context, forum, out webTemplateId) + "?id=" + forumId,
				forum.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				webForumClone);
			
			if (webTemplateId != null)
			{
				node["adx_webtemplateid"] = webTemplateId;
			}

			return node;
		}

		protected CrmSiteMapNode GetForumThreadNode(OrganizationServiceContext context, Entity thread)
		{
			thread.AssertEntityName("adx_communityforumthread");

			var forum = thread.GetRelatedEntity(context, "adx_communityforum_communityforumthread");
			var forumThreadId = thread.GetAttributeValue<Guid>("adx_communityforumthreadid");

			var url = OrganizationServiceContextExtensions.GetUrl(context, forum) + "/" + forumThreadId;
			var threadClone = thread.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				url,
				url,
				thread.GetAttributeValue<string>("adx_name"),
				thread.GetAttributeValue<string>("adx_name"),
				GetForumThreadPageTemplatePath(context, forum, out webTemplateId) + "?id=" + forumThreadId,
				thread.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				threadClone);

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

			if (entityName == "adx_communityforum")
			{
				return GetForumNode(context, entity);
			}

			if (entityName == "adx_communityforumthread")
			{
				return GetForumThreadNode(context, entity);
			}

			throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
		}

		protected string GetForumPageTemplatePath(OrganizationServiceContext context, Entity forum, out string webTemplateId)
		{
			webTemplateId = null;

			if (forum == null)
			{
				throw new ArgumentNullException("forum");
			}

			if (forum.LogicalName != "adx_communityforum")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(forum.Id, forum.GetType().FullName), "forum");
			}

			var pageTemplate = context.RetrieveSingle(
				"adx_pagetemplate",
				FetchAttribute.All,
				new Condition("adx_pagetemplateid", ConditionOperator.Equal, forum.GetAttributeValue<EntityReference>("adx_forumpagetemplateid").Id));

			if (pageTemplate == null)
			{
				return HttpContext.Current.GetWebsite().Settings.Get<string>("forums/forum/templatepath") ?? "~/Pages/Forum.aspx";
			}

			if (TryGetWebTemplateId(pageTemplate, out webTemplateId))
			{
				return pageTemplate.GetAttributeValue<bool?>("adx_usewebsiteheaderandfooter").GetValueOrDefault(true)
					? "~/Pages/WebTemplate.aspx"
					: "~/Pages/WebTemplateNoMaster.aspx";
			}

			return pageTemplate.GetAttributeValue<string>("adx_rewriteurl") ?? "~/Pages/Forum.aspx";
		}

		protected string GetForumThreadPageTemplatePath(OrganizationServiceContext context, Entity forum, out string webTemplateId)
		{
			webTemplateId = null;

			if (forum == null)
			{
				throw new ArgumentNullException("forum");
			}

			if (forum.LogicalName != "adx_communityforum")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(forum.Id, forum.GetType().FullName), "forum");
			}

			var pageTemplate = context.RetrieveSingle(
				"adx_pagetemplate",
				FetchAttribute.All,
				new Condition("adx_pagetemplateid", ConditionOperator.Equal, forum.GetAttributeValue<EntityReference>("adx_threadpagetemplateid").Id));

			if (pageTemplate == null)
			{
				return HttpContext.Current.GetWebsite().Settings.Get<string>("forums/forumthread/templatepath") ?? "~/Pages/ForumThread.aspx";
			}

			if (TryGetWebTemplateId(pageTemplate, out webTemplateId))
			{
				return pageTemplate.GetAttributeValue<bool?>("adx_usewebsiteheaderandfooter").GetValueOrDefault(true)
					? "~/Pages/WebTemplate.aspx"
					: "~/Pages/WebTemplateNoMaster.aspx";
			}

			return pageTemplate.GetAttributeValue<string>("adx_rewriteurl") ?? "~/Pages/ForumThread.aspx";
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
