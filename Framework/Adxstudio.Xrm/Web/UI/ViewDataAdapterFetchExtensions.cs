/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI
{
	internal static class ViewDataAdapterFetchExtensions
	{
		public static void AddAttributes(this FetchEntity fetchEntity, params string[] attributes)
		{
			foreach (var attribute in attributes)
			{
				if (fetchEntity.Attributes.Any(a => string.Equals(a.Name, attribute, StringComparison.InvariantCulture)))
				{
					continue;
				}

				fetchEntity.Attributes.Add(new FetchAttribute(attribute));
			}
		}

		public static void AddFilter(this Fetch fetch, Filter filter)
		{
			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>
				{
					new Filter { Type = LogicalOperator.And, Filters = new List<Filter> { filter } }
				};
			}
			else
			{
				var rootAndFilter = fetch.Entity.Filters.FirstOrDefault(f => f.Type == LogicalOperator.And);

				if (rootAndFilter != null)
				{
					if (filter.Conditions != null && filter.Conditions.Any())
					{
						if (rootAndFilter.Conditions == null)
						{
							rootAndFilter.Conditions = filter.Conditions;
						}
						else
						{
							foreach (var condition in filter.Conditions)
							{
								rootAndFilter.Conditions.Add(condition);
							}
						}
					}

					if (filter.Filters != null && filter.Filters.Any())
					{
						if (rootAndFilter.Filters == null)
						{
							rootAndFilter.Filters = filter.Filters;
						}
						else
						{
							foreach (var f in filter.Filters)
							{
								rootAndFilter.Filters.Add(f);
							}
						}
					}
				}
				else
				{
					fetch.Entity.Filters.Add(filter);
				}
			}
		}

		public static void AddLink(this Fetch fetch, Link link)
		{
			if (fetch.Entity.Links == null)
			{
				fetch.Entity.Links = new List<Link> { link };
			}
			else
			{
				fetch.Entity.Links.Add(link);
			}
		}
	}
}
