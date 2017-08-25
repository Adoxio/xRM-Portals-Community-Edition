/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRawLuceneIndexSearcher.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Adxstudio.Xrm.Search
{
	using System.Collections.Generic;
	using Lucene.Net.Search;
	using Microsoft.Xrm.Sdk;


	/// <summary>
	/// Interface for RawLuceneIndexSearcher
	/// </summary>
	////public interface IRawLuceneIndexSearcher
	public interface IRawLuceneIndexSearcher
	{
		/// <summary>
		/// Searching in Index directory
		/// </summary>
		/// <param name="query">
		/// The query
		/// </param>
		/// <param name="maxSearchResults">
		/// Max number of return results
		/// </param>
		/// <returns>
		/// List of EntityReferences (Index documents)
		/// </returns>
		IEnumerable<EntityReference> Search(Query query, int maxSearchResults);
	}
}
