/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.EntityList.OData
{
	/// <summary>
	/// Implentation of <see cref="IDataAdapterDependencies"/>
	/// </summary>
	public abstract class DataAdapterDependencies : IDataAdapterDependencies
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly EntityReference _website;

		protected DataAdapterDependencies(OrganizationServiceContext serviceContext, EntityReference website)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			_serviceContext = serviceContext;
			_website = website;
		}

		protected string PortalName { get; set; }

		public OrganizationServiceContext GetServiceContext()
		{
			return _serviceContext;
		}

		public EntityReference GetWebsite()
		{
			return _website;
		}
	}
}
