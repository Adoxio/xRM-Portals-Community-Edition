/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	using Adxstudio.Xrm.Services;

	using System;
	using System.ServiceModel;

	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;

	public class EntitySetDrop : PortalDrop
	{
		private readonly string _entityLogicalName;

		public EntitySetDrop(IPortalLiquidContext portalLiquidContext, string entityLogicalName) : base(portalLiquidContext)
		{
			if (entityLogicalName == null) throw new ArgumentNullException("entityLogicalName");

			_entityLogicalName = entityLogicalName;
		}

		public string LogicalName
		{
			get { return _entityLogicalName; }
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			if (string.Equals(method, "logicalname", StringComparison.OrdinalIgnoreCase))
			{
				return LogicalName;
			}

			Guid id;

			if (!Guid.TryParse(method, out id))
			{
				return null;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				try
				{
					var entity = serviceContext.RetrieveSingle(new EntityReference(this.LogicalName, id), new ColumnSet(true));

					EntityDrop entityDrop = (entity == null) ? null : new EntityDrop(this, entity);
					return (entityDrop != null && entityDrop.Permissions.CanRead) ? entityDrop : null;
				}
				catch (FaultException<OrganizationServiceFault>)
				{
					return null;
				}
			}
		}
	}
}
