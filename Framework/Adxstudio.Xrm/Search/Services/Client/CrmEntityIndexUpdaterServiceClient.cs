/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Adxstudio.Xrm.Search.Index;

namespace Adxstudio.Xrm.Search.Services.Client
{
	public class CrmEntityIndexUpdaterServiceClient : ClientBase<ICrmEntityIndexUpdaterService>, ICrmEntityIndexUpdater
	{
		public CrmEntityIndexUpdaterServiceClient() { }

		public CrmEntityIndexUpdaterServiceClient(string endpointConfigurationName) : base(endpointConfigurationName) { }

		public CrmEntityIndexUpdaterServiceClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexUpdaterServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexUpdaterServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

		public string SearchProvider { get; set; }

		public void Dispose()
		{
			Close();
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}

		public void DeleteEntity(string entityLogicalName, Guid id)
		{
			Channel.DeleteEntity(entityLogicalName, id, SearchProvider);
		}

		public void DeleteEntitySet(string entityLogicalName)
		{
			Channel.DeleteEntitySet(entityLogicalName, SearchProvider);
		}

		public void UpdateEntity(string entityLogicalName, Guid id)
		{
			Channel.UpdateEntity(entityLogicalName, id, SearchProvider);
		}

		public void UpdateEntitySet(string entityLogicalName)
		{
			Channel.UpdateEntitySet(entityLogicalName, SearchProvider);
		}

		public void UpdateEntitySet(string entityLogicalName, string entityAttribute, List<Guid> entityIds)
		{
			return;
		}

		public void UpdateCmsEntityTree(string entityLogicalName, Guid rootEntityId, int? lcid = null)
        {
            return;
        }
    }
}
