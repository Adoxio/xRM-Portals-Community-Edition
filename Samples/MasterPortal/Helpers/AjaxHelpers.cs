/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Site.Helpers
{
	public static class AjaxHelpers
	{
		public static IHtmlString RawActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
		{
			var replaceId = Guid.NewGuid().ToString();
			
			var actionLink = ajaxHelper.ActionLink(replaceId, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
			
			return new HtmlString(actionLink.ToString().Replace(replaceId, linkText));
		}
	}
}
