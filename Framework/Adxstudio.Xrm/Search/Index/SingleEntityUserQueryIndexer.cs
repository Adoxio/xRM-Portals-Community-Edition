/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Search.Index
{
	public class SingleEntityUserQueryIndexer : EntitySetUserQueryIndexer
	{
		private readonly Guid _id;

		public SingleEntityUserQueryIndexer(ICrmEntityIndex index, string savedQueryName, string entityLogicalName, Guid id) : base(index, savedQueryName, entityLogicalName)
		{
			_id = id;
		}

		protected override ICrmEntityIndexer GetIndexerForSavedQuery(Entity query)
		{
			var savedQuery = new UserQuery(query);

			var filteredFetchXml = this.GetFetchXmlFilteredToSingleEntity(savedQuery.FetchXml.ToString(), Index.DataContext, EntityLogicalName, _id);

			return new FetchXmlIndexer(Index, filteredFetchXml, savedQuery.TitleAttributeLogicalName);
		}
	}
}
