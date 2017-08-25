/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// <see cref="ActionFilterAttribute"/> to enforce portal site map provider security rules on MVC
	/// actions.
	/// </summary>
	/// <remarks>
	/// If the current user does not have access permissions to the current site map node for
	/// the action (which will be the site marker node, in the case that the action has been routed by
	/// a site marker route), the action will return a 403 Forbidden response. It's then expected that
	/// further handling at the end of the request will redirect the user to the appropriate login
	/// page, or something similar.
	/// </remarks>
	public class PortalSecurityAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!SiteMap.Enabled)
			{
				return;
			}

			var currentNode = SiteMap.CurrentNode as CrmSiteMapNode;

			if (currentNode == null)
			{
				return;
			}

			if (currentNode.StatusCode == HttpStatusCode.Forbidden)
			{
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}
		}
	}
}
