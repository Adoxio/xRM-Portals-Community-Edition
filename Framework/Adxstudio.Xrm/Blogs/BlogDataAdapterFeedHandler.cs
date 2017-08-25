/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Xml;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using System.Web.SessionState;

namespace Adxstudio.Xrm.Blogs
{
	public abstract class BlogDataAdapterFeedHandler : IHttpHandler, IRequiresSessionState
	{
		protected BlogDataAdapterFeedHandler(string portalName, string routeName)
		{
			PortalName = portalName;
			RouteName = routeName;
		}

		public string PortalName { get; private set; }

		public string RouteName { get; private set; }

		public void ProcessRequest(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName, context.Request.RequestContext);

			var feedFactory = new BlogSyndicationFeedFactory(GetDataAdapter(portal, context));

			SyndicationFeed feed;

			try
			{
				feed = feedFactory.CreateFeed(portal, context, RouteName, 40);
			}
			catch (InvalidOperationException)
			{
				throw new HttpException(404, "Blog not found.");
			}

			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.ContentType = "application/atom+xml";

			using (var writer = XmlWriter.Create(context.Response.Output))
			{
				feed.SaveAsAtom10(writer);
			}
		}

		protected abstract IBlogDataAdapter GetDataAdapter(IPortalContext portal, HttpContext context);

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
