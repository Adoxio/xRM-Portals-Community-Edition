/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Modules;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Routing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Site;
using Site.Areas.Setup;
using CrmSection = Microsoft.Xrm.Client.Configuration.CrmSection;
using CrmSectionCreatedEventArgs = Microsoft.Xrm.Client.Configuration.CrmSectionCreatedEventArgs;

[assembly: PreApplicationStartMethod(typeof(SetupConfig), "PreApplicationStart")]

namespace Site
{
	public class SetupRoutingModule : PortalRoutingModule
	{
		protected override RouteCollection Register(RouteCollection routes, IRouteHandler portalRouteHandler, IEmbeddedResourceRouteHandler embeddedResourceRouteHandler, IEmbeddedResourceRouteHandler scriptHandler)
		{
			RegisterEmbeddedResourceRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);
			return routes;
		}
	}

	public static class SetupConfig
	{
		private static readonly Lazy<SetupManager> _setupManager = new Lazy<SetupManager>(GetSetupManager);

		public static SetupManager SetupManager
		{
			get { return _setupManager.Value; }
		}

		private static readonly Lazy<CrmSection> _crmSection = new Lazy<CrmSection>(CrmConfigurationManager.GetCrmSection);

		public static CrmSection Section
		{
			get { return _crmSection.Value; }
		}

		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/js/jqueryval.bundle.js").Include("~/js/jquery.unobtrusive-ajax.min.js", "~/js/jquery.validate.min.js"));
		}

		public static void PreApplicationStart()
		{
			if (!SetupConfig.IsPortalConfigured())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Configuring setup components.");

				DynamicModuleUtility.RegisterModule(typeof(SetupRoutingModule));
				RegisterBundles(BundleTable.Bundles);
			}
			else
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Configuring standard portal components.");

				DynamicModuleUtility.RegisterModule(typeof(PortalRoutingModule));

				AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

				// read the connection settings from the local file

				CrmConfigurationProvider.ConfigurationCreated += OnConfigurationCreated;
				PortalCrmConfigurationProvider.ConfigurationCreated += OnConfigurationCreated;
			}
		}

		public static bool ApplicationStart()
		{
			if (InitialSetupIsRunning())
			{
				// the setup process is in progress

				RegisterInitialSetupRoutes(RouteTable.Routes);

				return true;
			}

			if (ProvisioningInProgress())
			{
				RegisterProvisioningInProgressRoutes(RouteTable.Routes);

				return true;
			}

			return false;
		}

		/// <summary>
		/// returns true if the portal is configured and provisioned
		/// </summary>
		/// <returns>true if provisioned, configured, and setup; otherwise false</returns>
		public static bool IsPortalConfigured()
		{
			return !InitialSetupIsRunning() && !ProvisioningInProgress();
		}

		public static bool InitialSetupIsRunning()
		{
			if (CrmConnectionExists()) return false;
			if (SetupManager.Exists()) return false;
			if (!SetupManager.Enabled()) return false;

			return true;
		}

		public static bool ProvisioningInProgress()
		{
			if (!PortalSettings.Instance.UseOnlineSetup) return false;

			bool inProgress;
			var setupInProgress = bool.TryParse(ConfigurationManager.AppSettings["PortalSetupInProgress"], out inProgress) && inProgress;

			if (!setupInProgress)
			{
				return false;
			}

			var websiteId = ConfigurationManager.AppSettings["PortalPackageName"];
			
			// Try to query the CRM for a PackageImportComplete setting to see if the package may have been installed already.
			try
			{
				var query = new QueryExpression("adx_setting") { ColumnSet = new ColumnSet("adx_name") };
				query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
				query.Criteria.AddCondition("adx_name", ConditionOperator.Equal, "PackageImportComplete");
				query.Criteria.AddCondition("adx_value", ConditionOperator.Equal, websiteId);

				var service = CrmConfigurationManager.CreateService();

				if (service.RetrieveMultiple(query).Entities.Count > 0)
				{
					return false;
				}
			}
			catch (Exception e)
			{
				// Ignore connection exceptions

				ADXTrace.Instance.TraceInfo(TraceCategory.Exception, string.Format("PackageImportComplete: Provisioning is in progress: {0}", e));
			}

			// If there is a website record & the plugins are enabled then we can create a website binding. 
			try
			{
				var adxWebsiteQuery = new QueryExpression("adx_website");
				adxWebsiteQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
				adxWebsiteQuery.Criteria.AddCondition("adx_websiteid", ConditionOperator.Equal, websiteId);

				var service = CrmConfigurationManager.CreateService();

				if (service.RetrieveMultiple(adxWebsiteQuery).Entities.Count > 0)
				{
					// Now Check if the plugins are enabled. If they are enabled then we know that the data import is complete or if
					//  its not then the cache invalidation plugin is enabled so the webapp will be told of any new changes. 
					var webNotificationPluginQuery = new QueryExpression("pluginassembly");
					webNotificationPluginQuery.Criteria.AddCondition("name", ConditionOperator.Equal, "Adxstudio.Xrm.Plugins.WebNotification");
					webNotificationPluginQuery.Criteria.AddCondition("componentstate", ConditionOperator.Equal, 0);

					if (service.RetrieveMultiple(webNotificationPluginQuery).Entities.Count > 0)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Ending provisioning in progress. Creating the AdxSetting");
						SetSettingForPackageComplete(service, websiteId);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				// Ignore connection exceptions

				ADXTrace.Instance.TraceInfo(TraceCategory.Exception, string.Format("Website: Provisioning is in progress: {0}", e));
			}

			return true;
		}

		/// <summary>
		/// Creates a record for the Adx_setting entity to show that provisioning has been completed.
		/// </summary>
		/// <param name="service">
		/// The service.
		/// </param>
		/// <param name="websiteId">
		/// The website id.
		/// </param>
		private static void SetSettingForPackageComplete(IOrganizationService service, string websiteId)
		{
			try
			{
				var entity = new Entity("adx_setting")
				{
					Attributes =
					{
						["adx_name"] = "PackageImportComplete",
						["adx_value"] = websiteId
					}
				};

				service.Create(entity);
			}
			catch (Exception e)
			{
				// If something goes wrong ignore it get the site running.
				ADXTrace.Instance.TraceError(TraceCategory.Exception, string.Format("Failed to create the PackageImportComplete Record with Exception: {0}", e));
				WebEventSource.Log.GenericErrorException(e);
			}
		} 

		private static bool CrmConnectionExists()
		{
			if (ConfigurationManager.ConnectionStrings["Xrm"] != null) return true;
			if (Section.ConnectionStrings["Xrm"] != null) return true;
			return false;
		}

		private static void RegisterInitialSetupRoutes(RouteCollection routes)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Configuring setup routes.");

			routes.RegisterRoutesWithLock(r =>
			{
				r.IgnoreRoute("{resource}.axd/{*pathInfo}");

				r.Add(new SetupRoute(
					"{area}/{controller}/{action}",
					new { area = "Setup", controller = "Setup", action = "Index" },
					new { area = "Setup", controller = "Setup" }));
			});
		}
		
		private static void OnConfigurationCreated(object sender, CrmSectionCreatedEventArgs args)
		{
			if (CrmConnectionExists()) return;

			args.Configuration.ConnectionStrings.Add(GetConnectionString());
		}

		private static void OnConfigurationCreated(object sender, PortalCrmSectionCreatedEventArgs args)
		{
			var portals = args.Configuration.Portals
				.Cast<PortalContextElement>()
				.Where(portal => portal.Parameters["websiteName"] == null && portal.WebsiteSelector.Parameters["websiteName"] == null)
				.ToList();

			if (!portals.Any()) return;

			var websiteName = GetWebsiteName();

			if (!string.IsNullOrWhiteSpace(websiteName))
			{
				foreach (var portal in portals)
				{
					portal.Parameters["websiteName"] = websiteName;
					portal.WebsiteSelector.Parameters["websiteName"] = websiteName;
				}
			}
		}

		private static ConnectionStringSettings GetConnectionString()
		{
			var connectionString = SetupManager.GetConnectionString();

			if (connectionString == null)
			{
				throw new ConfigurationErrorsException("The connection is undefined.");
			}

			return connectionString;
		}

		private static string GetWebsiteName()
		{
			return SetupManager.GetWebsiteName();
		}

		private static SetupManager GetSetupManager()
		{
			var type = TypeExtensions.GetType("Site.Global");

			if (type != null)
			{
				var method = type.GetMethod("GetSetupManager", BindingFlags.Public | BindingFlags.Static);

				if (method != null)
				{
					return (SetupManager)method.Invoke(null, null);
				}
			}

			return DefaultSetupManager.Create();
		}

		private static void RegisterProvisioningInProgressRoutes(RouteCollection routes)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Configuring provisioning routes.");

			routes.RegisterRoutesWithLock(r =>
			{
				r.IgnoreRoute("{resource}.axd/{*pathInfo}");

				r.Add(new SetupRoute(
					"{area}/{controller}/{action}",
					new { area = "Setup", controller = "Setup", action = "Provisioning" },
					new { area = "Setup", controller = "Setup" }));
			});
		}
	}
}
