/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public interface IFormConfiguration
	{
		/// <summary>
		/// Logical name of the entity associated with the view.
		/// </summary>
		string EntityName { get; set; }
		/// <summary>
		/// Logical name of the primary key attribute of the entity associated with the view.
		/// </summary>
		string PrimaryKeyName { get; set; }
		/// <summary>
		/// Unique identifier of the configuration.
		/// </summary>
		Guid Id { get; set; }
		/// <summary>
		/// Link for Delete Action
		/// </summary>
		DeleteActionLink DeleteActionLink { get; set; }
		/// <summary>
		/// Actions that are applicable to a single record item.
		/// </summary>
		List<ViewActionLink> TopFormActionLinks { get; set; }
		/// <summary>
		/// Actions that are applicable to a single record item.
		/// </summary>
		List<ViewActionLink> BottomFormActionLinks { get; set; }
		/// <summary>
		/// Indicates whether entity permission rules should be applied to the query.
		/// </summary>
		bool EnableEntityPermissions { get; set; }
		/// <summary>
		/// Gets or sets the language code
		/// </summary>
		int LanguageCode { get; set; }
		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		string PortalName { get; set; }

		ActionButtonStyle? ActionButtonStyle { get; set; }

		ActionButtonPlacement? ActionButtonPlacement { get; set; }

		ActionButtonAlignment? ActionButtonAlignment { get; set; }

		ShowActionButtonContainer? ShowActionButtonContainer { get; set; }

		string ActionButtonDropDownLabel { set; get; }

		string ActionNavbarCssClass { set; get; }

	}

}
