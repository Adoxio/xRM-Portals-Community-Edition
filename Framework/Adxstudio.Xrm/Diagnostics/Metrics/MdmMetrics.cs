/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Metrics
{
	public static class MdmMetrics
	{
		public static readonly IAdxMetric RequestExecutionTimeMetric = AdxMetrics.CreateMetric("Request Execution time (milliseconds)");
		public static readonly IAdxMetric CacheMissedMetric = AdxMetrics.CreateMetric("Cache misses");

		public static readonly IAdxMetric CmsHomeSiteMarkerNotFoundMetric = AdxMetrics.CreateMetric("CMS - Home Site Marker Not Found");
		public static readonly IAdxMetric CmsWebsiteBindingNotFoundMetric = AdxMetrics.CreateMetric("CMS - Website Binding Not Found");

		public static readonly IAdxMetric CrmServicesUnableToConnectMetric = AdxMetrics.CreateMetric("CRM Services - Unable to connect");

		public static readonly IAdxMetric CrmOrganizationRequestExecutionTimeMetric =
			AdxMetrics.CreateMetric("CRM OrganizationRequest Execution Time (milliseconds)");

		public static readonly IAdxMetric WebUnhandledExceptionMetric = AdxMetrics.CreateMetric("ASP.NET - Unhandled Exception");
		public static readonly IAdxMetric WebGenericErrorExceptionMetric = AdxMetrics.CreateMetric("Web - Generic Error Exception");

		//Monitors the Portal Heartbeat. Independent of CRM Connectivity. Basic monitor to track the portal Ups and Downs
		public static readonly IAdxMetric PortalHeartbeat = AdxMetrics.CreateMetric("Portal Heartbeat");

		public static readonly IAdxMetric UserRegistrationSignup = AdxMetrics.CreateMetric("User Registration Signup");
	}
}
