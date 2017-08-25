/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class GridOptions
	{
		public string CssClass { get; set; }

		public string GridCssClass { get; set; }

		public List<LanguageResources> LoadingMessage { get; set; }

		public List<LanguageResources> ErrorMessage { get; set; }

		public List<LanguageResources> AccessDeniedMessage { get; set; }

		public List<LanguageResources> EmptyMessage { get; set; }
	}
}
