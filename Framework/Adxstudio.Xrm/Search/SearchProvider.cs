/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Configuration.Provider;
using Adxstudio.Xrm.Search.Index;

namespace Adxstudio.Xrm.Search
{
	public abstract class SearchProvider : ProviderBase
	{
		public abstract ICrmEntityIndexBuilder GetIndexBuilder();

		public abstract ICrmEntityIndexSearcher GetIndexSearcher();

		public abstract ICrmEntityIndexUpdater GetIndexUpdater();

		public abstract IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo();

		public abstract IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode);

		public virtual IRawLuceneIndexSearcher GetRawLuceneIndexSearcher() { return null; }
    }
}
