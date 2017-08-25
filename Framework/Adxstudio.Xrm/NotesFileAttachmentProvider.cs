/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm
{
	internal class NotesFileAttachmentProvider : ICrmEntityFileAttachmentProvider
	{
		public string PortalName { get; private set; }

		public NotesFileAttachmentProvider(string portalName)
		{
			PortalName = portalName;
		}

		public virtual void AttachFile(OrganizationServiceContext context, Entity entity, HttpPostedFile postedFile)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (postedFile == null)
			{
				throw new ArgumentNullException("postedFile");
			}

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			securityProvider.Assert(context, entity, CrmEntityRight.Change);
			
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: HttpContext.Current.Request.RequestContext, portalName: PortalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var result = dataAdapter.CreateAnnotation(entity.ToEntityReference(), string.Empty, string.Empty,
				new HttpPostedFileWrapper(postedFile));
			if (result.Annotation.Entity.Id != null)
			{
				throw new InvalidOperationException("The file couldn't be attached to entity {0}.".FormatWith(entity));
			}
		}

		public virtual IEnumerable<ICrmEntityAttachmentInfo> GetAttachmentInfo(OrganizationServiceContext context, Entity entity)
		{
			var notes = context.GetNotes(entity);

			if (notes == null)
			{
				return new List<ICrmEntityAttachmentInfo>();
			}

			return notes
				.Select(note => new { Url = note.GetRewriteUrl(), LastModified = note.GetAttributeValue<DateTime?>("modifiedon"), CreatedOn = note.GetAttributeValue<DateTime?>("createdon") })
				.Where(e => !string.IsNullOrEmpty(e.Url) && e.CreatedOn.HasValue)
				.OrderByDescending(e => e.CreatedOn)
				.Select(e => new EntityAttachmentInfo(e.Url, e.LastModified))
				.ToList();
		}

		public class EntityAttachmentInfo : ICrmEntityAttachmentInfo
		{
			public EntityAttachmentInfo(string url, DateTime? lastModified)
			{
				if (string.IsNullOrWhiteSpace(url))
				{
					throw new ArgumentException("Value can't be null or whitespace.", "url");
				}

				Url = url;
				LastModified = lastModified;
			}

			public DateTime? LastModified { get; private set; }

			public string Url { get; private set; }
		}
	}
}
