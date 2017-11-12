/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Checks the HttpRequest and if it is a Web File or Web Page 
	/// and 'Enable Tracking' property is true then create a log of the download request.
	/// </summary>
	public static class AsyncTracking
	{
		/// <summary>
		/// Indicates if a request has been marked as trackable.
		/// </summary>
		private static bool IsTrackable
		{
			get
			{
				return HttpContext.Current != null && HttpContext.Current.Items["AsyncTrackingEnabled"] != null;
			}
		}

		/// <summary>
		/// Marks a request as trackable by the AsyncTracking class.
		/// </summary>
		/// <param name="context"></param>
		internal static void TrackRequest(HttpContextBase context)
		{
			context.Items["AsyncTrackingEnabled"] = new object();
		}

		public static IAsyncResult BeginRequestAsync(object source, EventArgs e, AsyncCallback cb, object state)
		{
			if (!IsTrackable)
			{
				// create a 'noop' operation that does nothing when called
				AsyncCallerLog noopFunc = delegate { };

				return noopFunc.BeginInvoke(null, null, null, cb, state);
			}

			var node = SiteMap.Provider.CurrentNode as CrmSiteMapNode;

			var user = PortalContext.Current.User;

			string ipAddress;

			try
			{
				ipAddress = ((HttpApplication)source).Context.Request.UserHostAddress ?? string.Empty;
			}
			catch
			{
				ipAddress = string.Empty;
			}
			
			// Create a delegate instance of the LogRequest method 
			var logDelegate = new AsyncCallerLog(LogRequest);
			
			// Asynchronously invoke the LogRequest method.
			return logDelegate.BeginInvoke(node, ipAddress, user, cb, state);
		}

		public static void EndRequestAsync(IAsyncResult ar)
		{
			// Extract the delegate from the System.Runtime.Remoting.Messaging.AsyncResult.
			var logDelegate = (AsyncCallerLog)((AsyncResult)ar).AsyncDelegate;

			logDelegate.EndInvoke(ar);
		}

		/// <summary>
		/// Declare an asynchronous delegate that matches the LogRequest method.
		/// </summary>
		public delegate void AsyncCallerLog(CrmSiteMapNode node, string ipAddress, Entity user);

		/// <summary>
		/// Method to process the node of the current request and save tracking info.
		/// </summary>
		private static void LogRequest(CrmSiteMapNode node, string ipAddress, Entity user)
		{
			if (node == null) return;

			if (node.StatusCode != HttpStatusCode.OK) return; //possbily put 404 tracking in here?

			var context = CrmConfigurationManager.CreateContext();

			switch (node.Entity.LogicalName)
			{
				case "adx_webpage":

					var webpage = context.CreateQuery("adx_webpage").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_webpageid") == node.Entity.Id);

					if (webpage != null && (webpage.GetAttributeValue<bool?>("adx_enabletracking") ?? false))
					{
						var webpagelog = new Entity("adx_webpagelog");

						webpagelog.SetAttributeValue("adx_name", webpage.GetAttributeValue<string>("adx_name") + " log");

						webpagelog.SetAttributeValue("adx_date", DateTime.UtcNow);

						webpagelog.SetAttributeValue("adx_ipaddress", ipAddress);

						context.AddObject(webpagelog);

						context.AddLink(webpagelog, "adx_webpage_webpagelog".ToRelationship(), webpage);

						if (user != null)
						{
							var contact = context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == user.Id);

							if (contact != null)
							{
								context.AddLink(webpagelog, "adx_contact_webpagelog".ToRelationship(), contact);
							}
						}

						context.SaveChanges();
					}

					break;

				case "adx_webfile":

					var webfile = context.CreateQuery("adx_webfile").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_webfileid") == node.Entity.Id);

					if (webfile != null && (webfile.GetAttributeValue<bool?>("adx_enabletracking") ?? false))
					{
						var webfilelog = new Entity("adx_webfilelog");

						webfilelog.SetAttributeValue("adx_name", webfile.GetAttributeValue<string>("adx_name") + " log");

						webfilelog.SetAttributeValue("adx_date", DateTime.UtcNow);

						webfilelog.SetAttributeValue("adx_ipaddress", ipAddress);

						context.AddObject(webfilelog);

						context.AddLink(webfilelog, "adx_webfile_webfilelog".ToRelationship(), webfile);

						if (user != null)
						{
							var contact = context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == user.Id);

							if (contact != null)
							{
								context.AddLink(webfilelog, "adx_contact_webfilelog".ToRelationship(), contact);
							}
						}

						context.SaveChanges();
					}

					break;
			}
		}
	}
}
