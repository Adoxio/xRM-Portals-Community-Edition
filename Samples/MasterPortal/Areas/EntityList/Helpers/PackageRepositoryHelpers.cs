/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.EntityList.Helpers
{
	public static class PackageRepositoryHelpers
	{
		public static string PackageUrl(this HtmlHelper html, UrlHelper url, string packageUniqueName)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			var urlBuilder = new UrlBuilder(href)
			{
				Fragment = packageUniqueName
			};

			return urlBuilder.ToString();
		}

		public static string PackageInstallUrl(this HtmlHelper html, UrlHelper url, string packageUniqueName)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			var urlBuilder = new UrlBuilder(href)
			{
				Fragment = packageUniqueName
			};

			return new Uri("web+adxstudioinstaller:{0}".FormatWith(Uri.EscapeDataString(Uri.EscapeDataString(urlBuilder)))).ToString();
		}

		public static IHtmlString PackageLink(this HtmlHelper html, UrlHelper url, string packageUniqueName)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			var link = new TagBuilder("link");

			link.Attributes["rel"] = "adxstudio.installer";
			link.Attributes["href"] = "{0}#{1}".FormatWith(href, packageUniqueName);

			return new HtmlString(link.ToString());
		}
		
		public static string PackageRepositoryUrl(this HtmlHelper html, UrlHelper url)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			return new UrlBuilder(href).ToString();
		}

		public static string PackageRepositoryInstallUrl(this HtmlHelper html, UrlHelper url)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			var urlBuilder = new UrlBuilder(href);

			return new Uri("web+adxstudioinstaller:{0}".FormatWith(Uri.EscapeDataString(Uri.EscapeDataString(urlBuilder)))).ToString();
		}

		public static IHtmlString PackageRepositoryLink(this HtmlHelper html, UrlHelper url)
		{
			Guid websiteId;
			Guid entityListId;
			Guid viewId;

			if (!TryGetRepositoryInfo(html, out websiteId, out entityListId, out viewId))
			{
				return null;
			}

			var href = url.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = websiteId,
				entityListId,
				viewId,
				area = "EntityList"
			});

			if (string.IsNullOrEmpty(href))
			{
				return null;
			}

			var link = new TagBuilder("link");

			link.Attributes["rel"] = "adxstudio.installer";
			link.Attributes["href"] = href;

			return new HtmlString(link.ToString());
		}

		private static bool TryGetRepositoryInfo(HtmlHelper html, out Guid websiteId, out Guid entityListId, out Guid viewId)
		{
			websiteId = default(Guid);
			entityListId = default(Guid);
			viewId = default(Guid);
			
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			if (portalViewContext.Entity == null)
			{
				return false;
			}

			var entityListAttribute = portalViewContext.Entity.GetAttribute("adx_entitylist");

			if (entityListAttribute == null)
			{
				return false;
			}

			var entityListReference = entityListAttribute.Value as EntityReference;

			if (entityListReference == null)
			{
				return false;
			}

			var serviceContext = portalViewContext.CreateServiceContext();

			var entityList = serviceContext.CreateQuery("adx_entitylist")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_entitylistid") == entityListReference.Id);

			if (entityList == null)
			{
				return false;
			}
			
			if (!TryGetViewId(entityList, out viewId))
			{
				return false;
			}

			websiteId = portalViewContext.Website.EntityReference.Id;
			entityListId = entityList.Id;
			
			return true;
		}

		private static bool TryGetViewId(Entity entityList, out Guid viewId)
		{
			// First, try get the view from the newer view configuration JSON.
			viewId = Guid.Empty;
			var viewMetadataJson = entityList.GetAttributeValue<string>("adx_views");

			if (!string.IsNullOrWhiteSpace(viewMetadataJson))
			{
				try
				{
					var viewMetadata = ViewMetadata.Parse(viewMetadataJson);

					var view = viewMetadata.Views.FirstOrDefault();

					if (view != null)
					{
						viewId = view.ViewId;

						return true;
					}
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error parsing adx_views JSON: {0}", e.ToString()));
                }
			}

			// Fall back to the legacy comma-delimited list of IDs.
			var viewIds = (entityList.GetAttributeValue<string>("adx_view") ?? string.Empty)
				.Split(',')
				.Select(s =>
				{
					Guid id;

					return Guid.TryParse(s, out id) ? new Guid?(id) : null;
				})
				.Where(id => id != null);

			viewId = viewIds.FirstOrDefault() ?? Guid.Empty;

			return viewId != Guid.Empty;
		}
	}
}
