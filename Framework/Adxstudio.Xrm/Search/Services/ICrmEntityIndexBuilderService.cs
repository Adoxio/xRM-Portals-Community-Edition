/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.ServiceModel;
using System.ServiceModel.Web;

namespace Adxstudio.Xrm.Search.Services
{
	[ServiceContract]
	public interface ICrmEntityIndexBuilderService
	{
		[OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool BuildIndex(string searchProvider);
	}
}
