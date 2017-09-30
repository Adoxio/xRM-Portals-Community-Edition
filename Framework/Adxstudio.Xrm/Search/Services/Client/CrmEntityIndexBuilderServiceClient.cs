/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Adxstudio.Xrm.Search.Index;

namespace Adxstudio.Xrm.Search.Services.Client
{
	public class CrmEntityIndexBuilderServiceClient : ClientBase<ICrmEntityIndexBuilderService>, ICrmEntityIndexBuilder
	{
		public CrmEntityIndexBuilderServiceClient() { }

		public CrmEntityIndexBuilderServiceClient(string endpointConfigurationName) : base(endpointConfigurationName) { }

		public CrmEntityIndexBuilderServiceClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexBuilderServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public CrmEntityIndexBuilderServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

		public string SearchProvider { get; set; }

		public void BuildIndex()
		{
			Channel.BuildIndex(SearchProvider);
		}

		public void Dispose()
		{
			Close();
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}
	}
}
