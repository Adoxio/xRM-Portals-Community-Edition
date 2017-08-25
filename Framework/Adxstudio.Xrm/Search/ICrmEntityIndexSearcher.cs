/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search
{
	public interface ICrmEntityIndexSearcher : IDisposable
	{
		ICrmEntitySearchResultPage Search(ICrmEntityQuery query);
	}
}
