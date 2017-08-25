/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public static class EntityUrlProviderExtensions
	{
		public static ApplicationPath GetApplicationPath(
			this IEntityUrlProvider entityUrlProvider,
			OrganizationServiceContext context,
			Entity entity,
			string partialUrlLogicalName,
			Relationship parentEntityRelationship,
			string parentEntityName,
			Func<OrganizationServiceContext, Entity, ApplicationPath> getParentApplicationPath,
			string siteMarker = null)
		{
			var parentEntity = entity.GetRelatedEntity(context, parentEntityRelationship);

			var partialUrlAttributeValue = entity.GetAttributeValue<object>(partialUrlLogicalName);
			var partialUrl = partialUrlAttributeValue == null ? null : partialUrlAttributeValue.ToString();

			if (parentEntity == null)
			{
				if (siteMarker == null)
				{
					return ApplicationPath.FromPartialPath(partialUrl);
				}
				
				var siteMarkerPage = context.GetPageBySiteMarkerName(context.GetWebsite(entity), siteMarker);
				var siteMarkerUrl = context.GetApplicationPath(siteMarkerPage);

				return JoinApplicationPath(siteMarkerUrl.PartialPath, partialUrl);
			}

			var parentUrl = getParentApplicationPath(context, parentEntity);

			var url = JoinApplicationPath(parentUrl.PartialPath, partialUrl);

			return url;
		}

		private static ApplicationPath JoinApplicationPath(string basePath, string extendedPath)
		{
			if (basePath.Contains("?") || basePath.Contains(":") || basePath.Contains("//") || basePath.Contains("&")
				|| basePath.Contains("%3f") || basePath.Contains("%2f%2f") || basePath.Contains("%26"))
			{
				throw new ApplicationException("Invalid base path");
			}

			if (extendedPath.Contains("?") || extendedPath.Contains("&") || extendedPath.Contains("//")
				|| extendedPath.Contains(":") || extendedPath.Contains("%3f") || extendedPath.Contains("%2f%2f") || extendedPath.Contains("%26"))
			{
				throw new ApplicationException("Invalid extendedPath");
			}

			var path = "{0}/{1}".FormatWith(basePath.TrimEnd('/'), extendedPath.TrimStart('/'));

			return ApplicationPath.FromPartialPath(path);
		}
	}
}
