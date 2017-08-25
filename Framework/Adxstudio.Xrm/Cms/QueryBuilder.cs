/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Services.Query;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// A component for producing queries for populating a content map.
	/// </summary>
	public abstract class QueryBuilder
	{
		/// <summary>
		/// Creates a query from an entity definition and a set of input parameters.
		/// </summary>
		/// <param name="ed"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public abstract Fetch CreateQuery(EntityDefinition ed, IDictionary<string, object> parameters);
	}

	/// <summary>
	/// Produces a query to return all entities of a specified entity type.
	/// </summary>
	public class RetrieveAllQueryBuilder : QueryBuilder
	{
		public override Fetch CreateQuery(EntityDefinition ed, IDictionary<string, object> parameters)
		{
			return ed.CreateFetchExpression();
		}
	}

	/// <summary>
	/// Produces a query to return entities that are filtered by a particular relationship or relationship chain.
	/// </summary>
	public class RetrieveByLinksQueryBuilder : QueryBuilder
	{
		public static RetrieveByLinksQueryBuilder Create(params Link[] links)
		{
			return new RetrieveByLinksQueryBuilder { Links = links };
		}

		/// <summary>
		/// The chain of relationships that relates the target entity type to the input parameters.
		/// </summary>
		public ICollection<Link> Links { get; set; }

		public override Fetch CreateQuery(EntityDefinition ed, IDictionary<string, object> parameters)
		{
			// the input links are appended to the query to filter the set of entities

			if (!parameters.ContainsKey("Links")) return null;

			var links = parameters["Links"] as ICollection<Link>;

			if (links == null) return null;

			var fetch = ed.CreateFetchExpression();

			if (Links != null && Links.Any())
			{
				// append the input links to the specified intermediate links

				var chains = Links.Clone();

				SetParameters(chains, links);

				fetch.Entity.Links = chains;
			}
			else
			{
				// assign the input links directly to the fetch

				fetch.Entity.Links = links;
			}

			// skip cache for ContentMap
			fetch.SkipCache = true;

			return fetch;
		}

		private void SetParameters(IEnumerable<Link> chains, ICollection<Link> parameterLinks)
		{
			foreach (var chain in chains)
			{
				if (chain.Links == null || !chain.Links.Any())
				{
					chain.Links = parameterLinks;
				}
				else
				{
					SetParameters(chain.Links, parameterLinks);
				}
			}
		}
	}
}
