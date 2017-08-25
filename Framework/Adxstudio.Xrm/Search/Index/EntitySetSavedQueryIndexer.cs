/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Search.Index
{
	public class EntitySetSavedQueryIndexer : SavedQueryIndexer
	{
		public EntitySetSavedQueryIndexer(ICrmEntityIndex index, string savedQueryName, string entityLogicalName, IEnumerable<Filter> filters = null, IEnumerable<Link> links = null) : base(index, savedQueryName)
		{
			if (string.IsNullOrEmpty(entityLogicalName))
			{
				throw new ArgumentException("Can't be null or empty.", "entityLogicalName");
			}

			EntityLogicalName = entityLogicalName;
            Filters = filters ?? Enumerable.Empty<Filter>();
            Links = links ?? Enumerable.Empty<Link>();
		}

		protected string EntityLogicalName { get; private set; }
        protected IEnumerable<Filter> Filters { get; private set; }
        protected IEnumerable<Link> Links { get; private set; }

        protected override ICrmEntityIndexer GetIndexerForSavedQuery(Entity query)
        {
            var savedQuery = new SavedQuery(query);

            foreach (var filter in Filters)
            {
                savedQuery.FetchXml.AddFilter(filter.ToXml());
            }

            foreach (var link in Links)
            {
                savedQuery.FetchXml.AddLinkEntity(link.ToXml());
            }

            return new FetchXmlIndexer(Index, savedQuery.FetchXml, savedQuery.TitleAttributeLogicalName);
        }

        protected override IQueryable<Entity> GetSavedQueries(OrganizationServiceContext dataContext)
		{
			var typeCode = this.GetReturnTypeCode(dataContext, EntityLogicalName);

			return base.GetSavedQueries(dataContext).Where(e => e.GetAttributeValue<int?>("returnedtypecode") == typeCode);
		}
	}
}
