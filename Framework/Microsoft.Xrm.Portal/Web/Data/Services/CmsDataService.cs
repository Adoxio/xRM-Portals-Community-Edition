/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq.Expressions;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	/// <summary>
	/// Base class for ADO.NET Data Services to support CMS front-side editing features.
	/// </summary>
	/// <typeparam name="TDataContext">
	/// The <see cref="CrmOrganizationServiceContext"/> whose entities are exposed by this service.
	/// </typeparam>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public abstract class CmsDataService<TDataContext> : DataService<TDataContext> where TDataContext : OrganizationServiceContext
	{
		public static string PortalName { get; set; }

		public static void InitializeService(IDataServiceConfiguration config)
		{
			GetProvider().InitializeService<TDataContext>(config);

			config.SetServiceOperationAccessRule("AttachFilesToEntity", ServiceOperationRights.All);
			config.SetServiceOperationAccessRule("DeleteEntity", ServiceOperationRights.All);
			config.SetServiceOperationAccessRule("GetEntityUrl", ServiceOperationRights.ReadSingle);
			config.SetServiceOperationAccessRule("GetSiteMapChildren", ServiceOperationRights.AllRead);
		}

		/// <summary>
		/// Service operation to add a file attachment annotation to an entity, by Guid and entity set name.
		/// </summary>
		[WebInvoke]
		public virtual string AttachFilesToEntity(string entitySet, Guid entityID)
		{
			var files = HttpContext.Current.Request.Files;

			if (files.Count < 1)
			{
				throw new DataServiceException(400, "Uploaded file not found.");
			}

			var postedFiles = new List<HttpPostedFile>();

			for (var i = 0; i < files.Count; i++)
			{
				postedFiles.Add(files[i]);
			}

			GetProvider().AttachFilesToEntity(CurrentDataSource, entitySet, entityID, postedFiles);

			// We need to return some kind of response (even an empty string) so that iframe-based async file
			// upload techniques will work. (I've found they don't like a "204 No Content" response, instead
			// of "200 OK".)
			return string.Empty;
		}

		/// <summary>
		/// Service operation to delete an entity.
		/// </summary>
		[WebInvoke]
		public virtual void DeleteEntity(string entitySet, Guid entityID)
		{
			GetProvider().DeleteEntity(CurrentDataSource, entitySet, entityID);
		}

		/// <summary>
		/// Service operation to get a URL for an entity, by Guid and entity set name.
		/// </summary>
		/// <returns>Entity URL as a string.</returns>
		[WebGet]
		public virtual string GetEntityUrl(string entitySet, Guid entityID)
		{
			return GetProvider().GetEntityUrl(CurrentDataSource, entitySet, entityID);
		}

		[WebGet]
		public virtual IEnumerable<SiteMapChildInfo> GetSiteMapChildren(string siteMapProvider, string startingNodeUrl, string cmsServiceBaseUri)
		{
			return GetProvider().GetSiteMapChildren(CurrentDataSource, siteMapProvider, startingNodeUrl, cmsServiceBaseUri);
		}

		/// <summary>
		/// Intercepts changes on the entity types we care about (through codegen <see cref="ChangeInterceptorAttribute">Change Interceptors</see>).
		/// </summary>
		/// <exception cref="DataServiceException">
		/// Throws 403 <see cref="DataServiceException"/> on any security permission failure, of when passed a <see cref="Entity"/> of
		/// an entity type that is not explicitly handled by this implementation.
		/// </exception>
		protected virtual void InterceptChange<TEntity>(TEntity entity, UpdateOperations operations) where TEntity : Entity
		{
			GetProvider().InterceptChange(CurrentDataSource, entity, operations);
		}

		protected virtual Expression<Func<TEntity, bool>> InterceptQuery<TEntity>() where TEntity : Entity
		{
			return GetProvider().InterceptQuery<TEntity>(CurrentDataSource);
		}

		protected static ICmsDataServiceProvider GetProvider()
		{
			return PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICmsDataServiceProvider>();
		}
	}
}
