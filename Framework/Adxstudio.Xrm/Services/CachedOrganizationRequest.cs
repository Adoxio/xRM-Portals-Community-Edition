/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Configuration;
	using System.Runtime.CompilerServices;
	using System.Runtime.Serialization;
	using Adxstudio.Xrm.Core.Flighting;
	using Microsoft.Xrm.Sdk;
	using Newtonsoft.Json;
	using Query;

	/// <summary>
	/// A customized <see cref="OrganizationRequest"/> containing telemetry.
	/// </summary>
	[Serializable]
	[DataContract(Name = "RetrieveMultipleRequest", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
	[KnownType(typeof(CachedOrganizationRequest))]
	internal sealed class CachedOrganizationRequest : OrganizationRequest
	{
		/// <summary>
		/// Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.
		/// </summary>
		private RequestFlag flag = RequestFlag.None;

		/// <summary>
		/// The wrapped request.
		/// </summary>
		[DataMember]
		public OrganizationRequest Request { get; private set; }

		/// <summary>
		/// The cache item telemetry.
		/// </summary>
		[DataMember]
		public CacheItemTelemetry Telemetry { get; private set; }

		/// <summary>
		/// Gets or sets the cache expiration.
		/// </summary>
		public TimeSpan? CacheExpiration { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedOrganizationRequest" /> class.
		/// </summary>
		/// <param name="request">The inner request.</param>
		/// <param name="caller">The caller.</param>
		public CachedOrganizationRequest(OrganizationRequest request, Caller caller)
			: this(request)
		{
			this.Telemetry = new CacheItemTelemetry(request, caller);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedOrganizationRequest"/> class.
		/// </summary>
		/// <param name="request">The inner request.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="caller">The caller.</param>
		public CachedOrganizationRequest(OrganizationRequest request, RequestFlag flag, TimeSpan? expiration, Caller caller)
			: this(request)
		{
			this.Telemetry = new CacheItemTelemetry(request, caller);
			this.flag = flag;
			this.CacheExpiration = expiration;
		}

		/// <summary>
		/// Get the Fetch Request from the Reuest based on type
		/// </summary>
		/// <returns> Fetch Query in Request</returns>
		public Fetch ToFetch()
		{
			Fetch fQuery = null;

			if (this.Request is FetchMultipleRequest)
			{
				fQuery = (this.Request as FetchMultipleRequest).Fetch;
			}
			else if (this.Request is RetrieveSingleRequest)
			{
				fQuery = (this.Request as RetrieveSingleRequest).Fetch;
			}

			return fQuery;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedOrganizationRequest" /> class.
		/// </summary>
		/// <param name="request">The wrapped request.</param>
		private CachedOrganizationRequest(OrganizationRequest request)
		{
			this.Request = request;
			this.ExtensionData = request.ExtensionData;
			this.Parameters = request.Parameters;
			this.RequestId = request.RequestId;
			this.RequestName = request.RequestName;
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="CachedOrganizationRequest" /> class from being created.
		/// </summary>
		/// <remarks>
		/// Required for json deserialization.
		/// </remarks>
		[JsonConstructor]
		private CachedOrganizationRequest()
		{
		}

		/// <summary>
		/// Checks if particular a flag is enabled for the given request or not.
		/// </summary>
		/// <param name="value"> The value</param>
		/// <returns> Returns true if the flag is enabled</returns>
		public bool IsFlagEnabled(RequestFlag value)
		{
			switch (value)
			{
				case RequestFlag.AllowStaleData:
					if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.PortalAllowStaleData))
					{
						return (this.flag & value) == value;
					}
					return false;
				default:
					return (this.flag & value) == value;
			}
		}
	}

	/// <summary>
	/// The request flag
	/// 1. BypassCacheInvalidation - To skip cache invlaidation when quering. The cache invalidation would eventually happen when notification is received from event hub.
	/// 2. AllowStaleData - Set this 
	/// </summary>
	[Flags]
	public enum RequestFlag
	{
		None = 0x0,
		ByPassCacheInvalidation = 0x1,
		AllowStaleData = 0x2,
		SkipDependencyCalculation = 0x4,
	}
}
