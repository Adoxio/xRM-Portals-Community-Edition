/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Xml;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Areas.Company
{
	/// <summary>
	/// Generates a feed of news for the site (news pages are considered to be any visible child
	/// pages of the page identified by the "News" site marker).
	/// </summary>
	public class NewsFeedHandler : IHttpHandler
	{
		public class RouteHandler : IRouteHandler
		{
			public IHttpHandler GetHttpHandler(RequestContext requestContext)
			{
				return new NewsFeedHandler();
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentEncoding = Encoding.UTF8;

			var portalContext = PortalContext.Current;
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			serviceContext.MergeOption = MergeOption.NoTracking;
			
			var newsRootPage = serviceContext.GetPageBySiteMarkerName(portalContext.Website, "News");

			if (newsRootPage == null)
			{
				context.Response.StatusCode = 404;
				context.Response.ContentType = "text/plain";
				context.Response.Write(ResourceManager.GetString("Not_Found_Exception"));

				return;
			}

			var feed = new SyndicationFeed(GetSyndicationItems(serviceContext, newsRootPage.ToEntityReference()))
			{
				Title = SyndicationContent.CreatePlaintextContent(newsRootPage.GetAttributeValue<string>("adx_title") ?? newsRootPage.GetAttributeValue<string>("adx_name")),
				Description = SyndicationContent.CreateHtmlContent(newsRootPage.GetAttributeValue<string>("adx_summary") ?? string.Empty),
				BaseUri = new Uri(context.Request.Url.GetLeftPart(UriPartial.Authority))
			};

			context.Response.ContentType = "application/atom+xml";

			using (var writer = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8))
			{
				feed.SaveAsAtom10(writer);
			}
		}

		private static IEnumerable<SyndicationItem> GetSyndicationItems(OrganizationServiceContext serviceContext, EntityReference newsRootPage)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();

			return serviceContext.CreateQuery("adx_webpage")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_parentpageid") == newsRootPage)
				.OrderByDescending(e => e.GetAttributeValue<DateTime?>("adx_displaydate"))
				.Take(20)
				.ToArray()
				.Where(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read))
				.Select(e => GetSyndicationItem(serviceContext, e));
		}

		private static SyndicationItem GetSyndicationItem(OrganizationServiceContext serviceContext, Entity newsItemPage)
		{
			var displayDate = newsItemPage.GetAttributeValue<DateTime?>("adx_displaydate");

			var item = new SyndicationItem(
				newsItemPage.GetAttributeValue<string>("adx_title") ?? newsItemPage.GetAttributeValue<string>("adx_name"),
				SyndicationContent.CreateHtmlContent(newsItemPage.GetAttributeValue<string>("adx_copy") ?? string.Empty),
				new Uri(new UrlBuilder(serviceContext.GetUrl(newsItemPage))),
				"uuid:{0}".FormatWith(newsItemPage.Id),
				displayDate.GetValueOrDefault(newsItemPage.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow)));

			if (displayDate != null)
			{
				item.PublishDate = displayDate.Value;
			}

			return item;
		}
	}
}
