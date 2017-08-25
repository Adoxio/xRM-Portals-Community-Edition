/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Collections.Generic
{
	internal class FetchXmlPostFilterPaginator
	{
		private readonly Predicate<Entity> _filter;
		private readonly int _initialLimitMultiple;
		private readonly OrganizationServiceContext _serviceContext;
		
		private XDocument _fetchXml;

		public FetchXmlPostFilterPaginator(OrganizationServiceContext serviceContext, XDocument fetchXml, Predicate<Entity> filter, int initialLimitMultiple = 1)
		{
			serviceContext.ThrowOnNull("serviceContext");
			fetchXml.ThrowOnNull("fetchXml");
			filter.ThrowOnNull("filter");

			if (initialLimitMultiple < 1)
			{
				throw new ArgumentException("Value can't be less than 1.", "initialLimitMultiple");
			}

			_serviceContext = serviceContext;
			_fetchXml = fetchXml;
			_filter = filter;
			_initialLimitMultiple = initialLimitMultiple;
		}

		public IEnumerable<Entity> Select(int offset, int limit)
		{
			if (offset % limit != 0)
			{
				throw new ArgumentException("limit value must be a factor of offset");
			}

			var items = new List<Entity>();

			Select(0, (offset + limit) * _initialLimitMultiple, offset + limit, items);

			return items.Skip(offset).Take(limit).ToArray();
		}

		private void Select(int offset, int limit, int itemLimit, ICollection<Entity> items)
		{
			_fetchXml.Root.SetAttributeValue("page", (offset / limit) + 1);
			_fetchXml.Root.SetAttributeValue("count", limit);

			var response = (RetrieveMultipleResponse)_serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(_fetchXml.ToString())
			});

			var selected = response.EntityCollection.Entities.ToArray();

			foreach (var entity in selected)
			{
				_serviceContext.Attach(entity);
			}

			foreach (var item in selected.Where(item => _filter(item)))
			{
				items.Add(item);

				if (items.Count >= itemLimit)
				{
					return;
				}
			}

			// If there are fewer items than what were asked for, there must be no further items
			// to select, and so we should quit after processing the items we did get.
			if (selected.Length < limit)
			{
				return;
			}

			// We still don't have enough items, so select the next page.
			Select(offset + limit, limit, itemLimit, items);
		}
	}
}
