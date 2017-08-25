/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Security
{
	using System;
	using System.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;

	/// <summary> The website right. </summary>
	public enum WebsiteRight
	{
		ManageContentSnippets,
		ManageSiteMarkers,
		ManageWebLinkSets,
		PreviewUnpublishedEntities
	}

	/// <summary> The WebsiteAccessPermissionProvider interface. </summary>
	public interface IWebsiteAccessPermissionProvider
	{
		/// <summary> The try assert. </summary>
		/// <param name="serviceContext"> The service context. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		bool TryAssert(OrganizationServiceContext serviceContext, WebsiteRight right);
	}

	/// <summary> The request caching website access permission provider. </summary>
	internal class RequestCachingWebsiteAccessPermissionProvider : IWebsiteAccessPermissionProvider
	{
		/// <summary> The _underlying provider. </summary>
		private readonly IWebsiteAccessPermissionProvider _underlyingProvider;

		/// <summary> The website. </summary>
		private readonly EntityReference _website;

		/// <summary> Initializes a new instance of the <see cref="RequestCachingWebsiteAccessPermissionProvider"/> class. </summary>
		/// <param name="underlyingProvider"> The underlying provider. </param>
		/// <param name="website"> The website. </param>
		public RequestCachingWebsiteAccessPermissionProvider(IWebsiteAccessPermissionProvider underlyingProvider, EntityReference website)
		{
			if (underlyingProvider == null)
			{
				throw new ArgumentNullException("underlyingProvider");
			}
			if (website == null)
			{
				throw new ArgumentNullException("website");
			}

			this._underlyingProvider = underlyingProvider;
			this._website = website;
		}

		/// <summary> The try assert. </summary>
		/// <param name="serviceContext"> The service context. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		public bool TryAssert(OrganizationServiceContext serviceContext, WebsiteRight right)
		{
			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return _underlyingProvider.TryAssert(serviceContext, right);
			}

			var cacheKey = "{0}:{1}:{2}".FormatWith(GetType().FullName, _website.Id, right);

			var cachedValue = httpContext.Items[cacheKey];

			if (cachedValue is bool)
			{
				return (bool)cachedValue;
			}

			var value = _underlyingProvider.TryAssert(serviceContext, right);

			httpContext.Items[cacheKey] = value;

			return value;
		}
	}
}
