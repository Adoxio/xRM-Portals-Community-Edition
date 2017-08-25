/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Concurrent;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	public class WebAppConfigurationProvider
	{
		private static string appStartTime;
		private static ConcurrentDictionary<string, bool> portalUsedEntitiesList = new ConcurrentDictionary<string, bool>(Environment.ProcessorCount * 2, 10009);

		private static T GetAppSetting<T>(string appSetting)
		{
			try
			{
				//We are storing the appSetting in Azure app settings section.
				//ConfigurationManager reads the setting from Azure if it's running on cloud otherwise from app.config.
				string appSettingValue = ConfigurationManager.AppSettings[appSetting];
				if (appSettingValue != null)
				{
					return (T)Convert.ChangeType(appSettingValue, typeof(T));
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
			}

			return default(T);
		}

		public static bool GetTimeTrackingTelemetryString()
		{
			return WebAppConfigurationProvider.GetAppSetting<bool>(Constants.TimeTrackingTelemetry);
		}

		public static string AppStartTime
		{
			get
			{
				if (appStartTime != null)
					return appStartTime;
				else
					return System.DateTime.UtcNow.ToString(("MM/dd/yyyy HH:mm:ss"));
			}
			set
			{
				appStartTime = value;
			}
		}

		/// <summary>
		/// Returns the List of Portal Used Entities
		/// </summary>
		public static ConcurrentDictionary<string, bool> PortalUsedEntities
		{
			get
			{
				return portalUsedEntitiesList;
			}
		}

		/// <summary>
		/// This function add List of Entities Enabled for Portal Search
		/// </summary>
		/// <returns></returns>
		public static ConcurrentDictionary<string, bool> GetPortalEntityList()
		{
			var searchEnabledEntities = SearchMetadataCache.Instance.GetPortalSearchEnabledEntities();
			foreach (string entity in searchEnabledEntities)
			{
				if (!string.IsNullOrWhiteSpace(entity))
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Entity {0} is Added to Enabled Entity List for Portal Search", entity));
					PortalUsedEntities.TryAdd(entity, true);
				}
			}
			return portalUsedEntitiesList;
		}

	}
}
