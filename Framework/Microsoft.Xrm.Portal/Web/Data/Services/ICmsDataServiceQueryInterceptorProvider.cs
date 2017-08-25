/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public interface ICmsDataServiceQueryInterceptorProvider
	{
		ICmsDataServiceQueryInterceptor GetInterceptor();
	}
}
