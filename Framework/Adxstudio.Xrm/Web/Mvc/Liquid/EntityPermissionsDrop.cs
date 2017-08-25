/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Security;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityPermissionsDrop : Drop
	{
		private readonly CrmEntityPermissionProvider.EntityPermissionResult _entityPermissionResult;

		public EntityPermissionsDrop(CrmEntityPermissionProvider.EntityPermissionResult entityPermissionResult)
		{
			if (entityPermissionResult == null) throw new ArgumentNullException("entityPermissionResult");

			_entityPermissionResult = entityPermissionResult;
		}

		public bool CanAppend
		{
			get { return _entityPermissionResult.CanAppend; }
		}

		public bool CanAppendTo
		{
			get { return _entityPermissionResult.CanAppendTo; }
		}

		public bool CanCreate
		{
			get { return _entityPermissionResult.CanCreate; }
		}

		public bool CanDelete
		{
			get { return _entityPermissionResult.CanDelete; }
		}

		public bool CanRead
		{
			get { return _entityPermissionResult.CanRead; }
		}

		public bool CanWrite
		{
			get { return _entityPermissionResult.CanWrite; }
		}

		public bool RulesExist
		{
			get { return _entityPermissionResult.RulesExist; }
		}
	}
}
