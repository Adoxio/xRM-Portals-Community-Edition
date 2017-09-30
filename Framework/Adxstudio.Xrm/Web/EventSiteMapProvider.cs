/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk.Client;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// A <see cref="SiteMapProvider"/> for navigating 'adx_event' <see cref="Entity"/> hierarchies.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <siteMap enabled="true" defaultProvider="Events">
	///    <providers>
	///     <add
	///      name="Events"
	///      type="Adxstudio.Xrm.Web.EventSiteMapProvider"
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
	[Obsolete]
	public class EventSiteMapProvider : CrmSiteMapProviderBase, ISolutionDependent
	{
		public IEnumerable<string> RequiredSolutions
		{
			get { return new[] { "AdxstudioEventManagement" }; }
		}

		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			TraceInfo("FindSiteMapNode({0})", rawUrl);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var applicationPath = ApplicationPath.Parse(rawUrl);
			var path = new UrlBuilder(applicationPath.PartialPath).Path;

			var foundEvent = FindEvent(context, website, path);

			return foundEvent != null ? GetAccessibleNodeOrAccessDeniedNode(context, foundEvent) : null;
		}

		protected virtual Entity FindEvent(OrganizationServiceContext context, Entity website, string path)
		{
			var eventsInCurrentWebsite = context.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference())
				.ToArray();

			var foundEvent = eventsInCurrentWebsite.FirstOrDefault(e => EntityHasPath(context, e, path));

			return foundEvent;
		}

		protected virtual IEnumerable<Entity> FindEvents(OrganizationServiceContext context, Entity website, Entity webpage)
		{
			var eventsInCurrentWebsite = context.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference())
				.ToArray();

			var events = eventsInCurrentWebsite.Where(e => Equals(e.GetAttributeValue<EntityReference>("adx_parentpageid"), webpage.ToEntityReference()));

			return events;
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

			var childEvents = FindEvents(context, website, entity);

			if (childEvents == null)
			{
				return children;
			}

			foreach (var childEvent in childEvents)
			{
				var childNode = GetNode(context, childEvent);

				if (ChildNodeValidator.Validate(portal.ServiceContext, childNode))
				{
					children.Add(childNode);
				}
			}

			return children;
		}

		public override SiteMapNode GetParentNode(SiteMapNode node)
		{
			TraceInfo("GetParentNode({0})", node.Key);

			var crmNode = node as CrmSiteMapNode;

			if (crmNode == null || !crmNode.HasCrmEntityName("adx_event"))
			{
				return null;
			}

			var portal = PortalContext;
			var context = portal.ServiceContext;

			var entity = context.MergeClone(crmNode.Entity);

			var parentPage = entity.GetRelatedEntity(context, "adx_webpage_event");

			return SiteMap.Provider.FindSiteMapNode(OrganizationServiceContextExtensions.GetUrl(context, parentPage));
		}

		protected override CrmSiteMapNode GetNode(OrganizationServiceContext context, Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_event")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
			}

			var url = OrganizationServiceContextExtensions.GetUrl(context, entity);

			var portal = PortalContext;
			var serviceContext = portal.ServiceContext;
			var website = portal.Website;

			var eventId = entity.GetAttributeValue<Guid>("adx_eventid");

			var eventsInCurrentWebsite = serviceContext.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference())
				.ToArray();

			var webEvent = eventsInCurrentWebsite
				.SingleOrDefault(e => e.GetAttributeValue<Guid>("adx_eventid") == eventId);

			// apply a detached clone of the entity since the SiteMapNode is out of the scope of the current OrganizationServiceContext
			var webEventClone = webEvent.Clone(false);

			string webTemplateId;

			var node = new CrmSiteMapNode(
				this,
				url,
				url,
				entity.GetAttributeValue<string>("adx_name"),
				entity.GetAttributeValue<string>("adx_description"),
				GetPageTemplatePath(serviceContext, entity, website, out webTemplateId) + "?id=" + eventId,
				entity.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				webEventClone);

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

		protected string GetPageTemplatePath(OrganizationServiceContext context, Entity entity, Entity website, out string webTemplateId)
		{
			webTemplateId = null;

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_event")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
			}

			var pageTemplate = entity.GetRelatedEntity(context, "adx_pagetemplate_event");

			if (pageTemplate == null)
			{
				return OrganizationServiceContextExtensions.GetSiteSettingValueByName(context, website, "events/event/templatepath") ?? "~/Pages/Event.aspx";
			}

			if (TryGetWebTemplateId(pageTemplate, out webTemplateId))
			{
				return pageTemplate.GetAttributeValue<bool?>("adx_usewebsiteheaderandfooter").GetValueOrDefault(true)
					? "~/Pages/WebTemplate.aspx"
					: "~/Pages/WebTemplateNoMaster.aspx";
			}

			return pageTemplate.GetAttributeValue<string>("adx_rewriteurl") ?? "~/Pages/Event.aspx";
		}

		protected virtual bool EntityHasPath(OrganizationServiceContext context, Entity entity, string path)
		{
			var entityPath = OrganizationServiceContextExtensions.GetApplicationPath(context, entity);
			if (entityPath == null) return false;
			return string.Equals(path, entityPath.PartialPath);
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
