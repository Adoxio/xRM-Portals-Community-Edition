/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Security
{
	/// <summary>
	/// A set of operations for asserting permissions on <see cref="Entity"/> objects.
	/// </summary>
	public interface ICrmEntitySecurityProvider
	{
		/// <summary>
		/// Asserts that the current user has the requested right to an entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="right"></param>
		/// <exception cref="SecurityException"></exception>
		void Assert(OrganizationServiceContext context, Entity entity, CrmEntityRight right);

		/// <summary>
		/// Asserts that the current user has the requested right to a set of entities.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entities"></param>
		/// <param name="right"></param>
		/// <exception cref="SecurityException"></exception>
		void Assert(OrganizationServiceContext context, IEnumerable<Entity> entities, CrmEntityRight right);

		/// <summary>
		/// Asserts that the current user has the requested right to an entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right);

		/// <summary>
		/// Asserts that the current user has the requested right to a set of entities.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entities"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		bool TryAssert(OrganizationServiceContext context, IEnumerable<Entity> entities, CrmEntityRight right);
	}
}
