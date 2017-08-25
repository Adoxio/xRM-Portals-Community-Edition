/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Portal.Controllers
{
	using System.Web.Mvc;
	using System.Web.UI;
	using Adxstudio.Xrm.AspNet.Cms;

	public class ResourcesController : Controller
	{

		[HttpGet]
		[OutputCache(Duration = 86400, VaryByParam = "lang", Location = OutputCacheLocation.ServerAndClient)]
		public ActionResult ResourceManager()
		{
			var languageFormat = "lang={0}";
			string fallbackCode;
			string code = this.HttpContext.Request.QueryString["lang"];
			if (!string.IsNullOrWhiteSpace(code) && ContextLanguageInfo.ResolveCultureCode(code, out fallbackCode))
			{
				var current = this.Request.QueryString["lang"];
				var url = this.Request.Url.AbsoluteUri.Replace(string.Format(languageFormat, current), string.Format(languageFormat, fallbackCode));
				return this.Redirect(url);
			}

			Response.Cache.SetOmitVaryStar(true);
			Response.ContentType = "text/javascript";
			return View("ResourceManager");
		}
	}
}
