/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public interface ICmsDataServiceQueryInterceptor
	{
		IQueryable<TEntity> InterceptQuery<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity;
	}
}
