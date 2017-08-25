/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Security
{
	public class PreviewPermission
	{
		private const string PreviewModeCookieName = "adxPreviewUnpublishedEntities";

		private readonly HttpContext _context;
		private readonly Entity _website;

		public PreviewPermission(OrganizationServiceContext context, Entity website)
		{
			if (website == null)
			{
				throw new ArgumentNullException("website");
			}

			_context = HttpContext.Current;
			_website = website;
			ServiceContext = context;
		}

		private HttpCookie Cookie
		{
			get { return _context.Request.Cookies[PreviewModeCookieName]; }
		}

		private bool HasCookie
		{
			get { return Cookie != null; }
		}

		public bool IsEnabled
		{
			get
			{
				// Enable preview mode by default, if the user doesn't have the cookie set yet, and has permission.
				if (!(HasCookie) && IsPermitted)
				{
					Enable();
				}

				bool cookieValue;

				return TryGetCookieValue(out cookieValue) && cookieValue;
			}
		}

		public bool IsEnabledAndPermitted
		{
			get { return IsEnabled && IsPermitted; }
		}

		public bool IsPermitted
		{
			get { return TryAssert(); }
		}

		public OrganizationServiceContext ServiceContext { get; private set; }

		public void Disable()
		{
			SetCookie("false");
		}

		public void Enable()
		{
			SetCookie("true");
		}

		private bool TryGetCookieValue(out bool cookieValue)
		{
			cookieValue = false;

			var cookie = Cookie;

			return cookie != null && bool.TryParse(cookie.Value, out cookieValue);
		}

		private void SetCookie(string cookieValue)
		{
			_context.Response.Cookies.Add(new HttpCookie(PreviewModeCookieName, cookieValue) { HttpOnly = false });
		}

		private bool TryAssert()
		{
			var cacheKeyFactory = new CrmEntitySecurityCacheInfoFactory(GetType().FullName);

			ICacheSupportingCrmEntitySecurityProvider provider = new ApplicationCachingCrmEntitySecurityProvider(new PreviewPermissionProvider(), cacheKeyFactory);

			if (_context != null)
			{
				provider = new RequestCachingCrmEntitySecurityProvider(provider, cacheKeyFactory);
			}

			return provider.TryAssert(ServiceContext, _website, CrmEntityRight.Read);
		}

		internal class PreviewPermissionProvider : CacheSupportingCrmEntitySecurityProvider
		{
			public override bool TryAssert(OrganizationServiceContext context, Entity website, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (website == null)
				{
					return false;
				}

				var securityProvider = new WebsiteAccessPermissionProvider(website, HttpContext.Current);
			
				return securityProvider.TryAssertRightProperty(context, "adx_previewunpublishedentities", dependencies);
			}
		}
	}
}
