/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Site.Pages;

namespace Site.Areas.Portal.Pages
{
	public partial class QuickForm : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Guid entityId;
			var entityIdValue = HttpContext.Current.Request["entityid"];
			var entityName = HttpContext.Current.Request["entityname"];
			var entityPrimaryKeyName = HttpContext.Current.Request["entityprimarykeyname"];
			var formName = HttpContext.Current.Request["formname"];
			var controlId = HttpUtility.HtmlEncode(HttpContext.Current.Request["controlid"]);
			int languageCode;
			
			if (!Guid.TryParse(entityIdValue, out entityId) || string.IsNullOrWhiteSpace(entityName) ||
				string.IsNullOrWhiteSpace(formName) || string.IsNullOrWhiteSpace(controlId)) return;

			if (string.IsNullOrWhiteSpace(entityPrimaryKeyName))
			{
				entityPrimaryKeyName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(ServiceContext, entityName);
			}

			if (string.IsNullOrWhiteSpace(entityPrimaryKeyName)) return;

			var fetch = new Fetch
			{
				MappingType = MappingType.Logical,
				Entity = new FetchEntity(entityName)
				{
					Attributes = FetchAttribute.All,
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new List<Condition> { new Condition(entityPrimaryKeyName, ConditionOperator.Equal, entityId) }
						}
					}
				}
			};

			var dataSource = new CrmDataSource
			{
				ID = string.Format("{0}_datasource", controlId),
				FetchXml = fetch.ToFetchExpression().Query,
				IsSingleSource = true
			};

			var formView = new CrmEntityFormView
			{
				ID = controlId,
				CssClass = "crmEntityFormView",
				DataSourceID = dataSource.ID,
				DataBindOnPostBack = true,
				EntityName = entityName,
				FormName = formName,
				Mode = FormViewMode.ReadOnly,
				ClientIDMode = ClientIDMode.Static,
				IsQuickForm = true,
			};

			var languageCodeSetting = HttpContext.Current.Request["languagecode"];

			if (!string.IsNullOrWhiteSpace(languageCodeSetting) && int.TryParse(languageCodeSetting, out languageCode))
			{
				var found = false;
				var portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				var portals = Microsoft.Xrm.Portal.Configuration.PortalCrmConfigurationManager.GetPortalCrmSection().Portals;

				formView.LanguageCode = languageCode;

				if (portals.Count > 0)
				{
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
						formView.ContextName = portalName;
						dataSource.CrmDataContextName = portalName;
					}
				}
			}
			
			FormPanel.Controls.Add(dataSource);
			FormPanel.Controls.Add(formView);
		}
	}
}
