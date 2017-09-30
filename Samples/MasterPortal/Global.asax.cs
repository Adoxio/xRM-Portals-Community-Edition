/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site
{
	using Adxstudio.Xrm.Core.Flighting;
	using System;
	using System.Configuration;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Configuration;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using System.Web.Optimization;
	using System.Web.Profile;
	using System.Web.Routing;
	using System.Web.Security;
	using System.Web.WebPages;
	using Adxstudio.Xrm;
	using Adxstudio.Xrm.Core.Telemetry;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Portal.Configuration;

	public class Global : HttpApplication
	{
		private static bool _setupRunning;

		void Application_Start(object sender, EventArgs e)
		{
			AntiForgeryConfig.CookieName = "__RequestVerificationToken"; // static name as its dependent on the ajax handler.
			MvcHandler.DisableMvcResponseHeader = true;
			_setupRunning = SetupConfig.ApplicationStart();

			if (_setupRunning) return;

			var areaRegistrationState = new PortalAreaRegistrationState();
			Application[typeof(IPortalAreaRegistrationState).FullName] = areaRegistrationState;

			AreaRegistration.RegisterAllAreas(areaRegistrationState);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			Session["init"] = 0;

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage)
				// ignore non-user pings
				&& TelemetryState.IsTelemetryEnabledUserAgent()
				// ignore requests to and referred requests from specific paths
				&& TelemetryState.IsTelemetryEnabledRequestPage()
				// only report this telemetry when the portal is configured
				&& SetupConfig.IsPortalConfigured())
			{
				PortalFeatureTrace.TraceInstance.LogSessionInfo(FeatureTraceCategory.SessionStart);
			}
		}

		void Application_Error(object sender, EventArgs e)
		{
			WebEventSource.Log.UnhandledException(Server.GetLastError());
		}

		public override string GetVaryByCustomString(HttpContext context, string arg)
		{
			switch (arg)
			{
				case "roles":
					return GetVaryByRolesString(context);
				case "roles;website":
					return GetVaryByRolesAndWebsiteString(context);
				case "user":
					return GetVaryByUserString(context);
				case "user;website":
					return GetVaryByUserAndWebsiteString(context);
				case "website":
					return GetVaryByWebsiteString(context);
				case "culture":
					return CultureInfo.CurrentCulture.LCID.ToString();
				case "user;language":
					return string.Format("{0}{1}", GetVaryByUserString(context), GetVaryByLanguageString());
				case "user;website;language":
					return string.Format("{0}{1}", GetVaryByUserAndWebsiteString(context), GetVaryByLanguageString());
				case "website;language":
					return string.Format("{0}{1}", GetVaryByWebsiteString(context), GetVaryByLanguageString());
			}

			return base.GetVaryByCustomString(context, arg);
		}

		public void Profile_MigrateAnonymous(object sender, ProfileMigrateEventArgs e)
		{
			var portalAreaRegistrationState = Application[typeof(IPortalAreaRegistrationState).FullName] as IPortalAreaRegistrationState;

			if (portalAreaRegistrationState != null)
			{
				portalAreaRegistrationState.OnProfile_MigrateAnonymous(sender, e);
			}
		}

		private static string GetVaryByLanguageString()
		{
			var langContext = HttpContext.Current.GetContextLanguageInfo();
			if (langContext.IsCrmMultiLanguageEnabled)
			{
				return string.Format(",{0}", langContext.ContextLanguage.Code);
			}

			return string.Empty;
		}

		private static string GetVaryByDisplayModesString(HttpContext context)
		{
			var availableDisplayModeIds = DisplayModeProvider.Instance
				.GetAvailableDisplayModesForContext(context.Request.RequestContext.HttpContext, null)
				.Select(mode => mode.DisplayModeId);

			return string.Join(",", availableDisplayModeIds);
		}

		private static string GetVaryByRolesString(HttpContext context)
		{
			// If the role system is not enabled, fall back to varying cache by user.
			if (!Roles.Enabled)
			{
				return GetVaryByUserString(context);
			}

			var roles = context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated
				? Roles.GetRolesForUser()
					.OrderBy(role => role)
					.Aggregate(new StringBuilder(), (sb, role) => sb.AppendFormat("[{0}]", role))
					.ToString()
				: string.Empty;

			return string.Format("IsAuthenticated={0},Roles={1},DisplayModes={2}",
				context.Request.IsAuthenticated,
				roles.GetHashCode(),
				GetVaryByDisplayModesString(context).GetHashCode());
		}

		private static string GetVaryByRolesAndWebsiteString(HttpContext context)
		{
			return string.Format("{0},{1}", GetVaryByRolesString(context), GetVaryByWebsiteString(context));
		}

		private static string GetVaryByUserString(HttpContext context)
		{
			return string.Format("IsAuthenticated={0},Identity={1},Session={2},DisplayModes={3}",
				context.Request.IsAuthenticated,
				GetIdentity(context),
				GetSessionId(context),
				GetVaryByDisplayModesString(context).GetHashCode());
		}

		private static object GetIdentity(HttpContext context)
		{
			return context.Request.IsAuthenticated && context.User != null && context.User.Identity != null
				? context.User.Identity.Name
				: string.Empty;
		}

		private static readonly SessionStateSection SessionStateConfigurationSection = ConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;

		private static object GetSessionId(HttpContext context)
		{
			if (!context.Request.IsAuthenticated || SessionStateConfigurationSection == null)
			{
				return string.Empty;
			}

			var sessionCookie = context.Request.Cookies[SessionStateConfigurationSection.CookieName];

			return sessionCookie == null ? string.Empty : sessionCookie.Value;
		}

		private static string GetVaryByUserAndWebsiteString(HttpContext context)
		{
			return string.Format("{0},{1}", GetVaryByUserString(context), GetVaryByWebsiteString(context));
		}

		private static string GetVaryByWebsiteString(HttpContext context)
		{
			var websiteId = GetWebsiteIdFromOwinContext(context) ?? GetWebsiteIdFromPortalContext(context);

			return websiteId == null ? "Website=" : string.Format("Website={0}", websiteId.GetHashCode());
		}

		private static string GetWebsiteIdFromOwinContext(HttpContext context)
		{
			var owinContext = context.GetOwinContext();

			if (owinContext == null)
			{
				return null;
			}

			var website = owinContext.GetWebsite();

			return website == null ? null : website.Id.ToString();
		}

		private static string GetWebsiteIdFromPortalContext(HttpContext context)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(request: context.Request.RequestContext);

			if (portalContext == null)
			{
				return null;
			}

			return portalContext.Website == null ? null : portalContext.Website.Id.ToString();
		}
	}
}
