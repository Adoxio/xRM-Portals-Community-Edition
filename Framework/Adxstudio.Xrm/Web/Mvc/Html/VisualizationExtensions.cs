/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	using System;
	using System.Web;
	using System.Web.Mvc;

	/// <summary>
	/// View helpers for rendering visualizations from CRM within Adxstudio Portals applications.
	/// </summary>
	public static class VisualizationExtensions
	{
		/// <summary>
		/// Renders an HTML structure for displaying a CRM chart.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceUrl">The URL to the service to retrieve visualization data.</param>
		/// <param name="chartId">Unique identifier of the CRM chart (savedqueryvisualization) record to be rendered.</param>
		/// <param name="viewId">An optional ID of a CRM view (savedquery) record that can be used to adjust the chart's query filters.</param>
		/// <returns>Returns HTML to render a CRM chart.</returns>
		public static IHtmlString CrmChart(this HtmlHelper html, string serviceUrl, Guid chartId, Guid? viewId = null)
		{
			var container = new TagBuilder("div");
			container.AddCssClass("crm-chart");
			container.MergeAttribute("data-serviceurl", serviceUrl);
			container.MergeAttribute("data-chartid", chartId.ToString());

			if (viewId != null && viewId != Guid.Empty)
			{
				container.MergeAttribute("data-viewid", viewId.ToString());
			}

			return new HtmlString(container.ToString());
		}
	}
}
