/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Text;
using System.Web;
using System.Xml;
using Adxstudio.Xrm.Forums;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Cms;

namespace Adxstudio.Xrm.Web
{
	public class ForumSiteMapHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.ContentType = "text/xml";

			using (var xml = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8))
			{
				xml.WriteStartDocument();

				xml.WriteStartElement("urlset");
				xml.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

				foreach (var forum in PortalContext.Current.ServiceContext.GetForums())
				{
					ProcessNode(xml, "http://" + context.Request.ServerVariables["HTTP_HOST"], forum);
				}

				xml.WriteEndDocument();
			}
		}

		public void ProcessNode(XmlTextWriter xml, string urlPrefix, Entity forum)
		{
			forum.AssertEntityName("adx_communityforum");

			var context = PortalContext.Current.ServiceContext;

			var threads = forum.GetRelatedEntities(context, "adx_communityforum_communityforumthread");

			foreach (var thread in threads)
			{
				xml.WriteStartElement("url");
				xml.WriteElementString("loc", urlPrefix + context.GetUrl(thread));
				xml.WriteElementString("lastmod", thread.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow).ToString("yyyy-MM-dd"));
				xml.WriteEndElement();
			}
		}
		
	}
}
