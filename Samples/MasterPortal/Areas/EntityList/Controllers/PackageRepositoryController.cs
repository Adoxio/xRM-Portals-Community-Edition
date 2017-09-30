/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Site.Areas.EntityList.Controllers
{
	[PortalView]
	public class PackageRepositoryController : Controller
	{
		[HttpGet]
		public ActionResult GetRepositories()
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var website = dataAdapterDependencies.GetWebsite();

			var repositories = serviceContext.CreateQuery("adx_packagerepository")
				.Where(e => e.GetAttributeValue<string>("adx_name") != null)
				.Where(e => e.GetAttributeValue<string>("adx_partialurl") != null)
				.Where(e => e.GetAttributeValue<int?>("statecode") == 0)
				.Where(e => e.GetAttributeValue<bool?>("adx_hidden").GetValueOrDefault(false) != true)
				.OrderBy(e => e.GetAttributeValue<int?>("adx_order"))
				.ThenBy(e => e.GetAttributeValue<string>("adx_name"))
				.ToArray();

			AddCrossOriginAccessHeaders(Response);
			
			return new JObjectResult(new JObject
			{
				{ "Repositories", new JArray(repositories.Select(e =>
					new JObject
					{
						{ "Name", e.GetAttributeValue<string>("adx_name") },
						{ "URL", GetPackageRepositoryUrl(website.Id, e.GetAttributeValue<string>("adx_partialurl")) },
					})) },
			});
		}

		[HttpGet]
		public ActionResult Index(Guid entityListId, Guid viewId, string category, string filter, string search)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var repository = serviceContext.CreateQuery("adx_packagerepository")
				.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_entitylistid") == new EntityReference("adx_entitylist", entityListId)
					&& e.GetAttributeValue<int?>("statecode") == 0);

			if (repository == null)
			{
				return HttpNotFound();
			}

			return Index(repository.ToEntityReference(), entityListId, viewId, category, filter, search);
		}

		[HttpGet]
		public ActionResult IndexByPartialUrl(string repositoryPartialUrl, string category, string filter, string search)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var repository = serviceContext.CreateQuery("adx_packagerepository")
				.FirstOrDefault(e => e.GetAttributeValue<string>("adx_partialurl") == repositoryPartialUrl
					&& e.GetAttributeValue<EntityReference>("adx_entitylistid") != null
					&& e.GetAttributeValue<int?>("statecode") == 0);

			if (repository == null)
			{
				return HttpNotFound();
			}

			var entityList = serviceContext.CreateQuery("adx_entitylist")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_entitylistid") == repository.GetAttributeValue<EntityReference>("adx_entitylistid").Id
					&& e.GetAttributeValue<int?>("statecode") == 0);

			if (entityList == null)
			{
				return HttpNotFound();
			}

			var viewId = (entityList.GetAttributeValue<string>("adx_view") ?? string.Empty)
				.Split(',')
				.Select(e =>
				{
					Guid parsed;

					return Guid.TryParse(e.Trim(), out parsed) ? new Guid?(parsed) : null;
				})
				.FirstOrDefault(e => e.HasValue);

			if (viewId == null)
			{
				return HttpNotFound();
			}

			return Index(repository.ToEntityReference(), entityList.Id, viewId.Value, category, filter, search);
		}

		private ActionResult Index(EntityReference packageRepository, Guid entityListId, Guid viewId, string category, string filter, string search)
		{
			var dataAdapter = new EntityListPackageRepositoryDataAdapter(
				packageRepository,
				new EntityReference("adx_entitylist", entityListId),
				new EntityReference("savedquery", viewId),
				new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext),
				Memoize(GetPackageRepositoryUrl),
				GetPackageVersionUrl,
				GetPackageImageUrl);

			var repository = dataAdapter.SelectRepository(category, filter, search);

			if (repository == null)
			{
				return HttpNotFound();
			}

			AddCrossOriginAccessHeaders(Response);
			
			return new JObjectResult(new JObject
			{
				{ "Title", repository.Title },
				{ "InstallerVersion", repository.RequiredInstallerVersion },
				{ "Description", repository.Description },
				{ "Categories", new JArray(repository.Categories.Select(e => e.Name)) },
				{ "Packages", new JArray(repository.Packages.Select(ToJObject)) },
			});
		}

		[HttpGet]
		public ActionResult PackageImage(Guid packageImageId)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);

			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var query = from a in serviceContext.CreateQuery("annotation")
				join i in serviceContext.CreateQuery("adx_packageimage") on a["objectid"] equals i["adx_packageimageid"]
				where a.GetAttributeValue<string>("objecttypecode") == "adx_packageimage"
				where a.GetAttributeValue<bool?>("isdocument") == true
				where i.GetAttributeValue<Guid>("adx_packageimageid") == packageImageId
				where i.GetAttributeValue<OptionSetValue>("statecode") != null && i.GetAttributeValue<OptionSetValue>("statecode").Value == 0
				orderby a["createdon"] descending
				select a;

			var note = query.FirstOrDefault();
			
			return note == null ? HttpNotFound() : dataAdapter.DownloadAction(Response, note);
		}

		[HttpGet]
		public ActionResult PackageVersion(Guid packageVersionId)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);

			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var query = from a in serviceContext.CreateQuery("annotation")
				join v in serviceContext.CreateQuery("adx_packageversion") on a["objectid"] equals v["adx_packageversionid"]
				where a.GetAttributeValue<string>("objecttypecode") == "adx_packageversion"
				where a.GetAttributeValue<bool?>("isdocument") == true
				where v.GetAttributeValue<Guid>("adx_packageversionid") == packageVersionId
				where v.GetAttributeValue<OptionSetValue>("statecode") != null && v.GetAttributeValue<OptionSetValue>("statecode").Value == 0
				orderby a["createdon"] descending
				select a;

			var note = query.FirstOrDefault();

			return note == null ? HttpNotFound() : dataAdapter.DownloadAction(Response, note);
		}

		private void AddCrossOriginAccessHeaders(HttpResponseBase response)
		{
			response.Headers["Access-Control-Allow-Headers"] = "*";
			response.Headers["Access-Control-Allow-Origin"] = "*";
		}

		private string GetPackageImageUrl(Guid websiteId, Guid packageImageId)
		{
			var path = Url.Action("PackageImage", new
			{
				__portalScopeId__ = websiteId,
				packageImageId
			});

			return string.IsNullOrEmpty(path)
				? null
				: new UrlBuilder(path).ToString();
		}

		private string GetPackageRepositoryUrl(Guid websiteId, string repositoryPartialUrl)
		{
			var path = Url.Action("IndexByPartialUrl", new
			{
				__portalScopeId__ = websiteId,
				repositoryPartialUrl
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
			var path = Url.Action("PackageVersion", new
			{
				__portalScopeId__ = websiteId,
				packageVersionId
			});

			return string.IsNullOrEmpty(path) ? null : new UrlBuilder(path).ToString();
		}

		private static JObject ToJObject(Package package)
		{
			return new JObject
			{
				{ "DisplayName", package.DisplayName },
				{ "UniqueName", package.UniqueName },
				{ "URI", package.Uri },
				{ "URL", package.Url },
				{ "Version", package.Version },
				{ "PublisherName", package.PublisherName },
				{ "ReleaseDate", package.ReleaseDate.ToUniversalTime().ToString("o") },
				{ "RequiredInstallerVersion", package.RequiredInstallerVersion },
				{ "Summary", package.Summary },
				{ "Description", package.Description },
				{ "Type", package.Type.ToString() },
				{ "Content", new JObject
				{
					{ "URL", package.ContentUrl }
				} },
				{ "HideFromPackageListing", package.HideFromPackageListing },
				{ "OverwriteWarning", package.OverwriteWarning },
				{ "Icon", package.Icon == null ? null : ToJObject(package.Icon) },
				{ "Images", new JArray(package.Images.Select(ToJObject)) },
				{ "Categories", new JArray(package.Categories.Select(e => e.Name)) },
				{ "Components", new JArray(package.Components.Select(ToJObject)) },
				{ "Dependencies", new JArray(package.Dependencies.Select(ToJObject)) },
				{ "DuplicationEnabled", package.Configuration.DuplicationEnabled },
				{ "ReferenceReplacementTargets", package.Configuration.ReferenceReplacementTargets != null ? new JArray(package.Configuration.ReferenceReplacementTargets.Select(ToJObject)) : new JArray() },
				{ "AttributeReplacementTargets", package.Configuration.AttributeReplacementTargets != null ? new JArray(package.Configuration.AttributeReplacementTargets.Select(ToJObject)) : new JArray() }
			};
		}

		private static JObject ToJObject(PackageComponent component)
		{
			return new JObject
			{
				{ "DisplayName", component.DisplayName },
				{ "URI", component.Uri },
				{ "Version", component.Version },
			};
		}

		private static JObject ToJObject(PackageDependency dependency)
		{
			return new JObject
			{
				{ "DisplayName", dependency.DisplayName },
				{ "URI", dependency.Uri },
				{ "Version", dependency.Version },
			};
		}

		private static JObject ToJObject(PackageImage image)
		{
			return new JObject
			{
				{ "URL", image.Url },
				{ "Description", image.Description },
			};
		}

		private static JObject ToJObject(PackageReferenceReplacements referenceReplacement)
		{
			return new JObject
			{
				{ "Id", referenceReplacement.Id },
				{ "LogicalName", referenceReplacement.LogicalName },
				{ "Name", referenceReplacement.Name },
				{ "Operation", referenceReplacement.Operation }
			};
		}

		private static JObject ToJObject(PackageAttributeReplacements attributeReplacement)
		{
			var jObject = new JObject
			{
				{ "Id", attributeReplacement.Id },
				{ "EntityLogicalName", attributeReplacement.EntityLogicalName },
				{ "AttributeLogicalName", attributeReplacement.AttributeLogicalName },
				{ "Type", attributeReplacement.Type },
				{ "Description", attributeReplacement.Description },
				{ "Label", attributeReplacement.Label },
				{ "InputId", attributeReplacement.InputId },
				{ "Required", attributeReplacement.Required },
				{ "Placeholder", attributeReplacement.Placeholder },
				{ "DefaultValue", attributeReplacement.DefaultValue }
			};

			if (attributeReplacement.MaxLength.HasValue)
			{
				jObject.Add("MaxLength", attributeReplacement.MaxLength);
			}

			if (!string.IsNullOrWhiteSpace(attributeReplacement.Min))
			{
				jObject.Add("Min", attributeReplacement.Min);
			}

			if (!string.IsNullOrWhiteSpace(attributeReplacement.Max))
			{
				jObject.Add("Max", attributeReplacement.Max);
			}

			if (!string.IsNullOrWhiteSpace(attributeReplacement.Step))
			{
				jObject.Add("Step", attributeReplacement.Step);
			}

			if (attributeReplacement.Rows.HasValue)
			{
				jObject.Add("Rows", attributeReplacement.Rows);
			}

			if (attributeReplacement.Cols.HasValue)
			{
				jObject.Add("Cols", attributeReplacement.Cols);
			}

			if (attributeReplacement.Options != null && attributeReplacement.Options.Any())
			{
				jObject.Add("Options", new JArray(attributeReplacement.Options.Select(ToJObject)));
			}

			return jObject;
		}

		private static JObject ToJObject(SelectOption option)
		{
			return new JObject
			{
				{ "Value", option.Value },
				{ "Text", option.Text },
			};
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
