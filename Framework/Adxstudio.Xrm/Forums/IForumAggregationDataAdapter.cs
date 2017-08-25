/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumAggregationDataAdapter
	{
		IEnumerable<IForum> SelectForums();

		IForum Select(Guid forumId);

		IForum Select(string forumName);
	}

	
}
