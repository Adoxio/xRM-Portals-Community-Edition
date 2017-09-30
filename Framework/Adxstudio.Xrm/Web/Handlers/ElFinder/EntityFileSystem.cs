/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	internal class EntityFileSystem : EntityDirectoryFileSystem
	{
		private static readonly IDictionary<string, DirectoryType> DirectoryTypes = new Dictionary<string, DirectoryType>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "adx_webpage",  new WebPageDirectoryType() },
			{ "adx_blog",     new BlogDirectoryType()    },
			{ "adx_blogpost", new BlogPostDirectoryType() },
		};

		private readonly Lazy<Entity> _root;

		public EntityFileSystem(IDataAdapterDependencies dependencies) : base(dependencies)
		{
			_root = new Lazy<Entity>(GetRoot, LazyThreadSafetyMode.None);
		}

		protected Entity Root
		{
			get { return _root.Value; }
		}

		public override IDictionary<string, DirectoryType> EntityDirectoryTypes
		{
			get { return DirectoryTypes; }
		}

		public override T Using<T>(Func<IFileSystemContext, T> action)
		{
			if (Root == null)
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			DirectoryType rootDirectoryType;

			if (!EntityDirectoryTypes.TryGetValue(Root.LogicalName, out rootDirectoryType))
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			return action(new EntityFileSystemContext(this, Root, rootDirectoryType, new EntityDirectory(this, Root, rootDirectoryType)));
		}

		public override T Using<T>(DirectoryContentHash cwd, Func<IFileSystemContext, T> action)
		{
			if (Root == null)
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			DirectoryType rootDirectoryType;

			if (!EntityDirectoryTypes.TryGetValue(Root.LogicalName, out rootDirectoryType))
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			DirectoryType currentDirectoryType;

			if (!EntityDirectoryTypes.TryGetValue(cwd.LogicalName, out currentDirectoryType))
			{
				throw new InvalidOperationException("Unable to retrieve the target directory");
			}

			var current = new EntityDirectory(this, cwd.ToEntityReference(), currentDirectoryType);

			if (!current.Exists)
			{
				throw new InvalidOperationException("Unable to retrieve the target directory");
			}

			return action(new EntityFileSystemContext(this, Root, rootDirectoryType, current));
		}

		private Entity GetRoot()
		{
			var siteMarker = new SiteMarkerDataAdapter(Dependencies).SelectWithReadAccess("Home");

			return siteMarker == null ? null : siteMarker.Entity;
		}
	}

	internal class EntityFileSystemContext : EntityDirectoryFileSystemContext
	{
		private readonly IDirectory _current;
		private readonly Lazy<DirectoryTreeNode> _tree;

		public EntityFileSystemContext(IEntityDirectoryFileSystem fileSystem, Entity root, DirectoryType rootDirectoryType, IDirectory current) : base(fileSystem)
		{
			if (root == null) throw new ArgumentNullException("root");
			if (rootDirectoryType == null) throw new ArgumentNullException("rootDirectoryType");
			if (current == null) throw new ArgumentNullException("current");

			Root = root;
			RootDirectoryType = rootDirectoryType;
			_current = current;

			_tree = new Lazy<DirectoryTreeNode>(GetTree, LazyThreadSafetyMode.None);
		}

		public override IDirectory Current
		{
			get { return _current; }
		}

		public override DirectoryTreeNode Tree
		{
			get { return _tree.Value; }
		}

		public override DirectoryTreeNode TreeOfType(string type)
		{
			return GetTree(Root, RootDirectoryType, GetTreeParentLookups(type));
		}

		protected Entity Root { get; private set; }

		protected DirectoryType RootDirectoryType { get; set; }

		private DirectoryTreeNode GetTree()
		{
			return GetTree(Root, RootDirectoryType, GetTreeParentLookups());
		}

		private DirectoryTreeNode GetTree(Entity directory, DirectoryType directoryType, ILookup<EntityReference, IGrouping<EntityReference, Tuple<Entity, DirectoryType>>> parentLookup)
		{
			if (directory == null) throw new ArgumentNullException("directory");
			if (directoryType == null) throw new ArgumentNullException("directoryType");
			if (parentLookup == null) throw new ArgumentNullException("parentLookup");

			return new DirectoryTreeNode(directory)
			{
				name = directoryType.GetDirectoryName(directory),
				hash = new DirectoryContentHash(directory, true).ToString(),
				read = true,
				write = directoryType.SupportsUpload && TryAssertSecurity(directory, CrmEntityRight.Change),
				dirs = parentLookup[directory.ToEntityReference()].SelectMany(grouping => grouping)
					.Where(child => TryAssertSecurity(child.Item1, CrmEntityRight.Read))
					.Select(child => GetTree(child.Item1, child.Item2, parentLookup))
					.OrderBy(node => node.name)
					.ToArray()
			};
		}
	}

	internal class EntityDirectory : Directory
	{
		private readonly Entity _entity;
		private readonly EntityReference _entityReference;

		public EntityDirectory(IEntityDirectoryFileSystem fileSystem, EntityReference entityReference, DirectoryType directoryType) : base(fileSystem)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");
			if (directoryType == null) throw new ArgumentNullException("directoryType");

			_entityReference = entityReference;
			DirectoryType = directoryType;
		}

		public EntityDirectory(IEntityDirectoryFileSystem fileSystem, Entity entity, DirectoryType directoryType) : base(fileSystem)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (directoryType == null) throw new ArgumentNullException("directoryType");

			_entity = entity;
			_entityReference = entity.ToEntityReference();
			DirectoryType = directoryType;
		}

		public override EntityReference EntityReference
		{
			get { return _entityReference; }
		}

		public override bool SupportsUpload
		{
			get { return DirectoryType.SupportsUpload; }
		}

		public override string WebFileForeignKeyAttribute
		{
			get { return DirectoryType.WebFileForeignKeyAttribute; }
		}

		protected DirectoryType DirectoryType { get; private set; }

		protected override DirectoryContent[] GetChildren()
		{
			var directoryFile = new DirectoryContent
			{
				name = Info.name,
				hash = new DirectoryContentHash(EntityReference).ToString(),
				url = Info.url,
				date = Info.date,
				read = Info.read,
				write = Info.write,
				rm = Info.rm,
				mime = "application/x-{0}".FormatWith(EntityReference.LogicalName),
				size = 0,
			};

			var children = DirectoryType.GetEntityChildren(ServiceContext, Entity, Website)
				.Select(GetDirectoryContent)
				.Where(content => content != null)
				.OrderByDescending(content => content.mime == DirectoryMimeType)
				.ThenBy(content => content.name)
				.ToArray();

			return new[] { directoryFile }.Concat(children).ToArray();
		}

		protected override DirectoryContent GetInfo()
		{
			return new DirectoryContent
			{
				name = DirectoryType.GetDirectoryName(Entity),
				hash = new DirectoryContentHash(EntityReference, true).ToString(),
				mime = DirectoryMimeType,
				rel = Url,
				url = Url,
				size = 0,
				date = FormatDateTime(Entity.GetAttributeValue<DateTime?>("modifiedon")),
				read = CanRead,
				write = SupportsUpload && CanWrite,
				rm = false,
			};
		}

		protected override Entity GetEntity()
		{
			return _entity ?? DirectoryType.GetEntity(ServiceContext, EntityReference.Id, Website);
		}

		protected override string GetUrl()
		{
			try
			{
				return UrlProvider.GetUrl(ServiceContext, Entity);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error getting URL for entity [{0}:{1}]: {2}", EntityReference.LogicalName, EntityReference.Id, e.ToString()));

                return null;
			}
		}
	}
}
