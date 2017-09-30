/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityNoteDrop : EntityDrop
	{
		private readonly Lazy<string> _documentbody;
		private readonly Lazy<string> _url;

		public EntityNoteDrop(IPortalLiquidContext portalLiquidContext, Entity entity) : base(portalLiquidContext, entity)
		{
			_documentbody = new Lazy<string>(GetDocumentBody, LazyThreadSafetyMode.None);
			_url = new Lazy<string>(GetUrl, LazyThreadSafetyMode.None);
		}

		public override string Url
		{
			get { return _url.Value; }
		}

		public override object BeforeMethod(string method)
		{
			return string.Equals(method, "documentbody", StringComparison.InvariantCulture)
				? _documentbody.Value
				: base.BeforeMethod(method);
		}

		private string GetDocumentBody()
		{
			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest
				{
					Target = EntityReference,
					ColumnSet = new ColumnSet("documentbody")
				});

				return response.Entity.GetAttributeValue<string>("documentbody");
			}
		}

		private string GetUrl()
		{
			var virtualPath = RouteTable.Routes.GetVirtualPath(Html.ViewContext.RequestContext, typeof(EntityRouteHandler).FullName, new RouteValueDictionary
			{
				{ "prefix", "_entity" },
				{ "logicalName", LogicalName },
				{ "id", Id }
			});

			return virtualPath == null
				? null
				: VirtualPathUtility.ToAbsolute("~/" + virtualPath.VirtualPath);
		}
	}
}
