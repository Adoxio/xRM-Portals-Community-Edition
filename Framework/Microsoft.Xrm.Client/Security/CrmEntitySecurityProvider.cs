/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Security
{
	/// <summary>
	/// A set of operations for asserting permissions on <see cref="Entity"/> objects.
	/// </summary>
	public abstract class CrmEntitySecurityProvider : ICrmEntitySecurityProvider, IInitializable
	{
		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
		}

		/// <summary>
		/// Asserts that the current user has the requested right to an entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="right"></param>
		/// <exception cref="SecurityException"></exception>
		public virtual void Assert(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			if (!TryAssert(context, entity, right))
			{
				throw new SecurityException("Security assertion of right {0} failed for entity {1} ({2})".FormatWith(right, entity.Id, entity.GetType()));
			}
		}

		/// <summary>
		/// Asserts that the current user has the requested right to a set of entities.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entities"></param>
		/// <param name="right"></param>
		/// <exception cref="SecurityException"></exception>
		public virtual void Assert(OrganizationServiceContext context, IEnumerable<Entity> entities, CrmEntityRight right)
		{
			foreach (var entity in entities)
			{
				Assert(context, entity, right);
			}
		}

		/// <summary>
		/// Asserts that the current user has the requested right to an entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public abstract bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right);

		/// <summary>
		/// Asserts that the current user has the requested right to a set of entities.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entities"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public virtual bool TryAssert(OrganizationServiceContext context, IEnumerable<Entity> entities, CrmEntityRight right)
		{
			return entities.All(entity => TryAssert(context, entity, right));
		}
	}
}
