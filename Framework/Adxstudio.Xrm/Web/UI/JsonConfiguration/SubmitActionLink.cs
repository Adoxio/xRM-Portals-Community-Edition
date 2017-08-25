/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class SubmitActionLink : ViewActionLink
	{
		private static string DefaultButtonLabel = ResourceManager.GetString("Submit_Button_Label_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Submit_Button_Label_Text");
		private static readonly string DefaultButtonBusyText = ResourceManager.GetString("Default_Modal_Processing_Text");

		public SubmitActionLink()
		{
			Enabled = false;
		}

		public SubmitActionLink(IPortalContext portalContext, int languageCode, SubmitAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Submit, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip, DefaultButtonBusyText)
		{
		}
	}
}
