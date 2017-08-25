/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Notes
{
	public interface IAnnotationDataAdapter
	{
		IAnnotation GetAnnotation(Guid id);
		
		IAnnotation GetAnnotation(Entity entity);
		
		void Download(HttpContextBase context, Entity entity, Entity webfile);

		ActionResult DownloadAction(HttpResponseBase response, Entity entity);

		IAnnotationCollection GetAnnotations(EntityReference regarding, List<Order> orders, int page, int pageSize, AnnotationPrivacy privacy, EntityMetadata entityMetadata, bool respectPermissions);

		IAnnotationCollection GetDocuments(EntityReference regarding, bool respectPermissions, string webPrefix = null);

		IAnnotationResult CreateAnnotation(IAnnotation note, IAnnotationSettings settings = null);
		
		IAnnotationResult CreateAnnotation(EntityReference regarding, string subject, string noteText);
		
		IAnnotationResult CreateAnnotation(EntityReference regarding, string subject, string noteText, HttpPostedFileBase file);

		IAnnotationResult CreateAnnotation(EntityReference regarding, string subject, string noteText, string fileName, string contentType, byte[] content);

		IAnnotationResult UpdateAnnotation(IAnnotation note, IAnnotationSettings settings = null);

		IAnnotationResult DeleteAnnotation(IAnnotation note, IAnnotationSettings settings = null);
	}

	[Flags]
	public enum AnnotationPrivacy : short
	{
		Any = 0,
		Web = 1 << 0,
		Private = 1 << 1,
		Public = 1 << 2
	}
}
