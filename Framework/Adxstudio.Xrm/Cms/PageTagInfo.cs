/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// A tagging information wrapper class for PageTag entities.
	/// </summary>
	public class PageTagInfo : TagInfo
	{
		public PageTagInfo(Entity crmEntity)
			: base("adx_name", "adx_pagetag_webpage", crmEntity)
		{
		}
	}
}
