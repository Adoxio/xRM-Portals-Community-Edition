/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Adxstudio.Xrm.Search
{
	public class ScopedQueryIndexSearcher : CrmEntityIndexSearcher
	{
		public ScopedQueryIndexSearcher(ICrmEntityIndex index) : base(index) { }

		protected override Query CreateQuery(ICrmEntityQuery query)
		{
			var scopedQuery = query as IScopedEntityQuery;

			if (scopedQuery == null)
			{
				return base.CreateQuery(query);
			}

			var scopeQuery = new BooleanQuery();

			foreach (var scope in scopedQuery.Scopes)
			{
				if (scope == null)
				{
					continue;
				}

				scopeQuery.Add(new TermQuery(new Term(Index.ScopeFieldName, scope)), Occur.SHOULD);
			}

			scopeQuery.Add(new TermQuery(new Term(Index.ScopeFieldName, Index.ScopeDefaultValue)), Occur.SHOULD);

			var baseQuery = base.CreateQuery(scopedQuery);

			var compositeQuery = new BooleanQuery
			{
				{ baseQuery, Occur.MUST },
				{ scopeQuery, Occur.MUST }
			};

			return compositeQuery;
		}
	}
}
