/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web.Data.Services;

namespace Adxstudio.Xrm.Web.Data.Services
{
	public class ExtendedSiteMapChildInfo : SiteMapChildInfo
	{
		public bool HiddenFromSiteMap { get; set; }

		public Guid? Id { get; set; }

		public string LogicalName { get; set; }

		public string Url { get; set; }
	}
}
