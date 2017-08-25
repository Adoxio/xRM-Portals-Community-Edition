/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel;

namespace Adxstudio.Xrm.Search.WindowsAzure
{
	/// <summary>
	/// Service contract for search index service exposed by <see cref="CloudDriveServiceSearchProvider"/>.
	/// </summary>
	[ServiceContract]
	public interface ISearchService
	{
		[OperationContract]
		bool BuildIndex();

		[OperationContract]
		EntityIndexInfo GetIndexedEntityInfo();

		[OperationContract]
		EntitySearchResultPage Search(string query, int page, int pageSize, string logicalNames, string scope, string filter);

		[OperationContract]
		void DeleteEntity(string entityLogicalName, Guid id);

		[OperationContract]
		void DeleteEntitySet(string entityLogicalName);

		[OperationContract]
		void UpdateEntity(string entityLogicalName, Guid id);

		[OperationContract]
		void UpdateEntitySet(string entityLogicalName);
	}
}
