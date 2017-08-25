/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Trace
{
	using System;
	using System.Reflection;
	using Adxstudio.Xrm.Configuration;

	/// <summary>
	/// Cloud hosted portal details.
	/// </summary>
	public class PortalDetail
	{
		/// <summary>
		/// Cloud configuration setting required for SPLUNK reporting
		/// </summary>
		public static class PortalDetailName
		{
			/// <summary>
			/// Adx Diagnostics Geo Name
			/// </summary>
			public const string Geo = "Adx.Diagnostics.GeoName";

			/// <summary>
			/// Adx Diagnostics Tenant
			/// </summary>
			public const string Tenant = "Adx.Diagnostics.Tenant";

			/// <summary>
			/// Adx Diagnostics Organization ID
			/// </summary>
			public const string OrgId = "Adx.Diagnostics.OrgId";

			/// <summary>
			/// Adx Diagnostics Portal ID
			/// </summary>
			public const string PortalId = "Adx.Diagnostics.PortalId";

			/// <summary>
			/// Adx Diagnostics Portal App
			/// </summary>
			public const string PortalApp = "Adx.Diagnostics.PortalApp";

			/// <summary>
			/// Adx Diagnostics Portal Type
			/// </summary>
			public const string PortalType = "Adx.Diagnostics.PortalType";

			/// <summary>
			/// Portal Url
			/// </summary>
			public const string AzurePortalUrl = "Azure.Authentication.RedirectUri";

			/// <summary>
			/// Portal Version
			/// </summary>
			public const string PortalVersion = "Adx.Diagnostics.PortalVersion";

			/// <summary>
			/// Portal Version
			/// </summary>
			public const string PortalProductionOrTrialType = "Adx.Diagnostics.PortalProductionOrTrialType";
		}

		/// <summary>
		/// Returns an instance of the <see cref="PortalDetail" /> class.
		/// </summary>
		public static PortalDetail Instance
		{
			get { return Detail.Value; }
		}

		/// <summary>
		/// Portal Id GUID
		/// </summary>
		public string PortalId { get; private set; }

		/// <summary>
		/// Tenant ID GUID
		/// </summary>
		public string TenantId { get; private set; }

		/// <summary>
		/// Organization ID GUID
		/// </summary>
		public string OrgId { get; private set; }

		/// <summary>
		/// Geographic Location
		/// </summary>
		public string Geo { get; private set; }

		/// <summary>
		/// Portal Application
		/// </summary>
		public string PortalApp { get; private set; }

		/// <summary>
		/// Portal Type
		/// </summary>
		public string PortalType { get; private set; }

		/// <summary>
		/// Portal Url
		/// </summary>
		public string AzurePortalUrl { get; private set; }

		/// <summary>
		/// Portal Version
		/// </summary>
		public string PortalVersion { get; private set; }

		/// <summary>
		/// Portal production or trial type
		/// </summary>
		public string PortalProductionOrTrialType { get; private set; }

		/// <summary>
		/// Returns an instance of the <see cref="PortalDetail" /> class.
		/// </summary>
		private static readonly Lazy<PortalDetail> Detail = new Lazy<PortalDetail>(() => new PortalDetail());

		/// <summary>
		/// Prevents a default instance of the <see cref="PortalDetail" /> class from being created.
		/// </summary>
		private PortalDetail()
		{
			this.PortalId = PortalDetailName.PortalId.ResolveAppSetting() ?? string.Empty;
			this.TenantId = PortalDetailName.Tenant.ResolveAppSetting() ?? string.Empty;
			this.OrgId = PortalDetailName.OrgId.ResolveAppSetting() ?? string.Empty;
			this.Geo = PortalDetailName.Geo.ResolveAppSetting() ?? string.Empty;
			this.PortalApp = PortalDetailName.PortalApp.ResolveAppSetting() ?? string.Empty;
			this.PortalType = PortalDetailName.PortalType.ResolveAppSetting() ?? string.Empty;
			this.AzurePortalUrl = PortalDetailName.AzurePortalUrl.ResolveAppSetting() ?? string.Empty;
			this.PortalVersion = PortalDetailName.PortalVersion.ResolveAppSetting().GetValueOrDefault(Assembly.GetExecutingAssembly().GetName().Version.ToString());
			this.PortalProductionOrTrialType = PortalDetailName.PortalProductionOrTrialType.ResolveAppSetting() ?? string.Empty;
		}
	}
}
