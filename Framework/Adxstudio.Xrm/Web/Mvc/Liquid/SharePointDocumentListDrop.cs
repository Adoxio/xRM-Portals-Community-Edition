/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Adxstudio.SharePoint;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.SharePoint;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SharePointDocumentListDrop : PortalDrop
	{
		private readonly string _entityLogicalName;
		private readonly string _folderName;

		public SharePointDocumentListDrop(IPortalLiquidContext portalLiquidContext, string entityLogicalName, string folderName) : base(portalLiquidContext)
		{
			if (entityLogicalName == null) throw new ArgumentNullException("entityLogicalName");

			_entityLogicalName = entityLogicalName;
			_folderName = folderName;
		}

		public string LogicalName
		{
			get { return _entityLogicalName; }
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				try
				{
					Entity location;

					Guid id;

					if (!Guid.TryParse(method, out id))
					{
						return null;
					}

					if (!string.IsNullOrWhiteSpace(_folderName))
					{
						// Get documents based on provided folder name.

						var entity = new Entity(LogicalName) { Id = id };

						location = GetDocumentLocation(serviceContext, entity, _folderName);
					}
					else
					{
						// Get documents based on "primary attribute_id" folder.

						var metadata = serviceContext.GetEntityMetadata(LogicalName, EntityFilters.Attributes);

						location = GetDocumentLocation(serviceContext, metadata, id);
					}

					if (location == null)
					{
						return null;
					}

					var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

					if (!crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Read, location))
					{
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Read document locations.");

						return null;
					}

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Read SharePoint Document Location Permission Granted.");

					var spConnection = new SharePointConnection("SharePoint");

					var factory = new ClientFactory();

					using (var client = factory.CreateClientContext(spConnection))
					{
						// retrieve the SharePoint list and folder names for the document location
						string listName, folderName;

						serviceContext.GetDocumentLocationListAndFolder(location, out listName, out folderName);

						var folder = client.AddOrGetExistingFolder(listName, folderName);

						var fileCollection = folder.Files;
						client.Load(folder.Files);
						client.ExecuteQuery();

						var files = fileCollection.ToArray().OrderBy(file => file.Name);

						if (!files.Any())
						{
							return null;
						}

						return files.Select(file => new SharePointDocumentDrop(this, file, location));
					}
				}
				catch (FaultException<OrganizationServiceFault>)
				{
					return null;
				}
			}
		}

		private Entity GetDocumentLocation(OrganizationServiceContext context, EntityMetadata metadata, Guid entityId)
		{
			var response = (RetrieveResponse)context.Execute(new RetrieveRequest
			{
				Target = new EntityReference(LogicalName, entityId),
				ColumnSet = new ColumnSet(metadata.PrimaryNameAttribute),
			});

			var regarding = response.Entity;

			if (regarding == null)
			{
				return null;
			}

			return GetDocumentLocation(context, regarding,
				"{0}_{1}".FormatWith(regarding.GetAttributeValue<string>(metadata.PrimaryNameAttribute),
					regarding.Id.ToString("N").ToUpper()));
		}

		private static Entity GetDocumentLocation(OrganizationServiceContext context, Entity entity, string folderName)
		{
			var spConnection = new SharePointConnection("SharePoint");
			var spSite = context.GetSharePointSiteFromUrl(spConnection.Url);

			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var result = new SharePointResult(entity.ToEntityReference(), entityPermissionProvider, context);

			if (!result.PermissionsExist || !result.CanCreate || !result.CanAppend || !result.CanAppendTo)
			{
				return null;
			}

			return context.AddOrGetExistingDocumentLocationAndSave<Entity>(spSite, entity, folderName);
		}
	}
}
