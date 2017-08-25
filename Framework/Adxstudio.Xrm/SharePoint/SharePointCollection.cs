/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.SharePoint
{
	public class SharePointCollection : ISharePointCollection
	{
		private SharePointCollection(bool accessDenied)
		{
			SharePointItems = Enumerable.Empty<SharePointItem>();
			AccessDenied = accessDenied;
			PagingInfo = null;
			TotalCount = 0;
		}

		public SharePointCollection(IEnumerable<SharePointItem> sharePointItems, string pagingInfo, int totalCount)
		{
			SharePointItems = sharePointItems;
			AccessDenied = false;
			PagingInfo = pagingInfo;
			TotalCount = totalCount;
		}

		public bool AccessDenied { get; private set; }

		public string PagingInfo { get; private set; }
		
		public int TotalCount { get; private set; }

		public IEnumerable<SharePointItem> SharePointItems { get; set; }

		public static ISharePointCollection Empty(bool accessDenied)
		{
			return new SharePointCollection(accessDenied);
		}
	}
}
