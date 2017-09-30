/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Adxstudio.Xrm.Search
{
	public class ScopedIndexSearcher : CrmEntityIndexSearcher
	{
		private readonly IEnumerable<string> _scopes;

		public ScopedIndexSearcher(ICrmEntityIndex index, IEnumerable<string> scopes) : base(index)
		{
			if (scopes == null)
			{
				throw new ArgumentNullException("scopes");
			}

			_scopes = scopes;
		}

		public ScopedIndexSearcher(ICrmEntityIndex index, params string[] scopes) : this(index, scopes as IEnumerable<string>) { }

		protected override Query CreateQuery(ICrmEntityQuery query)
		{
			var scopeQuery = new BooleanQuery();

			foreach (var scope in _scopes)
			{
				if (scope == null)
				{
					continue;
				}

				scopeQuery.Add(new TermQuery(new Term(Index.ScopeFieldName, scope)), Occur.SHOULD);
			}
			if (_scopes.All(x => x != Index.ScopeDefaultValue))
			{
				scopeQuery.Add(new TermQuery(new Term(Index.ScopeFieldName, Index.ScopeDefaultValue)), Occur.SHOULD);
			}
			

			var baseQuery = base.CreateQuery(query);

			var compositeQuery = new BooleanQuery
			{
				{ baseQuery, Occur.MUST },
				{ scopeQuery, Occur.MUST }
			};

			return compositeQuery;
		}
	}
}
