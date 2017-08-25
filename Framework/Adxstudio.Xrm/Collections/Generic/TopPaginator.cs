/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Provides correct pagination of a sequence that must be filtered post-retrieval (i.e., in order to get correct
	/// pages, the sequence must be filtered from the start each time)
	/// </summary>
	/// <typeparam name="T">The type of items contained by the sequence being paginated.</typeparam>
	internal class TopPaginator<T>
	{
		private readonly int _extendedSearchLimitMultiple;
		private readonly Func<int, Top> _getTop;
		private readonly int _initialSearchLimitMultiple;
		private readonly int _pageSize;
		private readonly Predicate<T> _selector;

		public TopPaginator(int pageSize, Func<int, Top> getTop, Predicate<T> selector = null, int initialSearchLimitMultiple = 1, int extendedSearchLimitMultiple = 2)
		{
			if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize", ResourceManager.GetString("Must_Be_GreaterThan_Or_EqualTo_One_Exception"));
			if (getTop == null) throw new ArgumentNullException("getTop");
			if (initialSearchLimitMultiple < 1) throw new ArgumentOutOfRangeException("initialSearchLimitMultiple", ResourceManager.GetString("Must_Be_GreaterThan_Or_EqualTo_One_Exception"));
			if (extendedSearchLimitMultiple < 2) throw new ArgumentOutOfRangeException("extendedSearchLimitMultiple", ResourceManager.GetString("Must_Be_GreaterThan_One_Exception"));

			_pageSize = pageSize;
			_getTop = getTop;
			_selector = selector ?? (item => true);
			_initialSearchLimitMultiple = initialSearchLimitMultiple;
			_extendedSearchLimitMultiple = extendedSearchLimitMultiple;
		}

		public Page GetPage(int pageNumber)
		{
			var items = new List<T>();

			var totalUnfilteredItems = GetItems(
				((_pageSize * pageNumber) * _initialSearchLimitMultiple),
				0,
				(_pageSize * pageNumber),
				items);

			var itemOffset = (pageNumber - 1) * _pageSize;

			var pageItems = items.Skip(itemOffset).Take(_pageSize).ToList();

			return new Page(pageItems, totalUnfilteredItems, pageNumber, _pageSize);
		}

		private int GetItems(int topLimit, int unfilteredItemOffset, int itemLimit, ICollection<T> items)
		{
			var top = _getTop(topLimit);

			if (unfilteredItemOffset >= top.TotalUnfilteredItems)
			{
				return top.TotalUnfilteredItems;
			}

			foreach (var item in top.Skip(unfilteredItemOffset))
			{
				unfilteredItemOffset++;

				if (!_selector(item))
				{
					continue;
				}

				items.Add(item);

				if (items.Count >= itemLimit)
				{
					return top.TotalUnfilteredItems;
				}
			}

			if (topLimit >= top.TotalUnfilteredItems)
			{
				return items.Count;
			}

			return GetItems(topLimit * _extendedSearchLimitMultiple, unfilteredItemOffset, itemLimit, items);
		}

		public class Page : IEnumerable<T>
		{
			private readonly IEnumerable<T> _items;

			public Page(IEnumerable<T> items, int totalUnfilteredItems, int pageNumber, int pageSize)
			{
				if (items == null)
				{
					throw new ArgumentNullException("items");
				}

				_items = items.ToList();
				TotalUnfilteredItems = totalUnfilteredItems;
				PageNumber = pageNumber;
				PageSize = pageSize;
			}

			public int PageNumber { get; private set; }

			public int PageSize { get; private set; }

			public int TotalUnfilteredItems { get; private set; }

			public IEnumerator<T> GetEnumerator()
			{
				return _items.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class Top : IEnumerable<T>
		{
			private readonly IEnumerable<T> _items;

			public Top(IEnumerable<T> items, int totalUnfilteredItems)
			{
				if (items == null)
				{
					throw new ArgumentNullException("items");
				}

				_items = items.ToList();
				TotalUnfilteredItems = totalUnfilteredItems;
			}

			public int TotalUnfilteredItems { get; private set; }

			public IEnumerator<T> GetEnumerator()
			{
				return _items.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
