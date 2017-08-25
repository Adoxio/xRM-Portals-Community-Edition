/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Dropdown populated with the installed and provisioned Language Packs in CRM.
	/// Value is stored as an integer language code.
	/// </summary>
	[ToolboxData("<{0}:CrmLanguage runat=server></{0}:CrmLanguage>")]
	public class CrmLanguage : DropDownList
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

		override protected void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Items.Count > 0)
			{
				return;
			}
			
			var empty = new ListItem(string.Empty, string.Empty);
			empty.Attributes["label"] = " ";
			Items.Add(empty);

			var context = CrmConfigurationManager.CreateContext(ContextName);
			var request = new RetrieveProvisionedLanguagesRequest();
			var response = (RetrieveProvisionedLanguagesResponse)context.Execute(request);
			var currentCultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;

			if (LanguageCode > 0)
			{
				// Set the culture for the specified Language Code so the list of languages is displayed in the appropriate language.
				var tempCultureInfo = new CultureInfo(LanguageCode);
				System.Threading.Thread.CurrentThread.CurrentCulture = tempCultureInfo;
				System.Threading.Thread.CurrentThread.CurrentUICulture = tempCultureInfo;
			}
			foreach (var lcid in response.RetrieveProvisionedLanguages)
			{
				var culture = CultureInfo.GetCultureInfo(lcid);
				Items.Add(new ListItem(culture.DisplayName, culture.LCID.ToString(CultureInfo.InvariantCulture)));
			}
			if (LanguageCode > 0)
			{
				// Reset the culture back to the original culture.
				System.Threading.Thread.CurrentThread.CurrentCulture = currentCultureInfo;
				System.Threading.Thread.CurrentThread.CurrentUICulture = currentCultureInfo;
			}
			
		}
	}
}
