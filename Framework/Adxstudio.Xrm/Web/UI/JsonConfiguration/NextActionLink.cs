/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class NextActionLink : ViewActionLink
	{
		private static string DefaultButtonLabel = ResourceManager.GetString("Next_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Next_Button_Text");

		public NextActionLink()
		{
			Enabled = false;
		}

		public NextActionLink(IPortalContext portalContext, int languageCode, NextAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Next, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
		}
	}
}
