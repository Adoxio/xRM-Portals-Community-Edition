/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Link for associate action
	/// </summary>
	public class AssociateActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-link' aria-hidden='true'></span>" + ResourceManager.GetString("Associate_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Associate_Button_Text");

		/// <summary>
		/// The relationship for the associate request
		/// </summary>
		public Relationship Relationship { get; set; }

		/// <summary>
		/// Settings needed to be able to retrieve a view and configure its display
		/// </summary>
		public IEnumerable<ViewConfiguration> ViewConfigurations { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public AssociateActionLink()
		{
		}

		/// <summary>
		/// Constructor used by ViewConfiguration class
		/// </summary>
		public AssociateActionLink(IEnumerable<ViewConfiguration> viewConfigurations, Relationship relationship,
			IPortalContext portalContext, AssociateAction action, int languageCode, bool enabled = false, UrlBuilder url = null,
			string portalName = null, string label = null, string tooltip = null)
			: base(portalContext, languageCode, action, LinkActionType.Associate, enabled, url, portalName, label, tooltip)
		{
			Relationship = relationship;

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("Associate", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });

			ViewConfigurations = viewConfigurations ?? Enumerable.Empty<ViewConfiguration>();
		}

		protected override string GetDefaultButtonLabel()
		{
			return DefaultButtonLabel;
		}

		protected override string GetDefaultButtonTooltip()
		{
			return DefaultButtonTooltip;
		}
	}
}
