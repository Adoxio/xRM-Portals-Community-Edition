/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Web;
using Adxstudio.Xrm.Web.UI.EntityForm;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Site.Areas.Portal.Pages
{
	public partial class Form : PortalPage
	{
		protected bool IsLookupCreateForm { get; private set; }

		protected void Page_Init(object sender, EventArgs e)
		{
			IsLookupCreateForm = !string.IsNullOrEmpty(Request.QueryString["lookup"]);

			var languageCodeSetting = HttpContext.Current.Request["languagecode"];

			if (string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				return;
			}

			int languageCode;
			
			if (!int.TryParse(languageCodeSetting, out languageCode))
			{
				return;
			}

			EntityFormControl.LanguageCode = languageCode;

			var portalName = languageCode.ToString(CultureInfo.InvariantCulture);

			var portals = PortalCrmConfigurationManager.GetPortalCrmSection().Portals;
			
			if (portals.Count <= 0) return;

			var found = false;
			
			foreach (var portal in portals)
			{
				var portalContext = portal as PortalContextElement;
				if (portalContext != null && portalContext.Name == portalName)
				{
					found = true;
				}
			}

			if (found)
			{
				EntityFormControl.PortalName = portalName;
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			Guid entityFormId;
			var entityFormIdValue = HttpContext.Current.Request["entityformid"];

			if (!Guid.TryParse(entityFormIdValue, out entityFormId))
			{
				return;
			}

			EntityFormControl.EntityFormReference = new EntityReference("adx_entityform", entityFormId);

			if (string.IsNullOrEmpty(Page.Form.Action))
			{
				Page.Form.Action = Request.Url.PathAndQuery;
			}
		}

		protected void OnItemSaved(object sender, EntityFormSavedEventArgs e)
		{
			if (e == null)
			{
				return;
			}
			
			if (e.Exception == null)
			{
				var cs = Page.ClientScript;
				
				if (!cs.IsClientScriptBlockRegistered(GetType(), "EntityFormOnSuccessScript"))
				{
					cs.RegisterClientScriptBlock(
						GetType(),
						"EntityFormOnSuccessScript",
						string.Format(
							CultureInfo.InvariantCulture,
							@"window.parent.postMessage(""{0}"",""*"");",
							HttpUtility.JavaScriptStringEncode(GetSaveSuccessMessage(e))),
						true);
				}
			}
		}

		private string GetSaveSuccessMessage(EntityFormSavedEventArgs e)
		{
			if (!IsLookupCreateForm)
			{
				return "Success";
			}

			return new JObject
			{
				{ "type", "Success" },
				{ "name", e.EntityDisplayName },
				{ "id", e.EntityId.HasValue ? e.EntityId.Value.ToString() : null }
			}.ToString(Formatting.None);
		}
	}
}
