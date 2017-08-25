/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Adxstudio.Xrm.Search.Services
{
	[ServiceContract]
	public interface ICrmEntityIndexUpdaterService
	{
		[OperationContract, WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		void DeleteEntity(string entityLogicalName, Guid id, string searchProvider);

		[OperationContract, WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		void DeleteEntitySet(string entityLogicalName, string searchProvider);

		[OperationContract, WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		void UpdateEntity(string entityLogicalName, Guid id, string searchProvider);

		[OperationContract, WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		void UpdateEntitySet(string entityLogicalName, string searchProvider);
	}
}
