/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Defines the layout of a view for a subgrid
	/// </summary>
	public class SubgridViewLayout : ViewLayout
	{
		/// <summary>
		/// <see cref="EntityReference"/> of the source entity that the subgrid records are related to.
		/// </summary>
		public EntityReference Source { get; private set; }

		/// <summary>
		/// The <see cref="Relationship"/> between the source entity and the subgrid records.
		/// </summary>
		public Relationship Relationship { get; private set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public SubgridViewLayout()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public SubgridViewLayout(ViewConfiguration configuration, EntityReference source, Relationship relationship, string viewEntityLogicalName,
			EntityView view = null, string portalName = null,
			int languageCode = 0, bool addSelectColumn = false, bool addActionsColumn = false, string selectColumnHeaderText = "")
			: base(configuration, view, portalName, languageCode, addSelectColumn, addActionsColumn, selectColumnHeaderText)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (relationship == null)
			{
				throw new ArgumentNullException("relationship");
			}

			if (string.IsNullOrWhiteSpace(viewEntityLogicalName))
			{
				throw new ArgumentNullException("viewEntityLogicalName");
			}

			Source = source;
			Relationship = relationship;

			if ((configuration.EnableEntityPermissions && AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled) &&
				configuration.AssociateActionLink.Enabled)
			{
				var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(configuration.PortalName);
				var crmEntityPermissionProvider = new CrmEntityPermissionProvider(configuration.PortalName);

				configuration.AssociateActionLink.Enabled =
					crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.AppendTo,
						Retrieve(serviceContext, source)) &&
					crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Append, viewEntityLogicalName);
			}
		}

		private static Entity Retrieve(OrganizationServiceContext serviceContext, EntityReference target)
		{
			var request = new RetrieveRequest { Target = target, ColumnSet = new ColumnSet(true) };
			var response = serviceContext.Execute(request) as RetrieveResponse;
			return response == null ? null : response.Entity;
		}
	}
}
