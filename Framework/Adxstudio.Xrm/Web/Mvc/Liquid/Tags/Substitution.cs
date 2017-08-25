/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid.Tags
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web.Mvc;
	using DevTrends.MvcDonutCaching;
	using DotLiquid;
	using DotLiquid.Tags;
	using Adxstudio.Xrm.Web.Mvc.Html;

	/// <summary>
	/// Renders the donut cache substitution placeholder for some arbitrary Liquid source contained within
	/// the block, if the parent view supports donut caching. If not, just renders the source immediately.
	/// </summary>
	public class Substitution : Raw
	{
		/// <summary>
		/// Renders the donut cache substitution.
		/// </summary>
		/// <param name="context">The Liquid rendering context.</param>
		/// <param name="result">Writer for rendered output.</param>
		public override void Render(Context context, TextWriter result)
		{
			var html = context.Registers["htmlHelper"] as HtmlHelper;

			if (html == null)
			{
				return;
			}

			var source = NodeList.Cast<string>().Aggregate(new StringBuilder(), (sb, token) => sb.Append(token));
			var viewSupportsDonuts = context.ViewSupportsDonuts();

			context.Stack(() =>
			{
				// If donuts are supported then call the LiquidSubstitution action, otherwise render the liquid code directly.
				if (viewSupportsDonuts)
				{
					var encodedSource = Convert.ToBase64String(Encoding.UTF8.GetBytes(source.ToString()));
					result.Write(html.Action("LiquidSubstitution", "Layout", new { encodedSource }, viewSupportsDonuts));
				}
				else
				{
					html.RenderLiquid(source.ToString(), "Substitution string, but donuts not supported", result);
				}
			});
		}
	}
}
