/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Mvc;
using Site.Helpers;
using DevTrends.MvcDonutCaching;

namespace Site.Areas.Portal.Controllers
{
	public class LayoutController : Controller
	{
		/// <summary>
		/// Renders the current request URL with language code prepended.
		/// </summary>
		/// <param name="languageCode">Language code to prepend.</param>
		/// <returns>Current request URL with language code prepended.</returns>
		[ChildActionOnly, PortalView]
		public ActionResult ContextUrlWithLanguage(string languageCode)
		{
			var info = HttpContext.GetContextLanguageInfo();
			var url = info.FormatUrlWithLanguage(true, languageCode);
			return Content(url);
		}

		/// <summary>
		/// Renders the header. This action is donut-cached.
		/// </summary>
		/// <returns></returns>
		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "UserShared", Options = OutputCacheOptions.ReplaceDonutsInChildActions)]
		public ActionResult Header()
		{
			ViewBag.ViewSupportsDonuts = true;
			return PartialView("Header");
		}

		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "Roles")]
		public ActionResult HeaderChildNavbar()
		{
			return PartialView("HeaderChildNavbar");
		}

		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "RolesShared")]
		public ActionResult HeaderPrimaryNavigation()
		{
			return PartialView("HeaderPrimaryNavigation");
		}

		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "RolesShared")]
		public ActionResult HeaderPrimaryNavigationTabs()
		{
			return PartialView("HeaderPrimaryNavigationTabs");
		}

		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "RolesShared")]
		public ActionResult HeaderPrimaryNavigationXs()
		{
			return PartialView("HeaderPrimaryNavigationXs");
		}

		/// <summary>
		/// Renders the footer. This action is donut-cached.
		/// </summary>
		/// <returns></returns>
		[ChildActionOnly, PortalView, DonutOutputCache(CacheProfile = "UserShared", Options = OutputCacheOptions.ReplaceDonutsInChildActions)]
		public ActionResult Footer()
		{
			ViewBag.ViewSupportsDonuts = true;
			return PartialView("Footer");
		}

		/// <summary>
		/// Renders liquid source code for donut-hole substitution purposes.
		/// </summary>
		/// <param name="encodedSource"></param>
		/// <returns></returns>
		[ChildActionOnly, PortalView]
		public ActionResult LiquidSubstitution(string encodedSource)
		{
			ViewBag.LiquidSource = Encoding.UTF8.GetString(Convert.FromBase64String(encodedSource ?? string.Empty));
			return PartialView("LiquidSubstitution");
		}

		[ChildActionOnly, PortalView]
		public ActionResult RegisterUrl()
		{
			return Content(Url.SecureRegistrationUrl());
		}
		
		[ChildActionOnly, PortalView]
		public ActionResult SignInLink()
		{
			return PartialView("SignInLink");
		}

		[ChildActionOnly, PortalView]
		public ActionResult SignInUrl()
		{
			return Content(Url.SignInUrl());
		}

		[ChildActionOnly, PortalView]
		public ActionResult SignOutUrl()
		{
			return Content(Url.SignOutUrl());
		}

		/// <summary>
		/// Generates the CSRF form token.
		/// </summary>
		/// <returns>the form token which is encapsulated as a hidden field.</returns>
		[HttpGet]
		public HtmlString GetAntiForgeryToken()
		{
			try
			{
				return AntiForgery.GetHtml();
			}
			catch (Exception exception)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to generate csrf token: {0}", exception.ToString()));

				throw new SecurityException("Failed to generate csrf token for validation");
			}
		}
	}
}
