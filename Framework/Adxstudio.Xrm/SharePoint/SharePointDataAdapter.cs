/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Adxstudio.SharePoint;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.SharePoint
{
	public class SharePointDataAdapter : ISharePointDataAdapter
	{
		private const string SharePointDocumentLocationLogicalName = "sharepointdocumentlocation";
		private const string SharePointConnectionStringName = "SharePoint";
		private const string DefaultSortExpression = "FileLeafRef ASC";
		private const int DefaultPageSize = 10;

		private readonly IDataAdapterDependencies _dependencies;

		public SharePointDataAdapter(IDataAdapterDependencies dependencies)
		{
			_dependencies = dependencies;
		}

		public ISharePointResult AddFiles(EntityReference regarding, IList<HttpPostedFileBase> files, bool overwrite = true, string folderPath = null)
		{
			var context = _dependencies.GetServiceContextForWrite();
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var result = new SharePointResult(regarding, entityPermissionProvider, context);

			if (files == null || !files.Any()) return result;

			var entityMetadata = context.GetEntityMetadata(regarding.LogicalName);
			var entity = context.CreateQuery(regarding.LogicalName).First(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == regarding.Id);

			// assert permission to create the sharepointdocumentlocation entity
			if (!result.PermissionsExist || !result.CanCreate || !result.CanAppend || !result.CanAppendTo)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Create or Append document locations or AppendTo the regarding entity.");
				return result;
			}

			var spConnection = new SharePointConnection(SharePointConnectionStringName);
			var spSite = context.GetSharePointSiteFromUrl(spConnection.Url);

			var location = GetDocumentLocation(context, entity, entityMetadata, spSite);

			// assert permission to write the sharepointdocumentlocation entity
			if (!result.CanWrite)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Write document locations.");
				return result;
			}

			var factory = new ClientFactory();

			using (var client = factory.CreateClientContext(spConnection))
			{
				// retrieve the SharePoint list and folder names for the document location
				string listUrl, folderUrl;

				context.GetDocumentLocationListAndFolder(location, out listUrl, out folderUrl);

				var folder = client.AddOrGetExistingFolder(listUrl, folderUrl + folderPath);

				foreach (var postedFile in files)
				{
					using (var file = postedFile.InputStream)
					{
						// upload a file to the folder
						client.SaveFile(file, folder, Path.GetFileName(postedFile.FileName), overwrite);
					}
				}
			}

			return result;
		}

		public ISharePointResult AddFolder(EntityReference regarding, string name, string folderPath = null)
		{
			var context = _dependencies.GetServiceContextForWrite();
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var result = new SharePointResult(regarding, entityPermissionProvider, context);

			if (string.IsNullOrWhiteSpace(name)) return result;

			// Throw exception if the name begins or ends with a dot, contains consecutive dots,
			// or any of the following invalid characters ~ " # % & * : < > ? / \ { | }
			if (Regex.IsMatch(name, @"(\.{2,})|([\~\""\#\%\&\*\:\<\>\?\/\\\{\|\}])|(^\.)|(\.$)"))
			{
				throw new Exception("The folder name contains invalid characters. Please use a different name. Valid folder names can't begin or end with a period, can't contain consecutive periods, and can't contain any of the following characters: ~  # % & * : < > ? / \\ { | }.");
			}

			var entityMetadata = context.GetEntityMetadata(regarding.LogicalName);
			var entity = context.CreateQuery(regarding.LogicalName).First(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == regarding.Id);

			// assert permission to create the sharepointdocumentlocation entity
			if (!result.PermissionsExist || !result.CanCreate || !result.CanAppend || !result.CanAppendTo)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Create or Append document locations or AppendTo the regarding entity.");
				return result;
			}

			var spConnection = new SharePointConnection(SharePointConnectionStringName);
			var spSite = context.GetSharePointSiteFromUrl(spConnection.Url);

			var location = GetDocumentLocation(context, entity, entityMetadata, spSite);

			// assert permission to write the sharepointdocumentlocation entity
			if (!result.CanWrite)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Write document locations.");
				return result;
			}

			var factory = new ClientFactory();

			using (var client = factory.CreateClientContext(spConnection))
			{
				// retrieve the SharePoint list and folder names for the document location
				string listUrl, folderUrl;

				context.GetDocumentLocationListAndFolder(location, out listUrl, out folderUrl);

				client.AddOrGetExistingFolder(listUrl, "{0}{1}/{2}".FormatWith(folderUrl, folderPath, name));
			}

			return result;
		}

		public ISharePointResult DeleteItem(EntityReference regarding, int id)
		{
			var context = _dependencies.GetServiceContextForWrite();
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var result = new SharePointResult(regarding, entityPermissionProvider, context);

			// assert permission to delete the sharepointdocumentlocation entity
			if (!result.PermissionsExist || !result.CanDelete)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Create or Append document locations or AppendTo the regarding entity.");
				return result;
			}

			var entityMetadata = context.GetEntityMetadata(regarding.LogicalName);
			var entity = context.CreateQuery(regarding.LogicalName).First(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == regarding.Id);

			var spConnection = new SharePointConnection(SharePointConnectionStringName);
			var spSite = context.GetSharePointSiteFromUrl(spConnection.Url);

			var location = GetDocumentLocation(context, entity, entityMetadata, spSite);

			var factory = new ClientFactory();

			using (var client = factory.CreateClientContext(spConnection))
			{
				// retrieve the SharePoint list and folder names for the document location
				string listUrl, folderUrl;

				context.GetDocumentLocationListAndFolder(location, out listUrl, out folderUrl);

				var list = client.GetListByUrl(listUrl);
				var item = list.GetItemById(id);
				item.DeleteObject();

				client.ExecuteQuery();
			}

			return result;
		}

		public ISharePointCollection GetFoldersAndFiles(EntityReference regarding, string sortExpression = DefaultSortExpression, int page = 1, int pageSize = DefaultPageSize, string pagingInfo = null, string folderPath = null)
		{
			var context = _dependencies.GetServiceContextForWrite();
			var website = _dependencies.GetWebsite();
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var result = new SharePointResult(regarding, entityPermissionProvider, context);

			// assert permission to create the sharepointdocumentlocation entity
			if (!result.PermissionsExist || !result.CanCreate || !result.CanAppend || !result.CanAppendTo)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Create or Append document locations or AppendTo the regarding entity.");
				return SharePointCollection.Empty(true);
			}

			var entityMetadata = context.GetEntityMetadata(regarding.LogicalName);
			var entity = context.CreateQuery(regarding.LogicalName).First(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == regarding.Id);

			var spConnection = new SharePointConnection(SharePointConnectionStringName);
			var spSite = context.GetSharePointSiteFromUrl(spConnection.Url);

			var location = GetDocumentLocation(context, entity, entityMetadata, spSite);

			if (!entityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Read, location))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Read document locations.");
				return SharePointCollection.Empty(true);
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Read SharePoint Document Location Permission Granted.");

			var factory = new ClientFactory();

			using (var client = factory.CreateClientContext(spConnection))
			{
				// retrieve the SharePoint list and folder names for the document location
				string listUrl, folderUrl;

				context.GetDocumentLocationListAndFolder(location, out listUrl, out folderUrl);

				var sharePointFolder = client.AddOrGetExistingFolder(listUrl, folderUrl + folderPath);

				var list = client.GetListByUrl(listUrl);

				if (!sharePointFolder.IsPropertyAvailable("ServerRelativeUrl"))
				{
					client.Load(sharePointFolder, folder => folder.ServerRelativeUrl);
				}

				if (!sharePointFolder.IsPropertyAvailable("ItemCount"))
				{
					client.Load(sharePointFolder, folder => folder.ItemCount);
				}

				var camlQuery = new CamlQuery
				{
					FolderServerRelativeUrl = sharePointFolder.ServerRelativeUrl,
					ViewXml = @"
					<View>
						<Query>
							<OrderBy>
								<FieldRef {0}></FieldRef>
							</OrderBy>
							<Where></Where>
						</Query>
						<RowLimit>{1}</RowLimit>
					</View>".FormatWith(
							ConvertSortExpressionToCaml(sortExpression),
							pageSize)
				};
				if (page > 1)
				{
					camlQuery.ListItemCollectionPosition = new ListItemCollectionPosition { PagingInfo = pagingInfo };
				}
				var folderItems = list.GetItems(camlQuery);
				client.Load(folderItems,
					items => items.ListItemCollectionPosition,
					items => items.Include(
						item => item.ContentType,
						item => item["ID"],
						item => item["FileLeafRef"],
						item => item["Created"],
						item => item["Modified"],
						item => item["FileSizeDisplay"]));
				client.ExecuteQuery();

				var sharePointItems = new List<SharePointItem>();

				if (!string.IsNullOrEmpty(folderPath) && folderPath.Contains("/"))
				{
					var relativePaths = folderPath.Split('/');
					var parentFolderName = relativePaths.Length > 2 ? relativePaths.Skip(relativePaths.Length - 2).First() : "/";

					sharePointItems.Add(new SharePointItem()
					{
						Name = @"""{0}""".FormatWith(parentFolderName),
						IsFolder = true,
						FolderPath = folderPath.Substring(0, folderPath.LastIndexOf('/')),
						IsParent = true
					});
				}

				if (folderItems.Count < 1)
				{
					return new SharePointCollection(sharePointItems, null, sharePointItems.Count());
				}

				foreach (var item in folderItems)
				{
					var id = item["ID"] as int?;
					var name = item["FileLeafRef"] as string;
					var created = item["Created"] as DateTime?;
					var modified = item["Modified"] as DateTime?;

					long longFileSize;
					var fileSize = long.TryParse(item["FileSizeDisplay"] as string, out longFileSize) ? longFileSize : null as long?;

					if (string.Equals(item.ContentType.Name, "Folder", StringComparison.InvariantCultureIgnoreCase))
					{
						sharePointItems.Add(new SharePointItem
						{
							Id = id,
							Name = name,
							IsFolder = true,
							CreatedOn = created,
							ModifiedOn = modified,
							FolderPath = "{0}/{1}".FormatWith(folderPath, name)
						});
					}
					else
					{
						sharePointItems.Add(new SharePointItem
						{
							Id = id,
							Name = name,
							CreatedOn = created,
							ModifiedOn = modified,
							FileSize = fileSize,
							Url = GetAbsolutePath(website, location, name, folderPath)
						});
					}
				}

				var pageInfo = folderItems.ListItemCollectionPosition != null
					? folderItems.ListItemCollectionPosition.PagingInfo
					: null;

				return new SharePointCollection(sharePointItems, pageInfo, sharePointFolder.ItemCount);
			}
		}

		private static string GetAbsolutePath(EntityReference website, Entity entity, string fileName, string folderPath = null)
		{
			var virtualPath = website == null
				? RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName,
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", entity.LogicalName },
						{ "id", entity.Id },
						{ "file", fileName },
						{ "folderPath", folderPath }
					})
				: RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName + "PortalScoped",
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", entity.LogicalName },
						{ "id", entity.Id },
						{ "__portalScopeId__", website.Id },
						{ "file", fileName },
						{ "folderPath", folderPath }
					});

			var absolutePath = virtualPath == null
				? null
				: VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath);
			return absolutePath;
		}

		private static string ConvertSortExpressionToCaml(string sortExpression)
		{
			if (string.IsNullOrWhiteSpace(sortExpression)) throw new ArgumentNullException("sortExpression");

			var sort = sortExpression.Trim();
			var sortAsc = !sort.EndsWith(" DESC", StringComparison.InvariantCultureIgnoreCase);
			var sortBy = sort.Split(' ').First();

			return @"Name=""{0}"" Ascending=""{1}""".FormatWith(sortBy, sortAsc.ToString().ToUpperInvariant());
		}

		private static Entity GetDocumentLocation(OrganizationServiceContext context, Entity entity, EntityMetadata entityMetadata, Entity spSite)
		{
			var locations = context.CreateQuery(SharePointDocumentLocationLogicalName)
				.Where(docLoc => docLoc.GetAttributeValue<EntityReference>("regardingobjectid").Id == entity.Id && docLoc.GetAttributeValue<int>("statecode") == 0)
				.OrderBy(docLoc => docLoc.GetAttributeValue<DateTime>("createdon"))
				.ToArray();

			Entity location;

			if (locations.Count() > 1)
			{
				// Multiple doc locations found, choose the first created.
				location = locations.First();
			}
			else if (locations.Count() == 1)
			{
				location = locations.First();
			}
			else
			{
				// No document locations found, create an auto-generated one.
				var autoGeneratedRelativeUrl = "{0}_{1}".FormatWith(
					entity.GetAttributeValue<string>(entityMetadata.PrimaryNameAttribute),
					entity.Id.ToString("N").ToUpper());

				location = context.AddOrGetExistingDocumentLocationAndSave<Entity>(spSite, entity, autoGeneratedRelativeUrl);
			}

			if (location == null)
			{
				throw new Exception("A document location couldn't be found or created for the entity.");
			}

			return location;
		}
	}
}
