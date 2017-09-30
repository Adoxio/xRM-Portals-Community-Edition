/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Security;
using Lucene.Net.Documents;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Adxstudio.Xrm.AspNet;
using Adxstudio.Xrm.ContentAccess;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Search
{
	public class PortalSearchResultFactory : CrmEntitySearchResultFactory
	{
		public PortalSearchResultFactory(string portalName, ICrmEntityIndex index, ICrmEntitySearchResultFragmentProvider fragmentProvider) : base(index, fragmentProvider)
		{
			PortalName = portalName;
		}

		protected string PortalName { get; private set; }

		protected override Uri GetUrl(OrganizationServiceContext context, Document document, float score, int number, Entity entity)
		{
			if (entity == null)
			{
				return null;
			}

			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>();

			try
			{
				var url = urlProvider.GetUrl(context, entity);

				return url == null
					? null
					: new Uri(url, UriKind.RelativeOrAbsolute);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error generating search result URL, returning null URL: {0}", e.ToString()));

                return null;
			}
		}

		protected override bool Validate(OrganizationServiceContext context, CrmEntitySearchResult result)
		{
			// With permissions now indexed, invalid results should have been filtered out index time. 
			// Monitor whether this is true or whether the permission indexing is missing corner cases. 
			if (!base.Validate(context, result))
			{
				return false;
			}

			if (result.Url == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Search result had invalid URL. Logical name: {0}, ID: {1}", EntityNamePrivacy.GetEntityName(result.EntityLogicalName), result.EntityID));
				return false;
			}

			if (!ValidateCmsSecurityProvider(context, result))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search result failed CMS security check. Logical name: {0}, ID: {1}", EntityNamePrivacy.GetEntityName(result.EntityLogicalName), result.EntityID));
				if (!ValidateEntityPermission(context, result))
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search result failed entity permissions security check. Logical name: {0}, ID: {1}", EntityNamePrivacy.GetEntityName(result.EntityLogicalName), result.EntityID));
					return false;
				}
			}

			// Checking products and CALs for a full page of results can be costly
			// Only check if explicitly enabled (e.g. because a customer is having issues with CAL/product indexing)
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CALProductSearchPostFiltering))
			{
				var contentAccessLevelProvider = new ContentAccessLevelProvider();
				var productAccessProvider = new ProductAccessProvider();

				if (!ValidateContentAccessLevelAndProducts(context, result, contentAccessLevelProvider, productAccessProvider))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Search result failed CAL or product filtering. Logical name: {0}, ID: {1}", EntityNamePrivacy.GetEntityName(result.EntityLogicalName), result.EntityID));
					return false;
				}
			}

			return true;
		}

		private bool ValidateCmsSecurityProvider(OrganizationServiceContext serviceContext, CrmEntitySearchResult result)
		{
			return PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName)
				.TryAssert(serviceContext, result.Entity, CrmEntityRight.Read);
		}

		private bool ValidateEntityPermission(OrganizationServiceContext serviceContext, CrmEntitySearchResult result)
		{
			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return false;
			}

			var permissionResult = new CrmEntityPermissionProvider(PortalName).TryAssert(serviceContext, result.Entity);

			return permissionResult.RulesExist && permissionResult.CanRead;
		}

		/// <summary>
		/// Validates the content access level and product filtering.
		/// </summary>
		/// <param name="serviceContext">The service context.</param>
		/// <param name="result">The result.</param>
		/// <returns>Boolean</returns>
		private bool ValidateContentAccessLevelAndProducts(OrganizationServiceContext serviceContext, CrmEntitySearchResult result, ContentAccessLevelProvider contentAccessLevelProvider, ProductAccessProvider productAccessProvider)
		{
			if (result == null || result.EntityID == null)
			{
				return false;
			}

			// Content access levels/products will only filter knowledge articles
			if (result.EntityLogicalName != "knowledgearticle")
			{
				return true;
			}

			var baseFetch = string.Format(@"
				<fetch mapping='logical'>
					<entity name='knowledgearticle'>
						<filter type='and'>
							<condition attribute='knowledgearticleid' operator='eq' value='{0}' />
						</filter>
					</entity>
				</fetch>", result.EntityID);

			if (!contentAccessLevelProvider.IsEnabled() && !productAccessProvider.IsEnabled()) return true;

			Fetch filterCheckFetch = Fetch.Parse(baseFetch);
			contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, filterCheckFetch);
			productAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, filterCheckFetch);

			// If there are no results, user didn't have access to products or CALs associated to article
			var response = (RetrieveMultipleResponse)serviceContext.Execute(filterCheckFetch.ToRetrieveMultipleRequest());
			return response.EntityCollection != null && response.EntityCollection.Entities.Any();
		}
	}
}
