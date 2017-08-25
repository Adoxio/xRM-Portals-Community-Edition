/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Notes
{
	public class AnnotationCreateResult : IAnnotationResult
	{
		public AnnotationCreateResult(CrmEntityPermissionProvider provider, OrganizationServiceContext context, EntityReference regarding = null)
		{
			// To create and append a note to regarding object we need to test the following rights
			CanCreate = provider.TryAssert(context, CrmEntityPermissionRight.Create, "annotation", regarding);
			CanAppend = provider.TryAssert(context, CrmEntityPermissionRight.Append, "annotation", regarding);
			CanAppendTo = provider.TryAssert(context, CrmEntityPermissionRight.AppendTo, regarding);
			PermissionsExist = provider.PermissionsExist;
			PermissionGranted = CanCreate && CanAppend && CanAppendTo;
		}

		public IAnnotation Annotation { get; set; }

		public bool CanAppend { get; private set; }

		public bool CanAppendTo { get; private set; }

		public bool CanCreate { get; private set; }

		public bool PermissionsExist { get; private set; }

		public bool PermissionGranted { get; private set; }
	}

	public class AnnotationUpdateResult : IAnnotationResult
	{
		public AnnotationUpdateResult(IAnnotation note, CrmEntityPermissionProvider provider, OrganizationServiceContext context, EntityMetadata entityMetadata = null)
		{
			Annotation = note;
			if (note.Entity == null) return;
			PermissionsExist = provider.PermissionsExist;
			PermissionGranted = provider.TryAssert(context, CrmEntityPermissionRight.Write, note.Entity, entityMetadata, regarding: note.Regarding);
		}

		public IAnnotation Annotation { get; private set; }

		public bool PermissionsExist { get; private set; }

		public bool PermissionGranted { get; private set; }
	}

	public class AnnotationDeleteResult : IAnnotationResult
	{
		public AnnotationDeleteResult(IAnnotation note, CrmEntityPermissionProvider provider, OrganizationServiceContext context, EntityMetadata entityMetadata = null)
		{
			Annotation = note;
			if (note.Entity == null) return;
			PermissionsExist = provider.PermissionsExist;
			PermissionGranted = provider.TryAssert(context, CrmEntityPermissionRight.Delete, note.Entity, entityMetadata, regarding: note.Regarding);
		}

		public IAnnotation Annotation { get; private set; }

		public bool PermissionsExist { get; private set; }

		public bool PermissionGranted { get; private set; }
	}
}
