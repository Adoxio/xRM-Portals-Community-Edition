/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Adxstudio.Xrm.Search.Services.Client
{
	public class CrmEntityIndexSearcherServiceClient : ClientBase<ICrmEntityIndexSearcherService>, ICrmEntityIndexSearcher
	{
		public CrmEntityIndexSearcherServiceClient() { }

		public CrmEntityIndexSearcherServiceClient(string endpointConfigurationName) : base(endpointConfigurationName) { }

		public CrmEntityIndexSearcherServiceClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexSearcherServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexSearcherServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

		public string SearchProvider { get; set; }

		public void Dispose()
		{
		    Close();
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}

		public IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode = 0)
		{
			return Channel.GetIndexedEntityInfo(languageCode, SearchProvider);
		}

		public ICrmEntitySearchResultPage Search(ICrmEntityQuery query)
		{
			var logicalNames = query.LogicalNames.Any() ? string.Join(",", query.LogicalNames.ToArray()) : string.Empty;

			return Channel.Search(query.QueryText, query.PageNumber, query.PageSize, logicalNames, SearchProvider);
		}
	}
}
