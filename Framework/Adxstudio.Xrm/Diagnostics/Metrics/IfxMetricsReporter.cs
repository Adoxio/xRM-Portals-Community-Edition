/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Metrics
{
	using System;
	using System.Collections.Concurrent;
	using System.Globalization;
	using System.Threading;
	using Adxstudio.Xrm.Configuration;
#if CLOUDINSTRUMENTATION
	using Microsoft.Cloud.InstrumentationFramework;
#endif

	/// <summary>
	/// Default metric implementation, uses <see cref="IfxMetricsReporter"/> to send data.
	/// </summary>
	internal sealed class AdxMetric : IAdxMetric
	{
		public AdxMetric(string metricName)
		{
			metricName.ThrowOnNullOrWhitespace("metricName");
			this.MetricName = metricName;
		}

		public string MetricName { get; private set; }

		public void LogValue(long rawValue)
		{
#if CLOUDINSTRUMENTATION
			IfxMetricsReporter.Instance.LogValueForMetric(this, rawValue);
#else
			ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Microsoft.Cloud.InstrumentationFramework assemblies could not be found.");
#endif
		}
		public bool Equals(IAdxMetric other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return string.Equals(this.MetricName, other.MetricName);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as AdxMetric);
		}

		public override int GetHashCode()
		{
			return (this.MetricName != null ? this.MetricName.GetHashCode() : 0);
		}
	}

	/// <summary>
	/// Wraps the IFX metrics API for emitting MDM metrics. Will not throw an exception when invoked, uses 
	/// <see cref="MetricsReportingEvents"/> to emit errors.
	/// </summary>
	internal sealed class IfxMetricsReporter
	{
		static Lazy<IfxMetricsReporter> _lazyInstance = new Lazy<IfxMetricsReporter>(() => new IfxMetricsReporter());

		public static IfxMetricsReporter Instance
		{
			get { return _lazyInstance.Value; }
		}

		/// <summary>
		/// Cloud configuration settings required for reporting ADX metrics.
		/// </summary>
		private static class SettingNames
		{
			public const string MdmAccountName = "Adx.Diagnostics.MdmAccountName";
			public const string MdmNamespace = "Adx.Diagnostics.MdmNamespace";
			public const string Geo = "Adx.Diagnostics.GeoName";
			public const string Tenant = "Adx.Diagnostics.Tenant";
			public const string Role = "Adx.Diagnostics.Role";
			public const string RoleInstance = "Adx.Diagnostics.RoleInstance";
			public const string Org = "Adx.Diagnostics.OrgId";
			public const string PortalType = "Adx.Diagnostics.PortalType";
			public const string PortalId = "Adx.Diagnostics.PortalId";
			public const string PortalApp = "Adx.Diagnostics.PortalApp";
			public const string PortalUrl = "Azure.Authentication.RedirectUri";
			public const string PortalVersion = "Adx.Diagnostics.PortalVersion";

		}

		/// <summary>
		/// Cloud configuration settings required for reporting ADX metrics.
		/// </summary>
		private static class MdmDimensionNames
		{
			public const string Geo = "Geo";
			public const string Tenant = "Tenant";
			public const string Role = "Role";
			public const string RoleInstance = "RoleInstance";
			public const string Org = "Org";
			public const string PortalType = "PortalType";
			public const string PortalId = "PortalId";
			public const string PortalApp = "PortalApp";
			public const string PortalUrl = "PortalUrl";
			public const string PortalVersion = "PortalVersion";
		}

#if CLOUDINSTRUMENTATION
		readonly ConcurrentDictionary<IAdxMetric, MeasureMetric10D> _metricsCache = new ConcurrentDictionary<IAdxMetric, MeasureMetric10D>();
		readonly string _mdmAccountName, _mdmNamespace, _geo, _tenant, _role, _roleInstance, _org, _portalType, _portalId, _portalApp, _portalUrl, _portalVersion;
		readonly bool _initializationFailed;
#endif

		private IfxMetricsReporter()
		{
#if CLOUDINSTRUMENTATION
			try
			{
				this._mdmAccountName = GetCloudConfigurationSettingWithValue(SettingNames.MdmAccountName);
				this._mdmNamespace = GetCloudConfigurationSettingWithValue(SettingNames.MdmNamespace);
				this._geo = GetCloudConfigurationSettingWithValue(SettingNames.Geo);
				this._tenant = GetCloudConfigurationSettingWithValue(SettingNames.Tenant);
				this._role = GetCloudConfigurationSettingWithValue(SettingNames.Role);
				this._roleInstance = GetCloudConfigurationSettingWithValue(SettingNames.RoleInstance);
				this._org = GetCloudConfigurationSettingWithValue(SettingNames.Org);
				this._portalType = GetCloudConfigurationSettingWithValue(SettingNames.PortalType);
				this._portalId = GetCloudConfigurationSettingWithValue(SettingNames.PortalId);
				this._portalApp = GetCloudConfigurationSettingWithValue(SettingNames.PortalApp);
				this._portalUrl = GetCloudConfigurationSettingWithValue(SettingNames.PortalUrl);
				this._portalVersion = GetCloudConfigurationSettingWithValue(SettingNames.PortalVersion);

				if (IfxInitializer.IfxInitializeStatus == IfxInitializer.IfxInitState.IfxUninitalized)
				{
					IfxInitializer.Initialize(this._tenant, this._role, this._roleInstance, "NO_DISK_LOGS");
				}

				this._initializationFailed = false;
			}
			catch (Exception ex)
			{
				if (ex is OutOfMemoryException || ex is ThreadAbortException)
				{
					throw;
				}
				MetricsReportingEvents.Instance.MetricInitializationFailed(ex.ToString());
				this._initializationFailed = true;
			}
#endif
		}

		private static string GetCloudConfigurationSettingWithValue(string settingName)
		{
			var result = settingName.ResolveAppSetting();
			result.ThrowOnNullOrWhitespace("settingName",
				string.Format(CultureInfo.InvariantCulture,
				"Missing value for configuration setting {0}, metrics API cannot initialize.", settingName));
			return result;
		}

		public void LogValueForMetric(IAdxMetric metric, long rawValue)
		{
#if CLOUDINSTRUMENTATION
			if (this._initializationFailed) return;
			var ifxMetric = _metricsCache.GetOrAdd(metric, this.CreateMetric);

			try
			{
				ErrorContext errorContext = new ErrorContext();
				ifxMetric.LogValue(rawValue, this._geo, this._tenant, this._role, this._roleInstance, this._org, this._portalType, this._portalId, this._portalApp, this._portalUrl, this._portalVersion, ref errorContext);
				if (errorContext.ErrorCode != 0)
				{
					MetricsReportingEvents.Instance.MetricReportingFailed(metric.MetricName, "LogValue failed: " + errorContext.ErrorMessage);
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is OutOfMemoryException)
				{
					throw;
				}
				MetricsReportingEvents.Instance.MetricReportingFailed(metric.MetricName, ex.ToString());
			}
#endif
		}

#if CLOUDINSTRUMENTATION
		private MeasureMetric10D CreateMetric(IAdxMetric metric)
		{
			ErrorContext errorContext = new ErrorContext();
			MeasureMetric10D instance = MeasureMetric10D.Create(
				this._mdmAccountName,
				this._mdmNamespace,
				metric.MetricName,
				MdmDimensionNames.Geo,
				MdmDimensionNames.Tenant,
				MdmDimensionNames.Role,
				MdmDimensionNames.RoleInstance,
				MdmDimensionNames.Org,
				MdmDimensionNames.PortalType,
				MdmDimensionNames.PortalId,
				MdmDimensionNames.PortalApp,
				MdmDimensionNames.PortalUrl,
				MdmDimensionNames.PortalVersion,
				ref errorContext);

			if (errorContext.ErrorCode != 0)
			{
				MetricsReportingEvents.Instance.MetricReportingFailed(metric.MetricName, "CreateMetric failed: " + errorContext.ErrorMessage);
			}

			return instance;
		}
#endif
	}
}
