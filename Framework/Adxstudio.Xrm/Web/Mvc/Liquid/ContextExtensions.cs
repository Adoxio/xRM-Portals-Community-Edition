/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	internal static class ContextExtensions
	{
		public static bool TryGetPortalLiquidContext(this Context context, out IPortalLiquidContext portalLiquidContext)
		{
			portalLiquidContext = null;

			object register;

			if (!context.Registers.TryGetValue("portalLiquidContext", out register))
			{
				return false;
			}

			portalLiquidContext = register as IPortalLiquidContext;

			return portalLiquidContext != null;
		}

		/// <summary>
		/// Gets whether the current MVC view supports donuts.
		/// This value would have been set in the controller action and saved to the ViewBag.
		/// </summary>
		/// <param name="context">Liquid context.</param>
		/// <returns>Whether the current MVC view supports donuts.</returns>
		public static bool ViewSupportsDonuts(this Context context)
		{
			var html = context.Registers["htmlHelper"] as HtmlHelper;
			return html != null && ((bool?)html.ViewBag.ViewSupportsDonuts).GetValueOrDefault(false);
		}
	}
}
