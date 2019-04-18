/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	internal class CloudBlobRedirectHandler : IHttpHandler
	{
		private readonly string _blobAddress;
		private readonly bool _enableTracking;
		private readonly EntityReference _entity;

		public CloudBlobRedirectHandler(Entity entity, string portalName = null)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			PortalName = portalName;

			_blobAddress = entity.GetAttributeValue<string>("adx_cloudblobaddress");
			_entity = entity.ToEntityReference();
			_enableTracking = entity.GetAttributeValue<bool?>("adx_enabletracking").GetValueOrDefault();
		}

		protected string PortalName { get; private set; }

		public void ProcessRequest(HttpContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (_blobAddress == null)
			{
				context.Response.StatusCode = 404;
				context.Response.ContentType = "text/plain";
				context.Response.Write(ResourceManager.GetString("Not_Found_Exception"));

				return;
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(PortalName, context.Request.RequestContext);

			CloudStorageAccount storageAccount;

			if (!TryGetCloudStorageAccount(context, out storageAccount))
			{
				context.Response.StatusCode = 404;
				context.Response.ContentType = "text/plain";
				context.Response.Write(ResourceManager.GetString("Failed_To_Configure_Cloud_Storage_Account"));

				return;
			}

			if (_enableTracking)
			{
				var log = new Entity("adx_webfilelog");

				log["adx_name"] = _blobAddress;
				log["adx_date"] = DateTime.UtcNow;
				log["adx_ipaddress"] = context.Request.UserHostAddress;
				log["adx_webfileid"] = _entity;

				var user = dataAdapterDependencies.GetPortalUser();
				
				if (user != null && user.LogicalName == "contact")
				{
					log["adx_contactid"] = user;
				}

				var serviceContext = dataAdapterDependencies.GetServiceContextForWrite();

				serviceContext.AddObject(log);
				serviceContext.SaveChanges();
			}

			var blobClient = storageAccount.CreateCloudBlobClient();
			var blob = blobClient.GetBlobReferenceFromServer(new Uri(blobClient.BaseUri + _blobAddress));

			var accessSignature = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
			{
				Permissions = SharedAccessBlobPermissions.Read,
				SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(55)
			});
			
			context.Response.Redirect(blob.Uri + accessSignature);
		}

		public bool IsReusable
		{
			get { return false; }
		}

		protected virtual bool TryGetCloudStorageAccount(HttpContext context, out CloudStorageAccount storageAccount)
		{
			storageAccount = null;
			var website = context.GetWebsite();
			var settingValue = website.Settings.Get<string>("WebFiles/CloudStorageAccount");

			if (!string.IsNullOrEmpty(settingValue) && CloudStorageAccount.TryParse(settingValue, out storageAccount))
			{
				return true;
			}

			const string configurationKey = "Adxstudio.Xrm.Cms.WebFiles.CloudStorageAccount";

			try
			{
				storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings.Get(configurationKey));

				return storageAccount != null;
			}
			catch (InvalidOperationException)
			{
				var appSetting = ConfigurationManager.AppSettings[configurationKey];

				return !string.IsNullOrEmpty(appSetting) && CloudStorageAccount.TryParse(appSetting, out storageAccount);
			}
		}

		public static bool IsCloudBlob(Entity entity)
		{
			return entity != null && !string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_cloudblobaddress"));
		}

		public static bool TryGetCloudBlobHandler(Entity entity, out IHttpHandler handler, string portalName = null)
		{
			if (IsCloudBlob(entity))
			{
				handler = new CloudBlobRedirectHandler(entity, portalName);

				return true;
			}

			handler = null;

			return false;
		}
	}
}
