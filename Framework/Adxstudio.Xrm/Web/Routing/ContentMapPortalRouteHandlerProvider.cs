/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// Retrieves handlers based on the content map entity cache.
	/// </summary>
	internal class ContentMapPortalRouteHandlerProvider : PortalRouteHandlerProvider
	{
		private readonly IContentMapProvider _contentMapProvider;

		public ContentMapPortalRouteHandlerProvider(string portalName, IContentMapProvider provder)
			: base(portalName)
		{
			_contentMapProvider = provder;
		}

		public override bool TryCreateHandler(IPortalContext portal, out IHttpHandler handler)
		{
			IHttpHandler local = null;
			var result = _contentMapProvider.Using(map => TryCreateHandler(portal, map, out local));
			handler = local;

			return result;
		}

		private bool TryCreateHandler(IPortalContext portal, ContentMap map, out IHttpHandler handler)
		{
			switch (portal.Entity.LogicalName)
			{
				case "adx_webfile":
					WebFileNode webfile;
					if (map.TryGetValue(portal.Entity, out webfile))
					{
						if (CloudBlobRedirectHandler.TryGetCloudBlobHandler(portal.Entity, out handler))
						{
							return true;
						}

						// retrieve the most recently created annotation
						var note = webfile.Annotations.OrderByDescending(a => a.CreatedOn).FirstOrDefault();
						return TryCreateHandler(note, webfile, portal, out handler);
					}
					break;
				case "annotation":
					AnnotationNode annotation;
					if (map.TryGetValue(portal.Entity, out annotation))
					{
						return TryCreateHandler(annotation, null, portal, out handler);
					}
					break;
			}

			return base.TryCreateHandler(portal, out handler);
		}

		protected virtual bool TryCreateHandler(AnnotationNode annotation, WebFileNode webfileNode, IPortalContext portal, out IHttpHandler handler)
		{
			if (annotation != null)
			{
				// convert to Entity object and populate the documentbody attribute value
				var portalOrgService = HttpContext.Current.GetOrganizationService();
				var entity = annotation.ToEntity();

				var body = portalOrgService.RetrieveSingle("annotation",
					new[] { "documentbody" },
					new Condition("annotationid", ConditionOperator.Equal, annotation.Id));

				entity.SetAttributeValue("documentbody", body);
				
				var webfile = webfileNode != null
					? portalOrgService.RetrieveSingle("adx_webfile",
						FetchAttribute.All,
						new Condition("adx_webfileid", ConditionOperator.Equal, webfileNode.Id))
					: null;

				handler = CreateAnnotationHandler(entity, webfile);
				return true;
			}

			handler = CreateAnnotationHandler(null);
			return true;
		}
	}
}
