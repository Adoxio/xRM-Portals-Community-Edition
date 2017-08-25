/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Text;
using System.Web;
using System.Xml;
using System.Globalization;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web
{
	public class SiteMapHandler : IHttpHandler
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

				ProcessNode(xml, "http://" + context.Request.ServerVariables["HTTP_HOST"], SiteMap.Provider.RootNode);

				xml.WriteEndDocument();
			}
		}

		public void ProcessNode(XmlTextWriter xml, string urlPrefix, SiteMapNode node)
		{
			xml.WriteStartElement("url");
			xml.WriteElementString("loc", urlPrefix + node.Url);

			if (node is CrmSiteMapNode)
			{
				xml.WriteElementString("lastmod", ((CrmSiteMapNode)node).LastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			}

			xml.WriteEndElement();

			foreach (SiteMapNode child in node.ChildNodes)
			{
				ProcessNode(xml, urlPrefix, child);
			}
		}
	}
}


