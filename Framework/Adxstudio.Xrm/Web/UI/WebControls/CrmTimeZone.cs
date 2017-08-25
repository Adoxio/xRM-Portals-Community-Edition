/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Dropdown populated with Time Zones provided from 'timezonedefinition' records in CRM.
	/// Value is stored as an integer time zone code.
	/// </summary>
	[ToolboxData("<{0}:CrmTimeZone runat=server></{0}:CrmTimeZone>")]
	public class CrmTimeZone : DropDownList
	{
		/// <summary>
		/// The name used to retrieve the configured Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
		/// </summary>
		public string ContextName
		{
			get { return ViewState["ContextName"] as string; }
			set { ViewState["ContextName"] = value; }
		}

		/// <summary>
		/// Language Code
		/// </summary>
		public int LanguageCode
		{
			get { return (int)(ViewState["LanguageCode"] ?? 0); }
			set { ViewState["LanguageCode"] = value; }
		}

		/// <summary>
		/// Default Selected Value
		/// </summary>
		public string DefaultSelectedValue
		{
			get { return ViewState["DefaultSelectedValue"] as string; }
			set { ViewState["DefaultSelectedValue"] = value; }
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (Items.Count > 0)
			{
				return;
			}
			
			var empty = new ListItem(string.Empty, string.Empty);
			empty.Attributes["label"] = " ";
			Items.Add(empty);

			var context = CrmConfigurationManager.CreateContext(ContextName);

			if (LanguageCode == 0)
			{
				var organization = context.CreateQuery("organization").FirstOrDefault();

				if (organization == null)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to retrieve organization.");
                }
				else
				{
					LanguageCode = organization.GetAttributeValue<int?>("languagecode") ?? 0;
				}
			}

			if (LanguageCode == 0)
			{
				LanguageCode = 1033;
			}
			
			var request = new GetAllTimeZonesWithDisplayNameRequest { LocaleId = LanguageCode };
			
			var response = (GetAllTimeZonesWithDisplayNameResponse)context.Execute(request);

			foreach (var timeZoneDefinition in response.EntityCollection.Entities.OrderBy(o => o.GetAttributeValue<string>("userinterfacename")))
			{
				Items.Add(new ListItem(timeZoneDefinition.GetAttributeValue<string>("userinterfacename"), timeZoneDefinition.GetAttributeValue<int>("timezonecode").ToString(CultureInfo.InvariantCulture)));
			}


		}
	}
}
