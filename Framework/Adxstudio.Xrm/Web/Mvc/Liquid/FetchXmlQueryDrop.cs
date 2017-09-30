/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class FetchXmlQueryDrop : PortalDrop
	{
		private readonly Lazy<Fetch> _fetch;
		private readonly CrmEntityPermissionProvider.EntityPermissionRightResult _permissionRightResult;
		private readonly Lazy<EntityCollectionDrop> _results;
		private readonly Lazy<string> _xml;

		public FetchXmlQueryDrop(IPortalLiquidContext portalLiquidContext, Lazy<Fetch> fetch, CrmEntityPermissionProvider.EntityPermissionRightResult permissionRightResult = null) : base(portalLiquidContext)
		{
			if (fetch == null) throw new ArgumentNullException("fetch");

			_fetch = fetch;
			_permissionRightResult = permissionRightResult;
			_results = new Lazy<EntityCollectionDrop>(GetResults, LazyThreadSafetyMode.None);
			_xml = new Lazy<string>(GetXml, LazyThreadSafetyMode.None);
		}

		public FetchXmlQueryDrop(IPortalLiquidContext portalLiquidContext, Fetch fetch, CrmEntityPermissionProvider.EntityPermissionRightResult permissionRightResult = null)
			: this(portalLiquidContext, new Lazy<Fetch>(() => fetch, LazyThreadSafetyMode.None), permissionRightResult) { }

		public FetchXmlQueryDrop(IPortalLiquidContext portalLiquidContext, string fetchXml, CrmEntityPermissionProvider.EntityPermissionRightResult permissionRightResult = null)
			: this(portalLiquidContext, Fetch.Parse(fetchXml), permissionRightResult) { }

		public bool GlobalPermissionGranted
		{
			get { return _permissionRightResult != null && _permissionRightResult.GlobalPermissionGranted; }
		}

		public bool PermissionGranted
		{
			get { return _permissionRightResult != null && _permissionRightResult.PermissionGranted; }
		}

		public EntityCollectionDrop Results
		{
			get { return _results.Value; }
		}

		public bool RulesExist
		{
			get { return _permissionRightResult != null && _permissionRightResult.RulesExist; }
		}

		public string Xml
		{
			get { return _xml.Value; }
		}

		private EntityCollectionDrop GetResults()
		{
			if (_permissionRightResult != null && !(_permissionRightResult.GlobalPermissionGranted || _permissionRightResult.PermissionGranted))
			{
				return new EntityCollectionDrop(this, new EntityCollection());
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(_fetch.Value.ToRetrieveMultipleRequest());

				return new EntityCollectionDrop(this, response.EntityCollection);
			}
		}

		private string GetXml()
		{
			return _fetch.Value.ToXml().ToString();
		}
	}
}
