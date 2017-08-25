/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Data
{
	public class PaginatedList
	{
		public enum Page
		{
			First,
			Last
		}

		private static int? _pageSize;

		public static int PageSize
		{
			get
			{
				if (_pageSize == null)
				{
					var portal = PortalCrmConfigurationManager.CreatePortalContext();

					int pageSize;

					if (!int.TryParse(portal.ServiceContext.GetSiteSettingValueByName(portal.Website, "page_size_default"), out pageSize) || pageSize < 1)
					{
						pageSize = 10;
					}

					_pageSize = pageSize;
				}

				return _pageSize.Value;
			}
		}
	}

	public class PaginatedList<T> : List<T>, IPaginated
	{
		public PaginatedList(int? pageNumber, int totalCount, IEnumerable<T> items)
		{
			PageNumber = pageNumber.GetValueOrDefault(1);
			TotalCount = totalCount;
			TotalPages = (int)Math.Ceiling(TotalCount / (double)PaginatedList.PageSize);

			AddRange(items);
		}

		public PaginatedList(int? pageNumber, int totalCount, Func<int, int, IEnumerable<T>> select)
			: this(pageNumber, totalCount, select((pageNumber.GetValueOrDefault(1) - 1) * PaginatedList.PageSize, PaginatedList.PageSize)) { }

		public PaginatedList(PaginatedList.Page page, int totalCount, Func<int, int, IEnumerable<T>> select)
			: this(page == PaginatedList.Page.Last && totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)PaginatedList.PageSize) : 1, totalCount, select) { }

		public bool HasPreviousPage { get { return PageNumber > 1; } }

		public bool HasNextPage { get { return PageNumber < TotalPages; } }

		public int PageNumber { get; private set; }

		public int TotalCount { get; private set; }

		public int TotalPages { get; private set; }
	}
}
