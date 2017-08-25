/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site
{
	using System;
	using System.Web;
	using System.Web.Optimization;
	using Adxstudio.Xrm.AspNet;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.AspNet.PortalBus;
	using Adxstudio.Xrm.Core.Telemetry;
	using Adxstudio.Xrm.EventHubBasedInvalidation;
	using Adxstudio.Xrm.Performance;
	using Adxstudio.Xrm.Services.Cache;
	using Adxstudio.Xrm.Web;
	using Microsoft.Owin.Extensions;
	using Owin;
	using Site.Areas.Account.Models;

	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			try
			{
				this.ConfigurePortal(app);
			}
			catch (Exception ex)
			{
				WebEventSource.Log.UnhandledException(ex);
				throw;
			}
		}

		public void ConfigurePortal(IAppBuilder app)
		{
			var host = new PortalHostingEnvironment();
			var webAppSettings = WebAppSettings.Instance;

			// For cache we create one subscription per instance of web app, that's why we use instance id as the subscription name.
			var cacheEventHubJobSettings = new EventHubJobSettings(webAppSettings.InstanceId, EventHubSubscriptionType.CacheSubscription);

			// For search we create one subscription per webapp, that's why we use site name as the subscription name.
			var searchEventHubJobSettings = new EventHubJobSettings(webAppSettings.SiteName, EventHubSubscriptionType.SearchSubscription);

			var requireSslOptions = new RequireSslOptions(webAppSettings);

			var warmupCacheSettings = new WarmupCacheSettings();

			app.ConfigureApplicationLifecycleEvents();
			app.UseETWMiddleware();
			app.UseRequireSsl(requireSslOptions);
			app.UseAppInfo();
			app.UseHealth();
			app.UseScaleOutTelemetry();
			app.UseRequestTelemetry(SetupConfig.IsPortalConfigured);
			app.UseApplicationRestartPluginMessage(new PluginMessageOptions());
			app.UsePortalBus<ApplicationRestartPortalBusMessage>(webAppSettings, cacheEventHubJobSettings);
			app.UsePortalBus<CacheInvalidationPortalBusMessage>(webAppSettings, cacheEventHubJobSettings);
			app.CreatePerOwinContext<RequestElapsedTimeContext>(RequestElapsedTimeContext.Create);

			if (!SetupConfig.InitialSetupIsRunning() && !SetupConfig.ProvisioningInProgress())
			{
				using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.Startup, PerformanceMarkerArea.Crm, PerformanceMarkerTagName.StartUpConfiguration))
				{
					// indepdendent components
					app.CreatePerOwinContext(ApplicationDbContext.Create);
					var portalSolutions = app.ConfigurePortalSolutionsDetails(ApplicationDbContext.Create());
					app.CreatePerOwinContext<ApplicationOrganizationManager>(ApplicationOrganizationManager.Create);
					app.CreatePerOwinContext<ApplicationWebsiteManager>(ApplicationWebsiteManager.Create);
					app.CreatePerOwinContext<CrmWebsite>((options, context) => ApplicationWebsite.Create(options, context, host));

					// Set the culture for this request.
					app.UseCurrentThreadCulture();

					ApplicationWebsiteManager websiteManager;

					try
					{
						var settings = new CrmEntityStoreSettings { PortalSolutions = portalSolutions };
						websiteManager = ApplicationWebsiteManager.Create(ApplicationDbContext.Create(), settings);
					}
					catch
					{
						//We need to unload app domain in order to reinitialize owin during next request
						TelemetryState.ApplicationEndInfo = ApplicationEndFlags.Configuration;
						Adxstudio.Xrm.Web.Extensions.RestartWebApplication();
						return;
					}

					var website = websiteManager.Find(HttpContext.Current.Request.RequestContext, host);
					var hstsOptions = new StrictTransportSecurityOptions(website);

					// components that depend on the website
					app.UpdatePrimaryDomainName(websiteManager, website, portalSolutions);
					app.ConfigureDisplayModes(website);
					app.UseWebsiteHeaderSettings(website);
					app.UseStrictTransportSecuritySettings(hstsOptions);
					app.ConfigureSearchProvider(website);

					var contentMapProvider = app.ConfigureContentMap(ApplicationDbContext.Create, website, cacheEventHubJobSettings, portalSolutions);

					// configure user dependencies
					this.ConfigureAuth(app, website);

					// components that depend on the user
					app.CreatePerOwinContext<ContextLanguageInfo>(ContextLanguageInfo.Create);

					// Complete the authentication stage prior to invoking page handler
					app.UseStageMarker(PipelineStage.Authenticate);

					// components that depend on content map
					app.ConfigureCulture(website, contentMapProvider, BundleTable.Bundles, BundleConfig.RegisterLanguageSpecificBundles);

					// tail end components
					app.ConfigureEventHubCacheInvalidation(website.Id, ApplicationDbContext.Create, cacheEventHubJobSettings, searchEventHubJobSettings, new CacheInvalidationJobSettings(webAppSettings));
					app.WarmupCache(ApplicationDbContext.Create, warmupCacheSettings);
					app.StartupComplete();
				}
			}
		}
	}
}
