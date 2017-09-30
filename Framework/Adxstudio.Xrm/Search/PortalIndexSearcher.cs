/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search
{
	public class PortalIndexSearcher : ScopedIndexSearcher
	{
		public PortalIndexSearcher(ICrmEntityIndex index, Guid websiteID) : base(index, websiteID.ToString()) { }
	}
}
