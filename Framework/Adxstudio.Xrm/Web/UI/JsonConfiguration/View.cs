/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class View
	{
		public Guid ViewId { get; set; }

		public List<LanguageResources> DisplayName { get; set; }
	}
}
