/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search
{
	public class SingleUseIndexSearcherPool : IIndexSearcherPool
	{
		public ICrmEntityIndexSearcher Get(string name, Func<ICrmEntityIndexSearcher> searcherFactory)
		{
			return searcherFactory();
		}

		public void Refresh(string name) { }
	}
}
