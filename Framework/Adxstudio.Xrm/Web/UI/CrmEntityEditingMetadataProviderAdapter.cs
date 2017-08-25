/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Implements an adapter between the legacy <see cref="ICrmEntityEditingMetadataProvider"/> interface, and
	/// the newer <see cref="ICmsEntityEditingMetadataProvider"/> interface. The old interface was dependant on
	/// web controls, while the new one abstracts the rendering layer.
	/// </summary>
	public class CrmEntityEditingMetadataProviderAdapter : ICrmEntityEditingMetadataProvider
	{
		public CrmEntityEditingMetadataProviderAdapter(ICmsEntityEditingMetadataProvider provider, string portalName = null)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			Provider = provider;
			PortalName = portalName;
		}

		protected string PortalName { get; private set; }

		protected ICmsEntityEditingMetadataProvider Provider { get; private set; }

		public void AddAttributeMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity, string propertyName, string propertyDisplayName)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var metadataContainer = new ControlCmsEntityEditingMetadataContainer(container);
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName ?? PortalName);
			var attributeLogicalName = GetAttributeLogicalNameFromPropertyName(serviceContext, entity.LogicalName, propertyName);

			Provider.AddAttributeMetadata(metadataContainer, entity.ToEntityReference(), attributeLogicalName, propertyDisplayName, portalName);
		}

		public void AddEntityMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var metadataContainer = new ControlCmsEntityEditingMetadataContainer(container);

			Provider.AddEntityMetadata(metadataContainer, entity.ToEntityReference(), portalName);
		}

		public void AddSiteMapNodeMetadata(string portalName, IEditableCrmEntityControl control, Control container, SiteMapNode node)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (node == null)
			{
				throw new ArgumentNullException("node");
			}

			var metadataContainer = new ControlCmsEntityEditingMetadataContainer(container);

			Provider.AddSiteMapNodeMetadata(metadataContainer, node, portalName);
		}

		protected virtual string GetAttributeLogicalNameFromPropertyName(OrganizationServiceContext serviceContext, string entityLogicalName, string propertyName)
		{
			EntitySetInfo entitySetInfo;

			if (OrganizationServiceContextInfo.TryGet(serviceContext.GetType(), entityLogicalName, out entitySetInfo) && entitySetInfo.Entity != null)
			{
				AttributeInfo attributeInfo;

				if (entitySetInfo.Entity.AttributesByPropertyName.TryGetValue(propertyName, out attributeInfo) && attributeInfo.CrmPropertyAttribute != null)
				{
					return attributeInfo.CrmPropertyAttribute.LogicalName;
				}
			}

			return propertyName.ToLowerInvariant();
		}
	}
}
