/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	public interface ICrmEntityFileAttachmentProvider
	{
		void AttachFile(OrganizationServiceContext context, Entity entity, HttpPostedFile postedFile);

		IEnumerable<ICrmEntityAttachmentInfo> GetAttachmentInfo(OrganizationServiceContext context, Entity entity);
	}
}
