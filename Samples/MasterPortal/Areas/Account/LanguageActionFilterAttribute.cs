/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Account
{
	using System;
	using System.Web.Mvc;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Web;

	/// <summary>
	/// Redirects to the language specific URL.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class LanguageActionFilterAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Filter on action executing.
		/// </summary>
		/// <param name="filterContext">The context.</param>
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// format url with/without language code based on site setting
			var contextLanguageInfo = filterContext.HttpContext.GetContextLanguageInfo();

			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				if (contextLanguageInfo.RequestUrlHasLanguageCode != ContextLanguageInfo.DisplayLanguageCodeInUrl || contextLanguageInfo.ContextLanguage.UsedAsFallback)
				{
					var url = ContextLanguageInfo.DisplayLanguageCodeInUrl ? contextLanguageInfo.FormatUrlWithLanguage() : contextLanguageInfo.AbsolutePathWithoutLanguageCode;
					filterContext.Result = new RedirectResult(url);

					return;
				}
			}

			base.OnActionExecuting(filterContext);
		}
	}
}
