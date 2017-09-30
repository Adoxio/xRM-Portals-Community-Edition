/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Json.JsonConverter;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Newtonsoft.Json;
using Site.Areas.Portal.Controllers;

namespace Site.Areas.CaseManagement.Controllers
{
	public class EntitlementsController : Controller
	{
		// POST: CaseManagement/Entitlements
		[HttpPost]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GetDefaultEntitlements(string layout, string sortExpression, IDictionary<string, string> customParameters)
		{
			EntityGridController egc = new EntityGridController();
			var result = GetData(layout, sortExpression, customParameters);

			if (result.Records.Any())
			{
				var entitlements = result.Records.Where(x => (bool)x.Attributes["isdefault"]).ToArray();
				if (entitlements.Any())
				{
					var defaultEntitlement = entitlements[0];
					return Json(new { id = defaultEntitlement.Id, name = defaultEntitlement.Attributes["name"], entityname = defaultEntitlement.LogicalName });
				}
			}
			return Json(null);
		}

		private ViewDataAdapter.FetchResult GetData(string base64SecureConfiguration, string sortExpression, IDictionary<string, string> customParameters)
		{
			var viewConfiguration = ConvertSecureStringToViewConfiguration(base64SecureConfiguration);
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: viewConfiguration.PortalName);

			var viewDataAdapter = new ViewDataAdapter(viewConfiguration, dataAdapterDependencies, 1, null, sortExpression, null, null, customParameters: customParameters);

			return viewDataAdapter.FetchEntities();
		}

		private ViewConfiguration ConvertSecureStringToViewConfiguration(string base64SecureConfiguration)
		{
			var secureConfigurationByteArray = Convert.FromBase64String(base64SecureConfiguration);
			var unprotectedByteArray = MachineKey.Unprotect(secureConfigurationByteArray, "Secure View Configuration");
			if (unprotectedByteArray == null)
			{
				return null;
			}
			var configurationJson = Encoding.UTF8.GetString(unprotectedByteArray);
			var viewConfiguration = JsonConvert.DeserializeObject<ViewConfiguration>(configurationJson, new JsonSerializerSettings { ContractResolver = JsonConfigurationContractResolver.Instance, Converters = new List<JsonConverter> { new UrlBuilderConverter() } });
			return viewConfiguration;
		}
	}
}
