/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	using Adxstudio.Xrm.Services;

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Web;

	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc.Html;

	using DotLiquid;

	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;

	public class EntityDrop : PortalUrlDrop, IEditableCollection
	{
		private readonly Lazy<CmsDrop> _cms;
		private readonly Lazy<IDictionary<string, EntityAliasedAttributesDrop>> _entityAliasedAttributesDrops;
		private readonly Lazy<EntityMetadata> _entityMetadata;
		private readonly Lazy<Hash> _formattedValues;
		private readonly Lazy<Entity> _fullEntity;
		private readonly Lazy<int> _languageCode;
		private readonly Lazy<IMoneyFormatInfo> _moneyFormatInfo;
		private readonly Lazy<EntityNoteDrop[]> _notes;
		private readonly Lazy<EntityPermissionsDrop> _permissions;
		private readonly Lazy<EntityListViewDrop> _view;
		private readonly Lazy<string> _url;

		public EntityDrop(IPortalLiquidContext portalLiquidContext, Entity entity, Lazy<int> languageCode = null, Lazy<EntityListViewDrop> view = null) : this(portalLiquidContext)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;
			EntityReference = entity.ToEntityReference();

			_languageCode = languageCode;
			_view = view;
		}

		protected EntityDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext)
		{
			_cms = new Lazy<CmsDrop>(GetCms, LazyThreadSafetyMode.None);
			_entityAliasedAttributesDrops = new Lazy<IDictionary<string, EntityAliasedAttributesDrop>>(GetEntityAliasedAttributesDrops, LazyThreadSafetyMode.None);
			_entityMetadata = new Lazy<EntityMetadata>(GetEntityMetadata, LazyThreadSafetyMode.None);
			_formattedValues = new Lazy<Hash>(GetFormattedValues, LazyThreadSafetyMode.None);
			_fullEntity = new Lazy<Entity>(GetEntity, LazyThreadSafetyMode.None);
			_moneyFormatInfo = new Lazy<IMoneyFormatInfo>(GetMoneyFormatInfo, LazyThreadSafetyMode.None);
			_notes = new Lazy<EntityNoteDrop[]>(GetNotes, LazyThreadSafetyMode.None);
			_permissions = new Lazy<EntityPermissionsDrop>(GetPermissions, LazyThreadSafetyMode.None);
			_url = new Lazy<string>(GetUrl, LazyThreadSafetyMode.None);
		}

		public CmsDrop Cms
		{
			get { return _cms.Value; }
		}

		public Hash FormattedValues
		{
			get { return _formattedValues.Value; }
		}

		public virtual string Id
		{
			get { return EntityReference.Id.ToString(); }
		}

		public virtual string LogicalName
		{
			get { return EntityReference.LogicalName; }
		}

		public IEnumerable<EntityNoteDrop> Notes
		{
			get { return _notes.Value; }
		}

		public EntityPermissionsDrop Permissions
		{
			get { return _permissions.Value; }
		}

		public override string Url
		{
			get { return _url.Value; }
		}

		protected virtual Entity Entity { get; private set; }

		protected virtual IDictionary<string, EntityAliasedAttributesDrop> EntityAliasedAttributesDrops
		{
			get { return _entityAliasedAttributesDrops.Value; }
		}

		protected virtual EntityReference EntityReference { get; private set; }

		protected virtual Entity FullEntity
		{
			get { return _fullEntity.Value; }
		}

		internal virtual EntityMetadata EntityMetadata
		{
			get { return _entityMetadata.Value; }
		}

		internal IMoneyFormatInfo MoneyFormatInfo
		{
			get { return _moneyFormatInfo.Value; }
		}

		private readonly IDictionary<string, object> _dynamicMethodCache = new Dictionary<string, object>();

		public override object BeforeMethod(string method)
		{
			object cached;

			if (_dynamicMethodCache.TryGetValue(method, out cached))
			{
				return cached;
			}

			var result = BeforeMethodInternal(method);

			_dynamicMethodCache[method] = result;

			return result;
		}

		public virtual string GetEditable(Context context, string key, EditableOptions options)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (key == null) throw new ArgumentNullException("key");
			if (options == null) throw new ArgumentNullException("options");

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var attachedEntity = serviceContext.MergeClone(Entity.Attributes.ContainsKey(key) ? Entity : FullEntity);
				var portalViewEntity = PortalViewContext.GetEntity(serviceContext, attachedEntity);

				IHtmlString html = null;

				context.Stack(() =>
				{
					html = Html.AttributeInternal(
						portalViewEntity.GetAttribute(key),
						options.Type ?? "html",
						options.Title,
						options.Escape.GetValueOrDefault(false),
						options.Tag ?? "div",
						options.CssClass,
						options.Liquid.GetValueOrDefault(true),
						context,
						options.Default);
				});

				return html == null ? null : html.ToString();
			}
		}

		protected virtual object BeforeMethodInternal(string method)
		{
			if (string.Equals(method, "logicalname", StringComparison.OrdinalIgnoreCase))
			{
				return LogicalName;
			}

			object value;

			if (Entity.Attributes.TryGetValue(method, out value))
			{
				return TransformAttributeValueForLiquid(LogicalName, method, value);
			}

			EntityAliasedAttributesDrop aliasedAttributesDrop;

			if (EntityAliasedAttributesDrops.TryGetValue(method, out aliasedAttributesDrop))
			{
				return aliasedAttributesDrop;
			}

			// Look up the dynamic method to see if there's a matching relationship name.

			var oneToMany = EntityMetadata.OneToManyRelationships
				.FirstOrDefault(r => string.Equals(r.SchemaName, method, StringComparison.OrdinalIgnoreCase));

			var manyToOne = EntityMetadata.ManyToOneRelationships
				.FirstOrDefault(r => string.Equals(r.SchemaName, method, StringComparison.OrdinalIgnoreCase));

			// If there's an ambiguous reflexive relationship, return a reflexive relationship drop so the user
			// can further dot-through to select which side of the relationship they want.
			if (oneToMany != null && manyToOne != null)
			{
				return new EntityReflexiveRelationshipDrop(
					new Lazy<EntityDrop[]>(() => GetRelatedEntities(new Relationship(oneToMany.SchemaName) { PrimaryEntityRole = EntityRole.Referenced }), LazyThreadSafetyMode.None),
					new Lazy<EntityDrop>(() => GetRelatedEntity(new Relationship(manyToOne.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }), LazyThreadSafetyMode.None));
			}

			if (oneToMany != null)
			{
				return GetRelatedEntities(new Relationship(oneToMany.SchemaName));
			}

			if (manyToOne != null)
			{
				return GetRelatedEntity(new Relationship(manyToOne.SchemaName));
			}

			var manyToMany = EntityMetadata.ManyToManyRelationships
				.FirstOrDefault(r => string.Equals(r.SchemaName, method, StringComparison.OrdinalIgnoreCase));

			if (manyToMany != null)
			{
				if (string.Equals(manyToMany.Entity1LogicalName, manyToMany.Entity2LogicalName, StringComparison.OrdinalIgnoreCase))
				{
					return new EntityManyToManyReflexiveRelationshipDrop(
						new Lazy<EntityDrop[]>(() => GetRelatedEntities(new Relationship(manyToMany.SchemaName) { PrimaryEntityRole = EntityRole.Referenced }), LazyThreadSafetyMode.None),
						new Lazy<EntityDrop[]>(() => GetRelatedEntities(new Relationship(manyToMany.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }), LazyThreadSafetyMode.None));
				}

				return GetRelatedEntities(new Relationship(manyToMany.SchemaName));
			}

			return base.BeforeMethod(method);
		}

		protected object TransformAttributeValueForLiquid(string entityLogicalName, string attributeLogicalName, object value)
		{
			if (value == null)
			{
				return null;
			}

			var aliasedValue = value as AliasedValue;

			if (aliasedValue != null)
			{
				return TransformAttributeValueForLiquid(aliasedValue.EntityLogicalName, aliasedValue.AttributeLogicalName, aliasedValue.Value);
			}

			if (value is Guid)
			{
				return ((Guid)value).ToString();
			}

			var entityReference = value as EntityReference;

			if (entityReference != null)
			{
				EntityAliasedAttributesDrop aliasedAttributesDrop;

				return EntityAliasedAttributesDrops.TryGetValue(attributeLogicalName, out aliasedAttributesDrop)
					? new EntityReferenceDrop(entityReference, aliasedAttributesDrop)
					: new EntityReferenceDrop(entityReference);
			}

			var optionSetValue = value as OptionSetValue;

			if (optionSetValue != null)
			{
				return new OptionSetValueDrop(this, entityLogicalName, attributeLogicalName, optionSetValue, _languageCode);
			}

			var money = value as Money;

			if (money != null)
			{
				return money.Value;
			}

			return value;
		}

		private CmsDrop GetCms()
		{
			return FullEntity == null ? null : new CmsDrop(this, FullEntity);
		}

		private Entity GetEntity()
		{
			var cacheKey = "liquid:entity:{0}:{1}".FormatWith(EntityReference.LogicalName, EntityReference.Id);

			object cached;

			if (Html.ViewContext.TempData.TryGetValue(cacheKey, out cached) && cached is Entity)
			{
				return cached as Entity;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest
				{
					ColumnSet = new ColumnSet(true),
					Target = EntityReference
				});

				var entity = response.Entity;

				Html.ViewContext.TempData[cacheKey] = entity;

				return entity;
			}
		}

		private IDictionary<string, EntityAliasedAttributesDrop> GetEntityAliasedAttributesDrops()
		{
			if (_view == null || _view.Value == null || _view.Value.View == null)
			{
				return new Dictionary<string, EntityAliasedAttributesDrop>();
			}

			var viewFetch = Fetch.Parse(_view.Value.View.FetchXml);

			if (viewFetch == null)
			{
				return new Dictionary<string, EntityAliasedAttributesDrop>();
			}

			return viewFetch.Entity.Links
				.Where(link => !string.IsNullOrEmpty(link.Alias))
				.GroupBy(link => link.FromAttribute)
				.ToDictionary(group => group.Key, GetEntityAliasedAttributesDropFromLinks);
		}

		private EntityAliasedAttributesDrop GetEntityAliasedAttributesDropFromLinks(IEnumerable<Link> links)
		{
			var aliasedAttributes = new Dictionary<string, AliasedValue>();

			foreach (var link in links)
			{
				foreach (var attribute in link.Attributes)
				{
					var aliasedName = "{0}.{1}".FormatWith(link.Alias, attribute.Name);

					object value;

					if (Entity.Attributes.TryGetValue(aliasedName, out value) && value is AliasedValue)
					{
						aliasedAttributes[attribute.Name] = value as AliasedValue;
					}
				}
			}

			return new EntityAliasedAttributesDrop(
				aliasedAttributes,
				aliasedValue =>
					TransformAttributeValueForLiquid(aliasedValue.EntityLogicalName, aliasedValue.AttributeLogicalName,
						aliasedValue.Value));
		}

		private EntityMetadata GetEntityMetadata()
		{
			var cacheKey = "liquid:entitymetadata:{0}".FormatWith(EntityReference.LogicalName);

			object cached;

			if (Html.ViewContext.TempData.TryGetValue(cacheKey, out cached) && cached is EntityMetadata)
			{
				return cached as EntityMetadata;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = LogicalName,
					EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships
				});

				var entityMetadata = response.EntityMetadata;

				Html.ViewContext.TempData[cacheKey] = entityMetadata;

				return entityMetadata;
			}
		}

		private Hash GetFormattedValues()
		{
			return Hash.FromDictionary(Entity.FormattedValues.ToDictionary(item => item.Key, item => item.Value as object));
		}

		private IMoneyFormatInfo GetMoneyFormatInfo()
		{
			return new EntityRecordMoneyFormatInfo(PortalViewContext, Entity);
		}

		private EntityNoteDrop[] GetNotes()
		{
			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var filter = new FilterExpression(LogicalOperator.And);

				filter.AddCondition("objecttypecode", ConditionOperator.Equal, EntityReference.LogicalName);
				filter.AddCondition("objectid", ConditionOperator.Equal, EntityReference.Id);

				var query = new QueryExpression("annotation")
				{
					// When we get notes, we want to get most attributes, but NOT documentbody, to avoid having
					// to load large file contents. We want people to use the handler URL on the note drop if
					// possible. If not, the note drop can lazy-load the documentbody attribute if requested.
					ColumnSet = new ColumnSet(new[]
					{
						"annotationid",
						"createdby",
						"createdon",
						"createdonbehalfby",
						"filename",
						"filesize",
						"isdocument",
						"langid",
						"mimetype",
						"modifiedby",
						"modifiedon",
						"modifiedonbehalfby",
						"notetext",
						"objectid",
						"objecttypecode",
						"ownerid",
						"owningbusinessunit",
						"owningteam",
						"owninguser",
						"stepid",
						"subject",
						"versionnumber"
					}),
					Criteria = filter
				};

				query.AddOrder("createdon", OrderType.Ascending);

				var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
				{
					Query = query
				});

				return response.EntityCollection.Entities
					.Select(e => new EntityNoteDrop(this, e)).ToArray();
			}
		}

		private EntityPermissionsDrop GetPermissions()
		{
			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return null;
			}

			if (FullEntity == null)
			{
				return null;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var clone = serviceContext.MergeClone(FullEntity);

				var permissionResult = new CrmEntityPermissionProvider(PortalViewContext.PortalName)
					.TryAssert(serviceContext, clone);

				return permissionResult == null
					? null
					: new EntityPermissionsDrop(permissionResult);
			}
		}

		private EntityDrop[] GetRelatedEntities(Relationship relationship)
		{
			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var clone = serviceContext.MergeClone(Entity);

				var drops = serviceContext.RetrieveRelatedEntities(clone, relationship).Entities
					.Select(e => this.CreateEntityDropWithPermission(e))
					.ToArray();

				return drops;
			}
		}

		private EntityDrop GetRelatedEntity(Relationship relationship)
		{
			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var clone = serviceContext.MergeClone(Entity);

				var entity = serviceContext.RetrieveRelatedEntity(clone, relationship);

				return this.CreateEntityDropWithPermission(entity);
			}
		}

		private EntityDrop CreateEntityDropWithPermission(Entity entity)
		{
			if (entity == null)
			{
				return null;
			}

			EntityDrop entityDrop = new EntityDrop(this, entity, _languageCode, _view);

			return entityDrop.Permissions.CanRead ? entityDrop : null;
		}

		private string GetUrl()
		{
			if (FullEntity == null)
			{
				return null;
			}

			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var clone = serviceContext.MergeClone(FullEntity);

				return PortalViewContext.UrlProvider.GetUrl(serviceContext, clone);
			}
		}
	}

	public class CmsDrop : PortalDrop
	{
		private readonly Lazy<bool> _canChange;
		private readonly Lazy<bool> _canRead;

		public CmsDrop(IPortalLiquidContext portalLiquidContext, Entity entity) : base(portalLiquidContext)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;
			
			_canChange = new Lazy<bool>(GetCanChange, LazyThreadSafetyMode.None);
			_canRead = new Lazy<bool>(GetCanRead, LazyThreadSafetyMode.None);
		}

		public bool CanChange
		{
			get { return _canChange.Value; }
		}

		public bool CanRead
		{
			get { return _canRead.Value; }
		}

		protected Entity Entity { get; private set; }

		private bool GetCanChange()
		{
			return TryAssert(CrmEntityRight.Change);
		}

		private bool GetCanRead()
		{
			return TryAssert(CrmEntityRight.Read);
		}

		private bool TryAssert(CrmEntityRight right)
		{
			using (var serviceContext = PortalViewContext.CreateServiceContext())
			{
				var attachedEntity = serviceContext.MergeClone(Entity);

				return PortalViewContext.CreateCrmEntitySecurityProvider().TryAssert(serviceContext, attachedEntity, right);
			}
		}
	}

	public class EntityReferenceDrop : Drop
	{
		private readonly EntityAliasedAttributesDrop _aliasedAttributesDrop;
		private readonly EntityReference _entityReference;

		public EntityReferenceDrop(EntityReference entityReference, EntityAliasedAttributesDrop aliasedAttributesDrop = null)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			_entityReference = entityReference;
			_aliasedAttributesDrop = aliasedAttributesDrop;
		}

		public string Id
		{
			get { return _entityReference.Id.ToString(); }
		}

		public bool IsEntityReference
		{
			get { return true; }
		}

		public string LogicalName
		{
			get { return _entityReference.LogicalName; }
		}

		public string Name
		{
			get { return _entityReference.Name; }
		}

		public override object BeforeMethod(string method)
		{
			if (string.Equals(method, "logicalname", StringComparison.OrdinalIgnoreCase))
			{
				return LogicalName;
			}

			if (_aliasedAttributesDrop != null)
			{
				return _aliasedAttributesDrop.InvokeDrop(method) ?? base.BeforeMethod(method);
			}

			return base.BeforeMethod(method);
		}

		public EntityReference ToEntityReference()
		{
			return _entityReference;
		}
	}

	public class EntityAliasedAttributesDrop : Drop
	{
		private readonly IDictionary<string, AliasedValue> _aliasedAttributes;
		private readonly Func<AliasedValue, object> _transformValue;

		public EntityAliasedAttributesDrop(IDictionary<string, AliasedValue> aliasedAttributes,
			Func<AliasedValue, object> transformValue)
		{
			if (aliasedAttributes == null) throw new ArgumentNullException("aliasedAttributes");
			if (transformValue == null) throw new ArgumentNullException("transformValue");

			_aliasedAttributes = aliasedAttributes;
			_transformValue = transformValue;
		}

		public override object BeforeMethod(string method)
		{
			AliasedValue value;

			if (_aliasedAttributes.TryGetValue(method, out value))
			{
				return _transformValue(value);
			}

			return base.BeforeMethod(method);
		}
	}

	public class EntityReflexiveRelationshipDrop : Drop
	{
		private readonly Lazy<EntityDrop[]> _referenced;
		private readonly Lazy<EntityDrop> _referencing;

		public EntityReflexiveRelationshipDrop(Lazy<EntityDrop[]> referenced, Lazy<EntityDrop> referencing)
		{
			if (referenced == null) throw new ArgumentNullException("referenced");
			if (referencing == null) throw new ArgumentNullException("referencing");

			_referenced = referenced;
			_referencing = referencing;
		}

		public bool IsReflexive
		{
			get { return true; }
		}

		public IEnumerable<EntityDrop> Referenced
		{
			get { return _referenced.Value; }
		}

		public EntityDrop Referencing
		{
			get { return _referencing.Value; }
		}
	}

	public class EntityManyToManyReflexiveRelationshipDrop : Drop
	{
		private readonly Lazy<EntityDrop[]> _referenced;
		private readonly Lazy<EntityDrop[]> _referencing;

		public EntityManyToManyReflexiveRelationshipDrop(Lazy<EntityDrop[]> referenced, Lazy<EntityDrop[]> referencing)
		{
			if (referenced == null) throw new ArgumentNullException("referenced");
			if (referencing == null) throw new ArgumentNullException("referencing");

			_referenced = referenced;
			_referencing = referencing;
		}

		public bool IsReflexive
		{
			get { return true; }
		}

		public IEnumerable<EntityDrop> Referenced
		{
			get { return _referenced.Value; }
		}

		public IEnumerable<EntityDrop> Referencing
		{
			get { return _referencing.Value; }
		}
	}
}
