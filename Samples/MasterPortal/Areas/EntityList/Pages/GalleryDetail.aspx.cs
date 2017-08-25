/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.EntityList;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.EntityList.Pages
{
	public partial class GalleryDetail : PortalPage
	{
		protected Package Package { get; private set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			AddCrossOriginAccessHeaders();

			Guid id;

			if (!Guid.TryParse(Request.QueryString["id"] ?? string.Empty, out id))
			{
				return;
			}

			var dataAdapter = new PackageDataAdapter(
				new EntityReference("adx_package", id),
				new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext),
				Memoize(GetPackageRepositoryUrl),
				GetPackageVersionUrl,
				GetPackageImageUrl);

			Package = dataAdapter.SelectPackage();

			if (Package == null)
			{
				return;
			}

			PackageHead.Visible = true;
			PackageBreadcrumbs.Visible = true;
			PackageHeader.Visible = true;
			PackageContent.Visible = true;
			PackageNotFound.Visible = false;
			PageBreadcrumbs.Visible = false;
		}

		private void AddCrossOriginAccessHeaders()
		{
			Response.Headers["Access-Control-Allow-Headers"] = "*";
			Response.Headers["Access-Control-Allow-Origin"] = "*";
		}

		private string GetPackageImageUrl(Guid websiteId, Guid packageImageId)
		{
			var path = Url.Action("PackageImage", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				packageImageId,
				area = "EntityList"
			});

			return string.IsNullOrEmpty(path) ? null : new UrlBuilder(path).ToString();
		}

		private string GetPackageRepositoryUrl(Guid websiteId, string repositoryPartialUrl)
		{
			var path = Url.Action("IndexByPartialUrl", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				repositoryPartialUrl,
				area = "EntityList"
			});

			return string.IsNullOrEmpty(path)
				? null
				: new UrlBuilder(path).ToString();
		}

		private string GetPackageRepositoryUrl(Guid websiteId, Guid repositoryId)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var repository = serviceContext.CreateQuery("adx_packagerepository")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_packagerepositoryid") == repositoryId
					&& e.GetAttributeValue<string>("adx_partialurl") != null
					&& e.GetAttributeValue<int?>("statecode") == 0);

			return repository == null
				? null
				: GetPackageRepositoryUrl(websiteId, repository.GetAttributeValue<string>("adx_partialurl"));
		}

		private string GetPackageVersionUrl(Guid websiteId, Guid packageVersionId)
		{
			var path = Url.Action("PackageVersion", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				packageVersionId,
				area = "EntityList"
			});

			return string.IsNullOrEmpty(path) ? null : new UrlBuilder(path).ToString();
		}

		private static Func<Guid, Guid, string> Memoize(Func<Guid, Guid, string> getResult)
		{
			var cache = new Dictionary<string, string>();

			return (a, b) =>
			{
				var key = "{0}:{1}".FormatWith(a, b);
				string result;

				if (cache.TryGetValue(key, out result))
				{
					return result;
				}

				result = getResult(a, b);

				cache[key] = result;

				return result;
			};
		}
	}
}
