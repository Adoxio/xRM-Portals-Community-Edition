/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Controls
{
	public class WebFormPortalUserControl : WebFormPortalViewUserControl
	{
		private readonly Lazy<OrganizationServiceContext> _xrmContext;

		public WebFormPortalUserControl()
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

		private OrganizationServiceContext CreateXrmServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		private enum WebFormStepMode
		{
			Insert = 100000000,
			Edit = 100000001,
			ReadOnly = 100000002
		}

		protected EntityReference GetTargetEntityReference()
		{
			if (CurrentStepEntityID != Guid.Empty)
			{
				return new EntityReference(CurrentStepEntityLogicalName, CurrentStepEntityID);
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var step = serviceContext.CreateQuery("adx_webformstep")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webformstepid") == WebForm.CurrentSessionHistory.CurrentStepId);

			if (step == null)
			{
				return null;
			}

			var mode = step.GetAttributeValue<OptionSetValue>("adx_mode");

			if (mode != null && mode.Value != (int)WebFormStepMode.Insert)
			{
				return null;
			}

			var entity = new Entity(CurrentStepEntityLogicalName);

			serviceContext.AddObject(entity);

			if (SetEntityReference && !string.IsNullOrEmpty(EntityReferenceTargetEntityName) && EntityReferenceTargetEntityID != Guid.Empty)
			{
				var populateLookupAttribute = step.GetAttributeValue<bool?>("adx_populateentityreferencelookupfield").GetValueOrDefault();
				var referenceLookupAttribute = step.GetAttributeValue<string>("adx_referencetargetlookupattributelogicalname");

				if (populateLookupAttribute && !string.IsNullOrWhiteSpace(referenceLookupAttribute))
				{
					entity[referenceLookupAttribute] = new EntityReference(EntityReferenceTargetEntityName, EntityReferenceTargetEntityID);
				}
				else if (!string.IsNullOrEmpty(EntityReferenceRelationshipName) && !string.IsNullOrEmpty(EntityReferenceTargetEntityPrimaryKeyName))
				{
					var source = serviceContext.CreateQuery(EntityReferenceTargetEntityName)
						.FirstOrDefault(e => e.GetAttributeValue<Guid>(EntityReferenceTargetEntityPrimaryKeyName) == EntityReferenceTargetEntityID);

					if (source != null)
					{
						serviceContext.AddLink(source, new Relationship(EntityReferenceRelationshipName), entity);
					}
				}
			}

			var associateCurrentPortalUser = step.GetAttributeValue<bool?>("adx_associatecurrentportaluser").GetValueOrDefault();
			var portalUserLookupAttribute = step.GetAttributeValue<string>("adx_targetentityportaluserlookupattribute");

			if (associateCurrentPortalUser && !string.IsNullOrEmpty(portalUserLookupAttribute) && Contact != null)
			{
				entity[portalUserLookupAttribute] = Contact.ToEntityReference();
			}

			serviceContext.SaveChanges();

			var reference = entity.ToEntityReference();

			UpdateEntityDefinition(new Adxstudio.Xrm.Web.UI.WebForms.WebFormEntitySourceDefinition(reference.LogicalName, CurrentStepEntityPrimaryKeyLogicalName, reference.Id));

			return reference;
		}
	}
}
