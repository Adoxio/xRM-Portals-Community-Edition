/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	internal sealed class NotesFileAttachmentProvider : ICrmEntityFileAttachmentProvider
	{
		public string PortalName { get; private set; }

		public NotesFileAttachmentProvider(string portalName)
		{
			PortalName = portalName;
		}

		public void AttachFile(OrganizationServiceContext context, Entity entity, HttpPostedFile postedFile)
		{
			context.ThrowOnNull("context");
			entity.ThrowOnNull("entity");
			postedFile.ThrowOnNull("postedFile");

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			securityProvider.Assert(context, entity, CrmEntityRight.Change);

			if (!context.AddNoteAndSave(entity, string.Empty, string.Empty, postedFile))
			{
				throw new InvalidOperationException("Failed to attach file to entity {0}.".FormatWith(entity));
			}
		}

		public IEnumerable<ICrmEntityAttachmentInfo> GetAttachmentInfo(OrganizationServiceContext context, Entity entity)
		{
			var note = context.GetNote(entity);

			if (note == null)
			{
				return new List<ICrmEntityAttachmentInfo>();
			}

			var noteUrl = note.GetRewriteUrl();

			if (string.IsNullOrEmpty(noteUrl))
			{
				return new List<ICrmEntityAttachmentInfo>();
			}

			return new[]
			{
				new CrmEntityAttachmentInfo(noteUrl, note.GetAttributeValue<DateTime?>("modifiedon"))
			};
		}
	}
}
