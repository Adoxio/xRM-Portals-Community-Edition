/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class PreviousActionLink : ViewActionLink
	{
		private static string DefaultButtonLabel = ResourceManager.GetString("Previous_Button_Label");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Previous_Button_Label");

		public PreviousActionLink()
		{
			Enabled = false;
		}

		public PreviousActionLink(IPortalContext portalContext, int languageCode, PreviousAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Previous, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
		}
	}
}
