/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// Retrieves handlers based on entity relationship traversal.
	/// </summary>
	public class PortalRouteHandlerProvider : IPortalRouteHandlerProvider
	{
		public virtual string PortalName { get; private set; }

		public PortalRouteHandlerProvider(string portalName)
		{
			PortalName = portalName;
		}

		public virtual bool TryCreateHandler(IPortalContext portal, out IHttpHandler handler)
		{
			if (string.Equals(portal.Entity.LogicalName, "adx_webfile", StringComparison.InvariantCulture))
			{
				if (CloudBlobRedirectHandler.TryGetCloudBlobHandler(portal.Entity, out handler))
				{
					return true;
				}

				var contentNote = portal.ServiceContext.GetNotes(portal.Entity)
					.OrderByDescending(note => note.GetAttributeValue<DateTime?>("createdon"))
					.FirstOrDefault();

				handler = CreateAnnotationHandler(contentNote, portal.Entity);
				return true;
			}

			if (string.Equals(portal.Entity.LogicalName, "annotation", StringComparison.InvariantCulture))
			{
				handler = CreateAnnotationHandler(portal.Entity);
				return true;
			}

			if (string.Equals(portal.Entity.LogicalName, "salesliteratureitem", StringComparison.InvariantCulture))
			{
				handler = CreateSalesAttachmentHandler(portal.Entity);
				return true;
			}

			handler = null;
			return false;
		}

		protected virtual IHttpHandler CreateAnnotationHandler(Entity annotation, Entity webfile)
		{
			return new AnnotationHandler(annotation, webfile);
		}

		protected virtual IHttpHandler CreateAnnotationHandler(Entity entity)
		{
			return new AnnotationHandler(entity);
		}

		protected virtual IHttpHandler CreateSalesAttachmentHandler(Entity entity)
		{
			return new SalesAttachmentHandler(entity);
		}
	}
}
