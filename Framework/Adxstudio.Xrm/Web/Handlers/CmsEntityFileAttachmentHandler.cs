/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Providers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityFileAttachmentHandler : CmsEntityHandler
	{
		public CmsEntityFileAttachmentHandler() { }

		public CmsEntityFileAttachmentHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id) : base(portalName, portalScopeId, entityLogicalName, id) { }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			if (!IsRequestMethod(context.Request, "POST"))
			{
				throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
			}
			
			var dataAdapterDependencies =
				new PortalConfigurationDataAdapterDependencies(requestContext: context.Request.RequestContext,
					portalName: PortalName);
			var annotationDataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var website = context.GetWebsite();

			var location = website.Settings.Get<string>("WebFiles/StorageLocation");
			StorageLocation storageLocation;
			if (!Enum.TryParse(location, true, out storageLocation))
			{
				storageLocation = StorageLocation.CrmDocument;
			}

			var maxFileSizeErrorMessage = website.Settings.Get<string>("WebFiles/MaxFileSizeErrorMessage");

			var annotationSettings = new AnnotationSettings(dataAdapterDependencies.GetServiceContext(),
				storageLocation: storageLocation, maxFileSizeErrorMessage: maxFileSizeErrorMessage);

			var files = context.Request.Files;
			var postedFiles = new List<HttpPostedFile>();

			for (var i = 0; i < files.Count; i++)
			{
				postedFiles.Add(files[i]);
			}

			foreach (var file in postedFiles)
			{
				annotationDataAdapter.CreateAnnotation(new Annotation
				{
					Regarding = entity.ToEntityReference(),
					FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(file), annotationSettings.StorageLocation)
				}, annotationSettings);
			}

			context.Response.ContentType = "text/plain";
			context.Response.Write("OK");
		}
	}
}
