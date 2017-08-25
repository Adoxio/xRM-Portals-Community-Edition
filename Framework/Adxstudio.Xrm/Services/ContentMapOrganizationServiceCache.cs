/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Caching;
using Adxstudio.Xrm.Configuration;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Services
{
	/// <summary>
	/// An <see cref="Microsoft.Xrm.Client.Services.OrganizationServiceCache"/> that maintains a <see cref="ContentMap"/> cache item.
	/// </summary>
	public class ContentMapOrganizationServiceCache : CompositeOrganizationServiceCache
	{
		public string PortalName { get; private set; }

		public ContentMapOrganizationServiceCache()
		{
		}

		public ContentMapOrganizationServiceCache(ObjectCache cache)
			: base(cache)
		{
		}

		public ContentMapOrganizationServiceCache(ObjectCache cache, CrmConnection connection)
			: base(cache, connection)
		{
		}

		public ContentMapOrganizationServiceCache(ObjectCache cache, string connectionId)
			: base(cache, connectionId)
		{
		}

		public ContentMapOrganizationServiceCache(ObjectCache cache, OrganizationServiceCacheSettings settings)
			: base(cache, settings)
		{
		}

		public ContentMapOrganizationServiceCache(IOrganizationServiceCache inner)
			: base(inner)
		{
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name, config);

			PortalName = config["portalName"];
		}

		public override T Execute<T>(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> execute, Func<OrganizationResponse, T> selector, string selectorCacheKey)
		{
			request.ThrowOnNull("request");
			execute.ThrowOnNull("execute");
			
			var response = base.Execute(request, execute, selector, selectorCacheKey);

			var message = GetMessage(request, response as OrganizationResponse);

			if (message != null)
			{
				Remove(message);
			}

			return response;
		}

		public override void Remove(string entityLogicalName, Guid? id)
		{
			entityLogicalName.ThrowOnNullOrWhitespace("entityLogicalName");

			base.Remove(entityLogicalName, id);

			if (id != null && id.Value != Guid.Empty)
			{
				Refresh(new EntityReference(entityLogicalName, id.Value));
			}
		}

		public override void Remove(EntityReference entity)
		{
			entity.ThrowOnNull("entity");

			base.Remove(entity);

			Refresh(entity);
		}

		public override void Remove(Entity entity)
		{
			entity.ThrowOnNull("entity");

			base.Remove(entity);

			Refresh(entity.ToEntityReference());
		}

		public override void Remove(OrganizationServiceCachePluginMessage message)
		{
			message.ThrowOnNull("message");
			if (message is OrganizationServiceCacheBatchedPluginMessage)
			{
				Remove((OrganizationServiceCacheBatchedPluginMessage)message);
			}
			else
			{
				base.Remove(message);

				if (message.Target != null && message.Relationship == null)
				{
					var entity = message.Target.ToEntityReference();

					Refresh(entity);
				}

				if (message.Target != null && message.Relationship != null && message.RelatedEntities != null)
				{
					var target = message.Target.ToEntityReference();
					var relationship = message.Relationship.ToRelationship();
					var relatedEntities = message.RelatedEntities.ToEntityReferenceCollection();

					if (message.MessageName == "Associate")
					{
						Associate(target, relationship, relatedEntities);
					}

					if (message.MessageName == "Disassociate")
					{
						Disassociate(target, relationship, relatedEntities);
					}
				}

				if (message.Category != null && message.Category.Value.HasFlag(CacheItemCategory.Content))
				{
					var contentMapProvider = GetContentMapProvider();

					if (contentMapProvider != null)
					{
						contentMapProvider.Clear();
					}
				}
			}
		}

		private void Remove(OrganizationServiceCacheBatchedPluginMessage messages)
		{
			var batchedMessages = messages.BatchedPluginMessage;
			List<EntityReference> entities = new List<EntityReference>();

			if (batchedMessages.Count > 0)
			{
				base.Remove(batchedMessages[0]);
			}

			foreach (var message in batchedMessages)
			{
				if (message.Target != null && message.Relationship == null)
				{
					var entity = message.Target.ToEntityReference();
					entities.Add(entity);
				}
				if (message.Target != null && message.Relationship != null && message.RelatedEntities != null)
				{
					var target = message.Target.ToEntityReference();
					var relationship = message.Relationship.ToRelationship();
					var relatedEntities = message.RelatedEntities.ToEntityReferenceCollection();

					if (message.MessageName == "Associate")
					{
						Associate(target, relationship, relatedEntities);
					}

					if (message.MessageName == "Disassociate")
					{
						Disassociate(target, relationship, relatedEntities);
					}
				}

				if (message.Category != null && message.Category.Value.HasFlag(CacheItemCategory.Content))
				{
					var contentMapProvider = GetContentMapProvider();

					if (contentMapProvider != null)
					{
						contentMapProvider.Clear();
					}
				}
			}
			Refresh(entities);
		}

		private void Refresh(EntityReference entity)
		{
			var contentMapProvider = GetContentMapProvider();

			if (contentMapProvider == null) return;

			var map = contentMapProvider.Using(m => m);

			contentMapProvider.Refresh(map, entity);
		}

		private void Refresh(List<EntityReference> entities)
		{
			var contentMapProvider = GetContentMapProvider();

			if (contentMapProvider == null) return;

			var map = contentMapProvider.Using(m => m);

			contentMapProvider.Refresh(map, entities);
		}

		private void Associate(EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			target.ThrowOnNull("target");
			relationship.ThrowOnNull("relationship");
			relatedEntities.ThrowOnNull("relatedEntities");

			var contentMapProvider = GetContentMapProvider();

			if (contentMapProvider == null) return;

			var map = contentMapProvider.Using(m => m);

			RefreshIntersect(map, relationship);

			contentMapProvider.Associate(map, target, relationship, relatedEntities);
		}

		private void Disassociate(EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			target.ThrowOnNull("target");
			relationship.ThrowOnNull("relationship");
			relatedEntities.ThrowOnNull("relatedEntities");

			var contentMapProvider = GetContentMapProvider();

			if (contentMapProvider == null) return;

			var map = contentMapProvider.Using(m => m);

			RefreshIntersect(map, relationship);

			contentMapProvider.Disassociate(map, target, relationship, relatedEntities);
		}

		private void RefreshIntersect(ContentMap map, Relationship relationship)
		{
			// invalidate any cached intersect queries

			ManyRelationshipDefinition mrd;

			if (map.Solution.ManyRelationships != null && map.Solution.ManyRelationships.TryGetValue(relationship.SchemaName, out mrd))
			{
				base.Remove(mrd.IntersectEntityname, null);
			}
		}

		private IContentMapProvider GetContentMapProvider()
		{
			return AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);
		}
	}
}
