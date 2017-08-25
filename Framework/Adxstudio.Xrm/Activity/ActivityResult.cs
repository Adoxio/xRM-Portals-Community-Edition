/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Activity
{
	public class PortalCommentCreateResult
	{
		private const string entityName = "adx_portalcomment";
		public PortalCommentCreateResult(CrmEntityPermissionProvider provider, OrganizationServiceContext context, EntityReference regarding = null)
		{
			// To create and append a note to regarding object we need to test the following rights
			CanCreate = provider.TryAssert(context, CrmEntityPermissionRight.Create, entityName, regarding);
			CanAppend = provider.TryAssert(context, CrmEntityPermissionRight.Append, entityName, regarding);
			CanAppendTo = provider.TryAssert(context, CrmEntityPermissionRight.AppendTo, regarding);
			PermissionsExist = provider.PermissionsExist;
			PermissionGranted = CanCreate && CanAppend && CanAppendTo;
		}

		public IPortalComment PortalComment { get; set; }

		public bool CanAppend { get; private set; }

		public bool CanAppendTo { get; private set; }

		public bool CanCreate { get; private set; }

		public bool PermissionsExist { get; private set; }

		public bool PermissionGranted { get; private set; }
	}
}
