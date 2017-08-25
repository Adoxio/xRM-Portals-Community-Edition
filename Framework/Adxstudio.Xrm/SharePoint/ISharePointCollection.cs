/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.SharePoint
{
	public interface ISharePointCollection
	{
		bool AccessDenied { get; }

		string PagingInfo { get; }

		int TotalCount { get; }

		IEnumerable<SharePointItem> SharePointItems { get; set; }
	}
}
