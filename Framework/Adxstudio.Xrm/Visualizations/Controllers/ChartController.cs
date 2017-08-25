/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations.Controllers
{
	using System;
	using System.Globalization;
	using System.Web.Mvc;

	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Portal.Configuration;

	/// <summary>
	/// MVC Controller for client side requests for CRM chart information.
	/// </summary>
	public class ChartController : Controller
	{
		/// <summary>
		/// Gets a serialized <see cref="CrmChartBuilder"/> object that defines the necessary information to aid in rendering a CRM chart.
		/// </summary>
		/// <param name="chartId">The ID of the CRM chart (savedqueryvisualization) record to get the definition of.</param>
		/// <param name="viewId">An optional ID of a CRM view (savedquery) record that can be used to adjust the query filters.</param>
		/// <returns>A serialized <see cref="CrmChartBuilder"/> object that defines the necessary information along with the data record collection to aid in rendering a CRM chart.</returns>
		[HttpGet]
		[JsonHandlerError]
		public ActionResult GetChartBuilder(Guid chartId, Guid? viewId)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var crmChartBuilder = new CrmChartBuilder(context, chartId, this.HttpContext.GetContextLanguageInfo(), viewId, languageCode: CultureInfo.CurrentCulture.LCID);

			return this.Json(crmChartBuilder, JsonRequestBehavior.AllowGet);
		}
	}
}
