/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Collections.Generic
{
	internal class PostFilterPaginator<T>
	{
		private readonly Predicate<T> _filter;
		private readonly int _initialLimitMultiple;
		private readonly Func<int, int, T[]> _select;

		public PostFilterPaginator(Func<int, int, T[]> select, Predicate<T> filter, int initialLimitMultiple = 1)
		{
			if (select == null)
			{
				throw new ArgumentNullException("select");
			}

			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}

			if (initialLimitMultiple < 1)
			{
				throw new ArgumentException("Value can't be less than 1.", "initialLimitMultiple");
			}

			_select = select;
			_filter = filter;
			_initialLimitMultiple = initialLimitMultiple;
		} 

		public IEnumerable<T> Select(int offset, int limit)
		{
			var items = new List<T>();

			Select(0, (offset + limit) * _initialLimitMultiple, offset + limit, items);

			return items.Skip(offset).Take(limit).ToArray();
		}

		private void Select(int offset, int limit, int itemLimit, ICollection<T> items)
		{
			var selected = _select(offset, limit);

			foreach (var item in selected)
			{
				offset++;

				if (!_filter(item))
				{
					continue;
				}

				items.Add(item);

				if (items.Count >= itemLimit)
				{
					return;
				}
			}

			// If _select returned fewer items than were asked for, there must be no further items
			// to select, and so we should quit after processing the items we did get.
			if (selected.Length < limit)
			{
				return;
			}

			// For the next selection, set the limit to the median value between the original query
			// limit, and the number of remaining items needed.
			var reselectLimit = Convert.ToInt32(Math.Round((limit + (itemLimit - items.Count)) / 2.0, MidpointRounding.AwayFromZero));

			Select(offset, reselectLimit, itemLimit, items);
		}
	}
}
