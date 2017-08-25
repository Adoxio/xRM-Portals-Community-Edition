/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Adxstudio.Xrm.Search.Services
{
	[ServiceContract]
	public interface ICrmEntityIndexSearcherService
	{
		[OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
		List<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode, string searchProvider);

		[OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
		CrmEntitySearchResultPage Search(string query, int page, int pageSize, string logicalNames, string searchProvider);
	}
}
