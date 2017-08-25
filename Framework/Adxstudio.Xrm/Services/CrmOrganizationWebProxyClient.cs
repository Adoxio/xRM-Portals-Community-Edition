/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Diagnostics.Tracing;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.WebServiceClient;

	/// <summary>
	///  CrmOrganizationWebProxyClient class
	/// </summary>
	public class CrmOrganizationWebProxyClient : OrganizationWebProxyClient
	{
		/// <summary>ICrmTokenManager instance</summary>
		private readonly ICrmTokenManager crmTokenManager;

		/// <summary>
		///    Initializes a new instance of the <see cref="CrmOrganizationWebProxyClient"/> class.
		/// </summary>
		/// <param name="serviceUrl">Type: Returns_URI. The URL of the Organization web service.</param>
		/// <param name="useStrongTypes">Type: Returns_Assembly. An assembly containing early-bound types.</param>
		/// <param name="crmTokenManager">Type: Returns_ICrmTokenManager token manager.</param>
		public CrmOrganizationWebProxyClient(Uri serviceUrl, bool useStrongTypes, ICrmTokenManager crmTokenManager)
			: base(serviceUrl, useStrongTypes)
		{
			this.crmTokenManager = crmTokenManager;
			this.CreateNewInitializer();
		}

		/// <summary>
		/// Creates the WCF proxy client initializer
		/// </summary>
		/// <returns>Type: <see cref="T:Microsoft.Xrm.Sdk.WebServiceClient.WebProxyClientContextInitializer`1"></see>A web proxy client context initializer.</returns>
		protected sealed override WebProxyClientContextInitializer<IOrganizationService> CreateNewInitializer()
		{
			this.HeaderToken = this.crmTokenManager.GetToken();
			var initializer = base.CreateNewInitializer();
			AddUserAgent();

			return initializer;
		}

		/// <summary>
		/// Add a UserAgent with the supplied information
		/// </summary>
		private static void AddUserAgent()
		{
			if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
			{
				var property = OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;

				if (property != null)
				{
					// Portals (PortalApp={PortalApp}; Instance={Instance}; ActivityId={ActivityId})
					var userAgent = $"Portals (PortalApp={PortalDetail.Instance.PortalApp}; Instance={WebAppSettings.Instance.InstanceId}; ActivityId={EventSource.CurrentThreadActivityId})";
					ADXTrace.Instance.TraceVerbose(TraceCategory.Application, $"Set UserAgent: {userAgent}");
					property.Headers[HttpRequestHeader.UserAgent] = userAgent;
				}
			}
		} 
	}
}
