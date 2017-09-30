/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public interface IFileSystemContext
	{
		IDirectory Current { get; }

		DirectoryTreeNode Tree { get; }

		DirectoryTreeNode TreeOfType(string type);
	}

	internal abstract class EntityDirectoryFileSystemContext : IFileSystemContext
	{
		protected EntityDirectoryFileSystemContext(IEntityDirectoryFileSystem fileSystem)
		{
			if (fileSystem == null) throw new ArgumentNullException("fileSystem");

			FileSystem = fileSystem;
		}

		public abstract IDirectory Current { get; }

		public abstract DirectoryTreeNode Tree { get; }

		protected IDictionary<string, DirectoryType> EntityDirectoryTypes
		{
			get { return FileSystem.EntityDirectoryTypes; }
		}

		protected IEntityDirectoryFileSystem FileSystem { get; private set; }

		protected ICrmEntitySecurityProvider SecurityProvider
		{
			get { return FileSystem.SecurityProvider; }
		}

		protected OrganizationServiceContext ServiceContext
		{
			get { return FileSystem.ServiceContext; }
		}

		protected IEntityUrlProvider UrlProvider
		{
			get { return FileSystem.UrlProvider; }
		}

		protected EntityReference Website
		{
			get { return FileSystem.Website; }
		}

		public abstract DirectoryTreeNode TreeOfType(string type);

		protected ILookup<EntityReference, IGrouping<EntityReference, Tuple<Entity, DirectoryType>>> GetTreeParentLookups()
		{
			return FileSystem.EntityDirectoryTypes
				.SelectMany(e => e.Value.GetTreeParents(ServiceContext, Website).Select(parent => new { Type = e.Value, ParentInfo = parent }))
				.GroupBy(e => e.ParentInfo.Item2, e => new Tuple<Entity, DirectoryType>(e.ParentInfo.Item1, e.Type))
				.ToLookup(group => group.Key, group => group);
		}

		protected ILookup<EntityReference, IGrouping<EntityReference, Tuple<Entity, DirectoryType>>> GetTreeParentLookups(string type)
		{
			DirectoryType directoryType;

			if (!FileSystem.EntityDirectoryTypes.TryGetValue(type, out directoryType))
			{
				return Enumerable.Empty<IGrouping<EntityReference, Tuple<Entity, DirectoryType>>>().ToLookup(e => (EntityReference)null);
			}

			return directoryType.GetTreeParents(ServiceContext, Website)
				.GroupBy(parent => parent.Item2, e => new Tuple<Entity, DirectoryType>(e.Item1, directoryType))
				.ToLookup(group => group.Key, group => group);
		}

		protected bool TryAssertSecurity(Entity entity, CrmEntityRight right)
		{
			if (!ServiceContext.IsAttached(entity))
			{
				entity = ServiceContext.MergeClone(entity);
			}

			try
			{
				return SecurityProvider.TryAssert(ServiceContext, entity, right);
			}
			catch (InvalidOperationException e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error validating security for entity [{0}:{1}]: {2}", entity.LogicalName, entity.Id, e.ToString()));

                return false;
			}
		}
	}
}
