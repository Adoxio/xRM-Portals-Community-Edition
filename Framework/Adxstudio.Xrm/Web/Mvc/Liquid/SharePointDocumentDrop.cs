/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SharePointDocumentDrop : PortalDrop
	{
		private readonly long _length;
		private readonly string _name;
		private readonly DateTime _timeCreated;
		private readonly DateTime _timeLastModified;
		private readonly string _title;
		private readonly string _url;

		public SharePointDocumentDrop(IPortalLiquidContext portalLiquidContext, File file, Entity documentLocation) : base(portalLiquidContext)
		{
			if (file == null) throw new ArgumentNullException("file");

			_name = file.Name;
			_length = file.Length;
			_timeCreated = file.TimeCreated;
			_timeLastModified = file.TimeLastModified;
			_title = file.Title;


			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var website = portalContext.Website;

			var virtualPath = website == null
				? RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName,
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", documentLocation.LogicalName },
						{ "id", documentLocation.Id },
						{ "file", file.Name }
					})
				: RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName + "PortalScoped",
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", documentLocation.LogicalName },
						{ "id", documentLocation.Id },
						{ "__portalScopeId__", website.Id },
						{ "file", file.Name }
					});

			_url = virtualPath == null
				? null
				: VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath);
		}

		public virtual long Length
		{
			get { return _length; }
		}

		public virtual string Name
		{
			get { return _name; }
		}

		public virtual DateTime TimeCreated
		{
			get { return _timeCreated; }
		}

		public virtual DateTime TimeLastModified
		{
			get { return _timeLastModified; }
		}

		public virtual string Title
		{
			get { return _title; }
		}

		public virtual string Url
		{
			get { return _url; }
		}
	}
}
