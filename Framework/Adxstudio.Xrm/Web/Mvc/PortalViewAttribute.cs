/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// <see cref="ActionFilterAttribute"/> to add a default instance of <see cref="IPortalViewContext"/> to view data,
	/// to support portal view helpers.
	/// </summary>
	public class PortalViewAttribute : ActionFilterAttribute
	{
		public string PortalName { get; private set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			filterContext.Controller.ViewData[PortalExtensions.PortalViewContextKey] = new PortalViewContext(
				new PortalConfigurationDataAdapterDependencies(PortalName, filterContext.RequestContext));
		}
	}
}
