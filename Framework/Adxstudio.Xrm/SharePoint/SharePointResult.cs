/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.SharePoint
{
	class SharePointResult : ISharePointResult
	{
		private readonly EntityReference _regarding;
		private readonly CrmEntityPermissionProvider _provider;
		private readonly OrganizationServiceContext _context;
		private bool? _canRead;
		private bool? _canCreate;
		private bool? _canWrite;
		private bool? _canDelete;
		private bool? _canAppend;
		private bool? _canAppendTo;

		public SharePointResult(EntityReference regarding, CrmEntityPermissionProvider provider, OrganizationServiceContext context)
		{
			_regarding = regarding;
			_provider = provider;
			_context = context;
		}

		public bool CanRead
		{
			get
			{
				if (!_canRead.HasValue)
				{
					_canRead = _provider.TryAssert(_context, CrmEntityPermissionRight.Read, "sharepointdocumentlocation");
				}
				return _canRead.Value;
			}
		}

		public bool CanCreate
		{
			get
			{
				if (!_canCreate.HasValue)
				{
					_canCreate = _provider.TryAssert(_context, CrmEntityPermissionRight.Create, "sharepointdocumentlocation");
				}
				return _canCreate.Value;
			}
		}

		public bool CanWrite
		{
			get
			{
				if (!_canWrite.HasValue)
				{
					_canWrite = _provider.TryAssert(_context, CrmEntityPermissionRight.Write, "sharepointdocumentlocation");
				}
				return _canWrite.Value;
			}
		}

		public bool CanDelete
		{
			get
			{
				if (!_canDelete.HasValue)
				{
					_canDelete = _provider.TryAssert(_context, CrmEntityPermissionRight.Delete, "sharepointdocumentlocation");
				}
				return _canDelete.Value;
			}
		}

		public bool CanAppend
		{
			get
			{
				if (!_canAppend.HasValue)
				{
					_canAppend = _provider.TryAssert(_context, CrmEntityPermissionRight.Append, "sharepointdocumentlocation");
				}
				return _canAppend.Value;
			}
		}

		public bool CanAppendTo
		{
			get
			{
				if (!_canAppendTo.HasValue)
				{
					var entityMetadata = _context.GetEntityMetadata(_regarding.LogicalName, EntityFilters.All);
					var primaryKeyName = entityMetadata.PrimaryIdAttribute;
					var entity = _context.CreateQuery(_regarding.LogicalName)
						.First(e => e.GetAttributeValue<Guid>(primaryKeyName) == _regarding.Id);

					_canAppendTo = _provider.TryAssert(_context, CrmEntityPermissionRight.AppendTo, entity);
				}
				return _canAppendTo.Value;
			}
		}

		public bool PermissionsExist
		{
			get { return _provider.PermissionsExist; }
		}
	}
}
