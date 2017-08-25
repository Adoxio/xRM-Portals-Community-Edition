/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// A <see cref="SiteMapProvider"/> for navigating 'adx_survey' <see cref="Entity"/> hierarchies.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <siteMap enabled="true" defaultProvider="Surveys">
	///    <providers>
	///     <add
	///      name="Surveys"
	///      type="Adxstudio.Xrm.Web.SurveySiteMapProvider"
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
	public class SurveySiteMapProvider : CrmSiteMapProviderBase
	{
		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			TraceInfo("FindSiteMapNode({0})", rawUrl);

			var path = new UrlBuilder(rawUrl).Path;

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var surveysInCurrentWebsite = website.GetRelatedEntities(context, "adx_website_survey");

			var foundSurvey = surveysInCurrentWebsite.FirstOrDefault(s => string.Equals(path, context.GetUrl(s)));

			return foundSurvey != null ? GetNode(context, foundSurvey) : null;
		}

		protected virtual IEnumerable<Entity> FindSurveys(OrganizationServiceContext context, Entity website, Entity webpage)
		{
			var forumsInCurrentWebsite = website.GetRelatedEntities(context, "adx_website_survey");
			var forums = forumsInCurrentWebsite.Where(e => e.GetAttributeValue<EntityReference>("adx_parentpageid") == webpage.ToEntityReference());
			return forums;
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

			var entity = crmNode.Entity;

			var childSurveys = FindSurveys(context, website, entity);

			if (childSurveys == null)
			{
				return children;
			}

			foreach (var childSurvey in childSurveys)
			{
				var childNode = GetNode(context, childSurvey);

				if (ChildNodeValidator.Validate(context, childNode))
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

			if (crmNode == null || !crmNode.HasCrmEntityName("adx_survey"))
			{
				return null;
			}

			var portal = PortalContext;
			var context = portal.ServiceContext;

			var parentPage = crmNode.Entity.GetRelatedEntity(context, "adx_webpage_survey");

			return SiteMap.Provider.FindSiteMapNode(context.GetUrl(parentPage));
		}

		protected override CrmSiteMapNode GetNode(OrganizationServiceContext context, Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_survey")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
			}

			var url = context.GetUrl(entity);

			var portal = PortalContext;
			var serviceContext = portal.ServiceContext;
			var website = portal.Website;

			var surveyID = entity.GetAttributeValue<Guid>("adx_surveyid");

			var webSurvey =
				website.GetRelatedEntities(serviceContext, "adx_website_survey")
					.SingleOrDefault(e => e.GetAttributeValue<Guid>("adx_surveyid") == surveyID);

			var pageTemplate = entity.GetRelatedEntity(context, "adx_pagetemplate_survey");
			string partialURL;
			if (pageTemplate != null)
			{
				partialURL = pageTemplate.GetAttributeValue<string>("adx_rewriteurl") ?? "~/Pages/Survey.aspx";
			}
			else
			{
				partialURL = "~/Pages/Survey.aspx";
			}

			return new CrmSiteMapNode(
				this,
				url,
				url,
				entity.GetAttributeValue<string>("adx_name"),
				entity.GetAttributeValue<string>("adx_description"),
				partialURL + "?id=" + surveyID,
				entity.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				webSurvey);
		}

		protected override SiteMapNode GetRootNodeCore()
		{
			TraceInfo("GetRootNodeCore()");

			return null;
		}
	}
}
