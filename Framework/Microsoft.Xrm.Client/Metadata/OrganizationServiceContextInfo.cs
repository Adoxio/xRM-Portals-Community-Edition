/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// Provides descriptions of a custom <see cref="OrganizationServiceContext"/> class through reflection.
	/// </summary>
	public sealed class OrganizationServiceContextInfo
	{
		/// <summary>
		/// The custom <see cref="OrganizationServiceContext"/> class.
		/// </summary>
		public Type ContextType { get; private set; }

		public OrganizationServiceContextInfo(Type contextType)
		{
			ContextType = contextType;
		}

		private IDictionary<string, EntitySetInfo> _entitySetsByEntityLogicalName;

		/// <summary>
		/// A lookup of <see cref="EntitySetInfo"/> keyed by the entity logical name.
		/// </summary>
		public IDictionary<string, EntitySetInfo> EntitySetsByEntityLogicalName
		{
			get
			{
				if (_entitySetsByEntityLogicalName == null)
				{
					_entitySetsByEntityLogicalName = LoadEntitySets(
						(pi, entityInfo) => entityInfo.EntityLogicalName != null ? entityInfo.EntityLogicalName.LogicalName : null);
				}

				return _entitySetsByEntityLogicalName;
			}
		}

		private IDictionary<string, EntitySetInfo> _entitySetsByPropertyName;

		/// <summary>
		/// A lookup of <see cref="EntitySetInfo"/> keyed by the entity set property name.
		/// </summary>
		public IDictionary<string, EntitySetInfo> EntitySetsByPropertyName
		{
			get
			{
				if (_entitySetsByPropertyName == null)
				{
					_entitySetsByPropertyName = LoadEntitySets((pi, entityInfo) => pi.Name);
				}

				return _entitySetsByPropertyName;
			}
		}

		private IDictionary<string, EntitySetInfo> LoadEntitySets(Func<PropertyInfo, EntityInfo, string> keySelector)
		{
			var dataContextPublicProperties = ContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

			var entitySetTypes =
				from property in dataContextPublicProperties
				let propertyType = property.PropertyType
				where propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IQueryable<>)
				let entityType = propertyType.GetGenericArguments().First()
				select new EntitySetInfo(property, new EntityInfo(entityType));

			var entitySets = new Dictionary<string, EntitySetInfo>();

			foreach (var type in entitySetTypes)
			{
				var key = keySelector(type.Property, type.Entity);

				if (!string.IsNullOrEmpty(key))
				{
					entitySets.Add(key, type);
				}
			}

			return entitySets;
		}

		private static readonly ConcurrentDictionary<Type, OrganizationServiceContextInfo> _typeToContextInfoLookup = new ConcurrentDictionary<Type, OrganizationServiceContextInfo>();

		/// <summary>
		/// Returns a <see cref="OrganizationServiceContextInfo"/> from a custom <see cref="OrganizationServiceContext"/> class.
		/// </summary>
		/// <param name="contextType"></param>
		/// <param name="contextInfo"></param>
		/// <returns></returns>
		public static bool TryGet(Type contextType, out OrganizationServiceContextInfo contextInfo)
		{
			OrganizationServiceContextInfo info;

			if (!_typeToContextInfoLookup.TryGetValue(contextType, out info))
			{
				info = new OrganizationServiceContextInfo(contextType);
				_typeToContextInfoLookup[contextType] = info;
			}

			contextInfo = info;

			return true;
		}

		/// <summary>
		/// Returns a <see cref="EntitySetInfo"/> from a custom <see cref="OrganizationServiceContext"/> class and an entity logical name.
		/// </summary>
		/// <param name="contextType"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="entitySetInfo"></param>
		/// <returns></returns>
		public static bool TryGet(Type contextType, string entityLogicalName, out EntitySetInfo entitySetInfo)
		{
			entitySetInfo = null;
			OrganizationServiceContextInfo contextInfo;

			return TryGet(contextType, out contextInfo) && contextInfo.EntitySetsByEntityLogicalName.TryGetValue(entityLogicalName, out entitySetInfo);
		}

		/// <summary>
		/// Returns a <see cref="EntitySetInfo"/> from a custom <see cref="OrganizationServiceContext"/> class and an entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="entitySetInfo"></param>
		/// <returns></returns>
		public static bool TryGet(OrganizationServiceContext context, Entity entity, out EntitySetInfo entitySetInfo)
		{
			return TryGet(context.GetType(), entity.LogicalName, out entitySetInfo);
		}
	}
}
