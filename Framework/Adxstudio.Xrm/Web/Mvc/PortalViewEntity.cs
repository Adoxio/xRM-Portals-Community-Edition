/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc
{
	public class PortalViewEntity : IPortalViewEntity
	{
		private readonly Lazy<string> _description;
		private readonly Lazy<bool> _editable;
		private readonly OrganizationServiceContext _serviceContext;
		private readonly Lazy<string> _url;
		
		public PortalViewEntity(OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (securityProvider == null)
			{
				throw new ArgumentNullException("securityProvider");
			}

			if (urlProvider == null)
			{
				throw new ArgumentNullException("urlProvider");
			}

			Entity = entity;
			EntityReference = entity.ToEntityReference();

			_description = new Lazy<string>(GetDescription, LazyThreadSafetyMode.None);
			_editable = new Lazy<bool>(() => TryAssertEditable(serviceContext, entity, securityProvider), LazyThreadSafetyMode.None);
			_serviceContext = serviceContext;
			_url = new Lazy<string>(() => urlProvider.GetUrl(serviceContext, entity));
		}

		public string Description
		{
			get { return _description.Value; }
		}

		public bool Editable
		{
			get { return _editable.Value; }
		}

		public EntityReference EntityReference { get; private set; }

		public string Url
		{
			get { return _url.Value; }
		}

		protected Entity Entity { get; private set; }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			if (attributeLogicalName == null)
			{
				return null;
			}

			var value = Entity.GetAttributeValue<object>(attributeLogicalName);

			return new PortalViewEntityAttribute(this, attributeLogicalName, value, new Lazy<string>(() => GetAttributeDescription(attributeLogicalName), LazyThreadSafetyMode.None));
		}

		private string GetAttributeDescription(string attributeLogicalName)
		{
			return _serviceContext.GetEntityPrimaryNameWithAttributeLabel(Entity, attributeLogicalName);
		}

		private string GetDescription()
		{
			return string.IsNullOrWhiteSpace(EntityReference.Name)
				? _serviceContext.GetEntityPrimaryName(Entity)
				: EntityReference.Name;
		}

		private static bool TryAssertEditable(OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider securityProvider)
		{
			return securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Change);
		}
	}
}
