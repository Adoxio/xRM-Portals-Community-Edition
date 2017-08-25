/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Security
{
	internal class CrmEntityCacheDependencyTrace : IEnumerable<string>
	{
		private const string _crmEntityDependencyFormat = "xrm:dependency:entity:{0}:id={1}";
		private const string _crmEntitySetDependencyFormat = "xrm:dependency:entity:{0}";

		private readonly HashSet<string> _dependencies = new HashSet<string>();

		public CrmEntityCacheDependencyTrace()
		{
			IsCacheable = true;
		}

		public bool IsCacheable { get; set; }

		public void AddDependency(string dependency)
		{
			_dependencies.Add(dependency);
		}

		public void AddEntityDependency(Entity entity)
		{
			if (entity == null || entity.Id == null)
			{
				return;
			}

			AddDependency(_crmEntityDependencyFormat.FormatWith(entity.LogicalName, entity.Id));
		}

		public void AddEntityDependencies(IEnumerable<Entity> entities)
		{
			if (entities == null)
			{
				return;
			}

			foreach (var entity in entities)
			{
				AddEntityDependency(entity);
			}
		}

		public void AddEntitySetDependency(string entityName)
		{
			if (string.IsNullOrEmpty(entityName))
			{
				return;
			}

			AddDependency(_crmEntitySetDependencyFormat.FormatWith(entityName));
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _dependencies.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
