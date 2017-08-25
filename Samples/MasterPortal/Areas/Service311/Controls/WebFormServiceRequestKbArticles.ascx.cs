/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Service311.Controls
{
	public partial class WebFormServiceRequestKbArticles : WebFormUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var context = PortalCrmConfigurationManager.CreateServiceContext();
			var site = portalContext.Website;
			var serviceRequestType = context.CreateQuery("adx_servicerequesttype").FirstOrDefault(s => s.GetAttributeValue<string>("adx_entityname") == CurrentStepEntityLogicalName);

			if (serviceRequestType == null)
			{
				throw new ApplicationException(string.Format("Service Request Type couldn't be found for the entity type {0}.", CurrentStepEntityLogicalName));
			}

			var website = context.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == site.Id);

			var maxLatestArticlesSetting = context.GetSiteSettingValueByName(website, "service_request_max_kb_articles");

			int maxLatestArticles;

			maxLatestArticles = int.TryParse(maxLatestArticlesSetting, out maxLatestArticles) ? maxLatestArticles : 10;

			var latestArticles = new List<Entity>();

			var subject = serviceRequestType.GetAttributeValue<EntityReference>("adx_subject");

			if (subject != null)
			{
				latestArticles = context.CreateQuery("kbarticle").Where(k => k.GetAttributeValue<OptionSetValue>("statecode") != null && k.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Enums.KbArticleState.Published && k.GetAttributeValue<bool>("msa_publishtoweb") && k.GetAttributeValue<EntityReference>("subjectid") == subject).OrderByDescending(k => k.GetAttributeValue<DateTime>("createdon")).Take(maxLatestArticles).ToList();
			}

			//if (!latestArticles.Any())
			//{
			//	MoveToNextStep();
			//}

			LatestArticlesList.DataSource = latestArticles;

			LatestArticlesList.DataBind();
		}

		protected string GetKbArticleUrl(Entity kbarticle)
		{
			if (kbarticle == null)
			{
				return null;
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var articleUrl = context.GetUrl(kbarticle);

			return articleUrl;
		}

		public static string HtmlEncode(object value)
		{
			return value == null ? string.Empty : System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(value.ToString(), true);
		}

		protected virtual UrlBuilder GetUrlForRequiredSiteMarker(string siteMarkerName)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var site = portalContext.Website;
			var context = PortalCrmConfigurationManager.CreateServiceContext();
			var website = context.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == site.Id);
			var page = context.GetPageBySiteMarkerName(website, siteMarkerName);

			if (page == null)
			{
				throw new Exception("Please contact your system administrator. The required site marker {0} is missing.".FormatWith(siteMarkerName));
			}

			var path = context.GetUrl(page);

			if (path == null)
			{
                throw new Exception("Please contact your System Administrator. Unable to build URL for Site Marker {0}.".FormatWith(siteMarkerName));
			}

			return new UrlBuilder(path);
		}
	}
}
