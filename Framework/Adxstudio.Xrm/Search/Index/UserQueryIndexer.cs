/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Search.Index
{
	public class UserQueryIndexer : SavedQueryIndexer
	{
		public UserQueryIndexer(ICrmEntityIndex index, string savedQueryName) : base(index, savedQueryName) { }

		protected override ICrmEntityIndexer GetIndexerForSavedQuery(Entity query)
		{
			var savedQuery = new UserQuery(query);

			return new FetchXmlIndexer(Index, savedQuery.FetchXml, savedQuery.TitleAttributeLogicalName);
		}

		protected override IQueryable<Entity> GetSavedQueries(OrganizationServiceContext dataContext)
		{
			return dataContext.CreateQuery("userquery").Where(e => e.GetAttributeValue<string>("name") == SavedQueryName);
		}
	}
}
