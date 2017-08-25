/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.AspNet.Identity;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core.Telemetry;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Globalization;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web
{
	public static class Extensions
	{
		/// <summary>
		/// Responds with a 403 status code.
		/// </summary>
		/// <param name="response"></param>
		public static void ForbiddenAndEndResponse(this HttpResponse response)
		{
			response.Clear();
			response.StatusCode = (int)HttpStatusCode.Forbidden;
			response.End();
		}

		/// <summary>
		/// Calls the Redirect and CompleteRequest operations.
		/// </summary>
		/// <param name="application"></param>
		/// <param name="url"></param>
		public static void RedirectAndEndResponse(this HttpApplication application, string url)
		{
			RedirectAndEndResponse(application, application.Response, url);
		}

		/// <summary>
		/// Calls the Redirect and CompleteRequest operations.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		public static void RedirectAndEndResponse(this HttpContext context, string url)
		{
			RedirectAndEndResponse(context.ApplicationInstance, context.Response, url);
		}

		/// <summary>
		/// Calls the Redirect and CompleteRequest operations.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		public static void RedirectAndEndResponse(this HttpContextBase context, string url)
		{
			RedirectAndEndResponse(context.ApplicationInstance, url);
		}

		/// <summary>
		/// Adds the 'Access-Control-Allow-Origin' header to the response or updates an existing header on the response.
		/// </summary>
		/// <param name="context">Current request context</param>
		/// <param name="allowOrigin">The origin value to be assigned to the 'Access-Control-Allow-Origin' header.</param>
		public static void SetAccessControlAllowOriginHeader(HttpContextBase context, string allowOrigin)
		{
			if (context == null || string.IsNullOrWhiteSpace(allowOrigin))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(context.Response.Headers.Get("Access-Control-Allow-Origin")))
			{
				context.Response.AppendHeader("Access-Control-Allow-Origin", allowOrigin);
			} else
			{
				context.Response.Headers["Access-Control-Allow-Origin"] = allowOrigin;
			}
		}
		
		/// <summary>
		/// Calls the Redirect and CompleteRequest operations. Validates the URL prior to redirect to ensure it is local to prevent an Open Redirection Attack.
		/// </summary>
		/// <param name="application"></param>
		/// <param name="response"></param>
		/// <param name="url"></param>
		private static void RedirectAndEndResponse(HttpApplication application, HttpResponse response, string url)
		{
			// Prevent an Open Redirection Attack. Any web application that redirects to a URL that is specified via the request such as the querystring or form data can potentially be tampered with to redirect users to an external, malicious URL.
			if (System.Web.WebPages.RequestExtensions.IsUrlLocalToHost(new HttpRequestWrapper(application.Request), url))
			{
				response.Redirect(url, false);
			}
			else
			{
				response.Redirect("~/");
			}

			application.CompleteRequest();
		}

		/// <summary>
		/// Attempts to restart the application, first by touching web.config, or if that fails, calling UnloadAppDomain.
		/// </summary>
		public static void RestartWebApplication()
		{
			if (!TryTouchWebConfig())
			{
				TelemetryState.ApplicationEndInfo |= ApplicationEndFlags.UnloadAppDomain;
				WebEventSource.Log.WriteApplicationLifecycleEvent(ApplicationLifecycleEventCategory.Restart, "Touching WebConfig Failed, Unloading AppDomain");
				HttpRuntime.UnloadAppDomain();
			}
		}

		/// <summary>
		/// Resets the last updated date for web.config, effectively forcing an app restart.
		/// </summary>
		private static bool TryTouchWebConfig()
		{
			try
			{
				TelemetryState.ApplicationEndInfo |= ApplicationEndFlags.TouchWebConfig;
				WebEventSource.Log.WriteApplicationLifecycleEvent(ApplicationLifecycleEventCategory.Restart, "Touching WebConfig");
				var configPath = GetWebConfigPath();
				File.SetLastWriteTimeUtc(configPath, DateTime.UtcNow);
				return true;
			}
			catch
			{
				TelemetryState.ApplicationEndInfo &= ~ApplicationEndFlags.TouchWebConfig;
				return false;
			}
		}

		internal static string LocalizeRecordTypeName(string logicalName)
		{
			if (string.IsNullOrEmpty(logicalName))
			{
				return string.Empty;
			}

			EntityMetadata entityMetadata = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			try
			{
				entityMetadata = portalContext.ServiceContext.GetEntityMetadata(logicalName);
			}
			catch (FaultException<OrganizationServiceFault> fe)
			{
				// an exception occurs when trying to retrieve a non-existing entity
				if (!fe.Message.EndsWith("Could not find entity"))
				{
					throw new ApplicationException(string.Format("Error getting EntityMetadata for {0}", logicalName), fe);
				}
			}

			return entityMetadata == null ? logicalName : entityMetadata.DisplayCollectionName.GetLocalizedLabel().Label;
		}

		private static string GetWebConfigPath()
		{
			return HostingEnvironment.MapPath("~/web.config");
		}

		#region IOwinContext

		public static IOwinContext Set<T>(this RequestContext request, T value)
		{
			return Set(request.HttpContext, value);
		}

		public static IOwinContext Set<T>(this HttpContextBase context, T value)
		{
			return Set(context.GetOwinContext(), value);
		}

		public static IOwinContext Set<T>(this IOwinContext context, T value)
		{
			return context.Set(typeof(T).FullName, value);
		}

		public static IOrganizationService GetOrganizationService(this HttpContext context)
		{
			return GetOrganizationService(context.GetOwinContext());
		}

		public static IOrganizationService GetOrganizationService(this HttpContextBase context)
		{
			return GetOrganizationService(context.GetOwinContext());
		}

		public static IOrganizationService GetOrganizationService(this IOwinContext context)
		{
			var dbContext = context.Get<CrmDbContext>();
			return dbContext.Service;
		}

		public static CrmWebsite GetWebsite(this HttpContext context)
		{
			return GetWebsite(context.GetOwinContext());
		}

		public static CrmWebsite GetWebsite(this HttpContextBase context)
		{
			return GetWebsite(context.GetOwinContext());
		}

		public static CrmWebsite GetWebsite(this IOwinContext context)
		{
			return context.Get<CrmWebsite>();
		}

		public static CrmUser GetUser(this HttpContext context)
		{
			return GetUser(context.GetOwinContext());
		}

		public static CrmUser GetUser(this HttpContextBase context)
		{
			return GetUser(context.GetOwinContext());
		}

		public static CrmUser GetUser(this IOwinContext context)
		{
			return context.Get<CrmUser>();
		}

		public static CrmSiteMapNode GetNode(this HttpContext context)
		{
			return GetNode(context.GetOwinContext());
		}

		public static CrmSiteMapNode GetNode(this HttpContextBase context)
		{
			return GetNode(context.GetOwinContext());
		}

		public static CrmSiteMapNode GetNode(this IOwinContext context)
		{
			return context.Get<CrmSiteMapNode>(typeof(CrmSiteMapNode).FullName);
		}

		public static Entity GetEntity(this HttpContext context)
		{
			var node = GetNode(context);
			return node != null ? node.Entity : null;
		}

		public static Entity GetEntity(this HttpContextBase context)
		{
			var node = GetNode(context);
			return node != null ? node.Entity : null;
		}

		public static Entity GetEntity(this IOwinContext context)
		{
			var node = GetNode(context);
			return node != null ? node.Entity : null;
		}

		public static IContentMapProvider GetContentMapProvider(this HttpContext context)
		{
			return GetContentMapProvider(context.GetOwinContext());
		}

		public static IContentMapProvider GetContentMapProvider(this HttpContextBase context)
		{
			return GetContentMapProvider(context.GetOwinContext());
		}

		public static IContentMapProvider GetContentMapProvider(this IOwinContext context)
		{
			return context.Get<ContentMapProvider>();
		}

		public static T GetSiteSetting<T>(this HttpContext context, string name)
		{
			return GetSiteSetting<T>(context.GetOwinContext(), name);
		}

		public static T GetSiteSetting<T>(this HttpContextBase context, string name)
		{
			return GetSiteSetting<T>(context.GetOwinContext(), name);
		}

		public static T GetSiteSetting<T>(this IOwinContext context, string name)
		{
			var website = GetWebsite(context);
			return website.Settings.Get<T>(name);
		}

		public static string GetSiteSetting(this HttpContext context, string name)
		{
			return GetSiteSetting(context.GetOwinContext(), name);
		}

		public static string GetSiteSetting(this HttpContextBase context, string name)
		{
			return GetSiteSetting(context.GetOwinContext(), name);
		}

		public static string GetSiteSetting(this IOwinContext context, string name)
		{
			var website = GetWebsite(context);
			return website.Settings.Get<string>(name);
		}

		public static ContextLanguageInfo GetContextLanguageInfo(this HttpContext context)
		{
			return GetContextLanguageInfo(context.GetOwinContext());
		}

		public static ContextLanguageInfo GetContextLanguageInfo(this HttpContextBase context)
		{
			return GetContextLanguageInfo(context.GetOwinContext());
		}

		public static ContextLanguageInfo GetContextLanguageInfo(this IOwinContext context)
		{
			return context.Get<ContextLanguageInfo>();
		}

		/// <summary>
		/// Gets the LCID of current CRM language. Ex: if current language is en-CA (4105), then 1033 (en-US) will be returned because that is the associated CRM language.
		/// </summary>
		/// <param name="context">Current Http context.</param>
		/// <returns>LCID of current CRM language.</returns>
		public static int GetCrmLcid(this HttpContext context)
		{
			return GetCrmLcid(context?.GetOwinContext());
		}

		/// <summary>
		/// Gets the LCID of current CRM language. Ex: if current language is en-CA (4105), then 1033 (en-US) will be returned because that is the associated CRM language.
		/// </summary>
		/// <param name="context">Current Http context.</param>
		/// <returns>LCID of current CRM language.</returns>
		public static int GetCrmLcid(this HttpContextBase context)
		{
			return GetCrmLcid(context?.GetOwinContext());
		}

		/// <summary>
		/// Gets the LCID of current CRM language. Ex: if current language is en-CA (4105), then 1033 (en-US) will be returned because that is the associated CRM language.
		/// </summary>
		/// <param name="context">Current Owin context.</param>
		/// <returns>LCID of current CRM language.</returns>
		public static int GetCrmLcid(this IOwinContext context)
		{
			return GetCrmLcid(context?.GetContextLanguageInfo());
		}

		/// <summary>
		/// Gets the LCID of current CRM language. Ex: if current language is en-CA (4105), then 1033 (en-US) will be returned because that is the associated CRM language.
		/// </summary>
		/// <param name="languageInfo">ContextLanguageInfo to extract language info from.</param>
		/// <returns>LCID of current CRM language.</returns>
		public static int GetCrmLcid(this ContextLanguageInfo languageInfo)
		{
			return languageInfo?.ContextLanguage?.CrmLcid ?? CultureInfo.CurrentCulture.LCID;
		}

		public static PortalSolutions GetPortalSolutionsDetails(this HttpContext context)
		{
			return GetPortalSolutionsDetails(context.GetOwinContext());
		}

		public static PortalSolutions GetPortalSolutionsDetails(this HttpContextBase context)
		{
			return GetPortalSolutionsDetails(context.GetOwinContext());
		}

		public static PortalSolutions GetPortalSolutionsDetails(this IOwinContext context)
		{
			return context.Get<PortalSolutions>();
		}

		public static string ToFilterLikeString(this string metadataFilter)
		{
			return !metadataFilter.Contains("*") ? metadataFilter : metadataFilter.Replace('*', '%');
		}
		
		#endregion
	}
}
