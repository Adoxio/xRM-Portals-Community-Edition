/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq.Expressions;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public interface ICmsDataServiceProvider
	{
		void InitializeService<TDataContext>(IDataServiceConfiguration config) where TDataContext : OrganizationServiceContext;

		void AttachFilesToEntity(OrganizationServiceContext context, string entitySet, Guid entityID, IEnumerable<HttpPostedFile> files);

		void DeleteEntity(OrganizationServiceContext context, string entitySet, Guid entityID);

		string GetEntityUrl(OrganizationServiceContext context, string entitySet, Guid entityID);

		IEnumerable<SiteMapChildInfo> GetSiteMapChildren(OrganizationServiceContext context, string siteMapProvider, string startingNodeUrl, string cmsServiceBaseUri);

		void InterceptChange<TEntity>(OrganizationServiceContext context, TEntity entity, UpdateOperations operations) where TEntity : Entity;

		Expression<Func<TEntity, bool>> InterceptQuery<TEntity>(OrganizationServiceContext context) where TEntity : Entity;
	}
}
