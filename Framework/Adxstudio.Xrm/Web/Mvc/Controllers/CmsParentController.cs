/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Handlers.ElFinder;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Controllers
{
	[PortalView]
	public class CmsParentController : Controller
	{
		internal const string GetParentOptionsForEntityRoutePath = "_services/portal/{__portalScopeId__}/__parents/{entityLogicalname}/{id}";

		internal static string GetAppRelativePathTemplate(Guid portalScopeId, string entityLogicalNameTemplateVariableName, string idTemplateVariableName)
		{
			var logicalNamePlaceholder = "__{0}__".FormatWith(Guid.NewGuid());
			var idPlaceholder = "__{0}__".FormatWith(Guid.NewGuid());

			var uriTemplate = new UriTemplate(GetParentOptionsForEntityRoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__",      portalScopeId.ToString() },
				{ "entityLogicalName",      logicalNamePlaceholder  },
				{ "id",                     idPlaceholder           },
			});

			var path = "~{0}".FormatWith(uri.PathAndQuery);

			return path.Replace(logicalNamePlaceholder, "{{{0}}}".FormatWith(entityLogicalNameTemplateVariableName)).Replace(idPlaceholder, "{{{0}}}".FormatWith(idTemplateVariableName));
		}

		private const int PageSize = 10;

		[OutputCache(CacheProfile = "User")]
		[HttpGet]
		public ActionResult GetParentOptions(Guid __portalScopeId__, int page = 1, string search = null, string selectedId = null)
		{
			try
			{
				var cmsFileSystem = CreateFileSystem();

				Guid parsedId;

				if (Guid.TryParse(selectedId, out parsedId))
				{
					var selected = cmsFileSystem.Using(new DirectoryContentHash(new EntityReference("adx_webpage", parsedId)), fs => fs.Current);

					return new JObjectResult(new JObject
					{
						{ "d", new JArray(GetWebPageReferenceJson(selected, __portalScopeId__)) },
						{ "more", false }
					});
				}

				var cmsTree = cmsFileSystem.Using(fs => fs.TreeOfType("adx_webpage"));

				var query = GetParentOptions(cmsTree);

				if (!string.IsNullOrEmpty(search))
				{
					query = query.Where(e => e.Item1.name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase));
				}
				
				var nodes = query
					.OrderBy(e => e.Item1.name)
					.ToArray();

				var offset = (Math.Max(page, 1) - 1) * PageSize;
				var results = nodes.Skip(offset).Take(PageSize);

				return new JObjectResult(new JObject
				{
					{ "d", new JArray(results.Select(e => GetWebPageReferenceJson(e, __portalScopeId__))) },
					{ "more", nodes.Length > (offset + PageSize) }
				});
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[OutputCache(CacheProfile = "User")]
		[HttpGet]
		public ActionResult GetParentOptionsForEntity(Guid __portalScopeId__, string entityLogicalName, Guid id, int page = 1, string search = null, string selectedId = null)
		{
			var child = new EntityReference(entityLogicalName, id);

			try
			{
				var cmsFileSystem = CreateFileSystem();

				Guid parsedId;

				if (Guid.TryParse(selectedId, out parsedId))
				{
					var selected = cmsFileSystem.Using(new DirectoryContentHash(new EntityReference("adx_webpage", parsedId)), fs => fs.Current);

					return new JObjectResult(new JObject
					{
						{ "d", new JArray(GetWebPageReferenceJson(selected, __portalScopeId__)) },
						{ "more", false }
					});
				}

				var cmsTree = cmsFileSystem.Using(fs => fs.TreeOfType("adx_webpage"));

				var query = GetParentOptions(child, GetExistingParent(child), cmsTree);

				if (!string.IsNullOrEmpty(search))
				{
					query = query.Where(e => e.Item1.name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase));
				}

				var nodes = query
					.OrderBy(e => e.Item1.name)
					.ToArray();

				var offset = (Math.Max(page, 1) - 1) * PageSize;
				var results = nodes.Skip(offset).Take(PageSize);

				return new JObjectResult(new JObject
				{
					{ "d", new JArray(results.Select(e => GetWebPageReferenceJson(e, __portalScopeId__))) },
					{ "more", nodes.Length > (offset + PageSize) }
				});
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		private EntityReference GetExistingParent(EntityReference child)
		{
			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider();
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);

			return contentMapProvider == null
				? GetExistingParent(child, dataAdapterDependencies)
				: contentMapProvider.Using(map => GetExistingParent(child, dataAdapterDependencies, map));
		}

		private static readonly IDictionary<string, Tuple<string, string>> ParentSchema = new Dictionary<string, Tuple<string, string>>
		{
			{ "adx_webpage", new Tuple<string, string>("adx_webpageid", "adx_parentpageid") },
			{ "adx_webfile", new Tuple<string, string>("adx_webfileid", "adx_parentpageid") },
			{ "adx_shortcut", new Tuple<string, string>("adx_shortcutid", "adx_parentpage_webpageid") },
			{ "adx_event", new Tuple<string, string>("adx_eventid", "adx_parentpageid") },
			{ "adx_blog", new Tuple<string, string>("adx_blogid", "adx_parentpageid") },
			{ "adx_communityforum", new Tuple<string, string>("adx_communityforumid", "adx_parentpageid") },
		};

		private EntityReference GetExistingParent(EntityReference child, IDataAdapterDependencies dependencies)
		{
			var serviceContext = dependencies.GetServiceContext();

			Tuple<string, string> parentSchema;

			if (ParentSchema.TryGetValue(child.LogicalName, out parentSchema))
			{
				return serviceContext.CreateQuery(child.LogicalName)
					.Where(e => e.GetAttributeValue<Guid>(parentSchema.Item1) == child.Id)
					.Select(e => e.GetAttributeValue<EntityReference>(parentSchema.Item2))
					.FirstOrDefault();
			}

			return null;
		}

		private EntityReference GetExistingParent(EntityReference child, IDataAdapterDependencies dependencies, ContentMap map)
		{
			if (child.LogicalName == "adx_webpage")
			{
				WebPageNode webPage;

				if (map.TryGetValue(child, out webPage))
				{
					var parent = webPage.Parent;

					return parent == null ? null : parent.ToEntityReference();
				}
			}

			if (child.LogicalName == "adx_webfile")
			{
				WebFileNode webFile;

				if (map.TryGetValue(child, out webFile))
				{
					var parent = webFile.Parent;

					return parent == null ? null : parent.ToEntityReference();
				}
			}

			if (child.LogicalName == "adx_shortcut")
			{
				ShortcutNode shortcut;

				if (map.TryGetValue(child, out shortcut))
				{
					var parent = shortcut.Parent;

					return parent == null ? null : parent.ToEntityReference();
				}
			}

			return GetExistingParent(child, dependencies);
		}

		private IFileSystem CreateFileSystem()
		{
			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider();
			var contentMapUrlProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IContentMapEntityUrlProvider>();
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);

			return contentMapProvider == null || contentMapUrlProvider == null
				? (IFileSystem)new EntityFileSystem(dataAdapterDependencies)
				: new ContentMapFileSystem(contentMapProvider, contentMapUrlProvider, dataAdapterDependencies);
		}

		private ActionResult Error(Exception e)
		{
			Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			return new JObjectResult(new JObject
			{
				{
					"error", new JObject
					{
						{ "code", new JValue(string.Empty) },
						{
							"message", new JObject
							{
								{ "value", new JValue(e.Message) }
							}
						},
						{ "innererror", GetExceptionJson(e) }
					}
				}
			});
		}

		private IEnumerable<Tuple<DirectoryTreeNode, string[]>> GetParentOptions(DirectoryTreeNode node)
		{
			return GetParentOptions(node, new string[] { });
		}

		private IEnumerable<Tuple<DirectoryTreeNode, string[]>> GetParentOptions(DirectoryTreeNode node, string[] path)
		{
			// A node is a valid parent option if the node is either already the parent of child, or if the node is
			// a web page that the user has change permission for.

			if (node.EntityReference.LogicalName != "adx_webpage")
			{
				yield break;
			}

			if (node.write)
			{
				yield return new Tuple<DirectoryTreeNode, string[]>(node, path);
			}

			foreach (var option in node.dirs.SelectMany(dir => GetParentOptions(dir, path.Concat(new[] { node.name }).ToArray())))
			{
				yield return option;
			}
		}

		private IEnumerable<Tuple<DirectoryTreeNode, string[]>> GetParentOptions(EntityReference child, EntityReference existingParent, DirectoryTreeNode node)
		{
			return GetParentOptions(child, existingParent, node, new string[] { });
		}

		private IEnumerable<Tuple<DirectoryTreeNode, string[]>> GetParentOptions(EntityReference child, EntityReference existingParent, DirectoryTreeNode node, string[] path)
		{
			// A node is a valid parent option if the node is either already the parent of child, or if the node is
			// a web page that the user has change permission for, that isn't child, or a descendant of child.

			if (node.EntityReference.LogicalName != "adx_webpage")
			{
				yield break;
			}

			if (node.EntityReference.Equals(child))
			{
				yield break;
			}

			if (node.write || (existingParent != null && node.EntityReference.Equals(existingParent)))
			{
				yield return new Tuple<DirectoryTreeNode, string[]>(node, path);
			}

			foreach (var option in node.dirs.SelectMany(dir => GetParentOptions(child, existingParent, dir, path.Concat(new[] { node.name }).ToArray())))
			{
				yield return option;
			}
		}

		private static JObject GetExceptionJson(Exception e)
		{
			if (e == null)
			{
				throw new ArgumentNullException("e");
			}

			var json = new JObject
			{
				{ "message", new JValue(e.Message) },
				{ "type", new JValue(e.GetType().FullName) },
				{ "stacktrace", new JValue(e.StackTrace) },
			};

			if (e.InnerException != null)
			{
				json["internalexception"] = GetExceptionJson(e.InnerException);
			}

			return json;
		}

		private static JObject GetWebPageReferenceJson(IDirectory directory, Guid portalScopeId)
		{
			var entity = directory.Entity;
			var name = entity.GetAttributeValue<string>("adx_name");

			var json = new JObject
			{
				{
					"__metadata", new JObject
					{
						{ "uri", new JValue(VirtualPathUtility.ToAbsolute(CmsEntityRouteHandler.GetAppRelativePath(portalScopeId, entity.ToEntityReference()))) },
						{ "type", new JValue(entity.GetType().FullName) },
					}
					},
				{ "Id", new JValue(entity.Id.ToString()) },
				{ "LogicalName", new JValue(entity.LogicalName) },
				{ "Name", new JValue(name) },
				{ "adx_name", new JValue(name) },
				{ "adx_webpageid", new JValue(entity.Id.ToString()) }
			};

			return json;
		}

		private static JObject GetWebPageReferenceJson(Tuple<DirectoryTreeNode, string[]> node, Guid portalScopeId)
		{
			var entity = node.Item1.Entity;
			var name = entity.GetAttributeValue<string>("adx_name");

			var json = new JObject
			{
				{
					"__metadata", new JObject
					{
						{ "uri", new JValue(VirtualPathUtility.ToAbsolute(CmsEntityRouteHandler.GetAppRelativePath(portalScopeId, entity.ToEntityReference()))) },
						{ "type", new JValue(entity.GetType().FullName) },
					}
					},
				{ "Id", new JValue(entity.Id.ToString()) },
				{ "LogicalName", new JValue(entity.LogicalName) },
				{ "Name", new JValue(name) },
				{ "Path", new JArray(node.Item2.AsEnumerable()) },
				{ "adx_name", new JValue(name) },
				{ "adx_webpageid", new JValue(entity.Id.ToString()) }
			};

			return json;
		}
	}
}
