/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Controls
{
	public class PortalUserControl : PortalViewUserControl
	{
		private readonly Lazy<OrganizationServiceContext> _xrmContext;

		public PortalUserControl()
		{
			_xrmContext = new Lazy<OrganizationServiceContext>(CreateXrmServiceContext);
		}

		public OrganizationServiceContext XrmContext
		{
			get { return _xrmContext.Value; }
		}

		public IPortalContext Portal
		{
			get { return PortalCrmConfigurationManager.CreatePortalContext(PortalName); }
		}

		public OrganizationServiceContext ServiceContext
		{
			get { return Portal.ServiceContext; }
		}

		public Entity Website
		{
			get { return Portal.Website; }
		}

		public Entity Contact
		{
			get { return Portal.User; }
		}

		public Entity Entity
		{
			get { return Portal.Entity; }
		}

		protected void RedirectToLoginIfAnonymous()
		{
			if (!Request.IsAuthenticated)
			{
				Response.ForbiddenAndEndResponse();
			}
		}

		private OrganizationServiceContext CreateXrmServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}
	}
}
