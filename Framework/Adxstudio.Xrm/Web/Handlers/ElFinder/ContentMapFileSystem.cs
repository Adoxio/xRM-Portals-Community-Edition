/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	internal class ContentMapFileSystem : EntityDirectoryFileSystem
	{
		private static readonly IDictionary<string, DirectoryType> DirectoryTypes = new Dictionary<string, DirectoryType>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "adx_blog",     new BlogDirectoryType()    },
			{ "adx_blogpost", new BlogPostDirectoryType() },
		};

		public ContentMapFileSystem(IContentMapProvider contentMapProvider, IContentMapEntityUrlProvider contentMapUrlProvider, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			if (contentMapProvider == null) throw new ArgumentNullException("contentMapProvider");
			if (contentMapUrlProvider == null) throw new ArgumentNullException("contentMapUrlProvider");

			ContentMapProvider = contentMapProvider;
			ContentMapUrlProvider = contentMapUrlProvider;
		}

		public IContentMapEntityUrlProvider ContentMapUrlProvider { get; private set; }

		protected IContentMapProvider ContentMapProvider { get; private set; }

		public override IDictionary<string, DirectoryType> EntityDirectoryTypes
		{
			get { return DirectoryTypes; }
		}

		public override T Using<T>(Func<IFileSystemContext, T> action)
		{
			return ContentMapProvider.Using(map => Using(map, action));
		}

		public override T Using<T>(DirectoryContentHash cwd, Func<IFileSystemContext, T> action)
		{
			return ContentMapProvider.Using(map => Using(map, cwd, action));
		}

		private IDirectory GetDirectory(ContentMap contentMap, DirectoryContentHash hash)
		{
			var entityReference = hash.ToEntityReference();

			WebPageNode node;

			if (contentMap.TryGetValue(entityReference, out node))
			{
				return new WebPageNodeDirectory(this, contentMap, node);
			}

			DirectoryType entityDirectoryType;

			if (EntityDirectoryTypes.TryGetValue(entityReference.LogicalName, out entityDirectoryType))
			{
				return new EntityDirectory(this, entityReference, entityDirectoryType);
			}

			return null;
		}

		private WebPageNode GetRoot(ContentMap contentMap)
		{
			IDictionary<EntityReference, EntityNode> siteMarkers;

			if (!contentMap.TryGetValue("adx_sitemarker", out siteMarkers))
			{
				return null;
			}

			var siteMarkerNode = siteMarkers.Values.Cast<SiteMarkerNode>()
				.FirstOrDefault(e => e.Website.Id == Website.Id && e.Name == "Home");

			if (siteMarkerNode == null || siteMarkerNode.WebPage == null)
			{
				return null;
			}

			WebPageNode root;

			return contentMap.TryGetValue(siteMarkerNode.WebPage, out root)
				? root
				: null;
		}

		private T Using<T>(ContentMap contentMap, Func<IFileSystemContext, T> action)
		{
			var root = GetRoot(contentMap);

			if (root == null)
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			return action(new ContentMapFileSystemContext(this, contentMap, root, new WebPageNodeDirectory(this, contentMap, root)));
		}

		private T Using<T>(ContentMap contentMap, DirectoryContentHash cwd, Func<IFileSystemContext, T> action)
		{
			var root = GetRoot(contentMap);

			if (root == null)
			{
				throw new InvalidOperationException("Unable to retrieve the root directory.");
			}

			var current = GetDirectory(contentMap, cwd);

			if (current == null || !current.Exists)
			{
				throw new InvalidOperationException("Unable to retrieve the target directory");
			}

			return action(new ContentMapFileSystemContext(this, contentMap, root, current));
		}
	}

	internal class ContentMapFileSystemContext : EntityDirectoryFileSystemContext
	{
		private readonly IDirectory _current;
		private readonly Lazy<DirectoryTreeNode> _tree;

		public ContentMapFileSystemContext(IEntityDirectoryFileSystem fileSystem, ContentMap contentMap, WebPageNode root, IDirectory current) : base(fileSystem)
		{
			if (contentMap == null) throw new ArgumentNullException("contentMap");
			if (root == null) throw new ArgumentNullException("root");
			if (current == null) throw new ArgumentNullException("current");

			ContentMap = contentMap;
			Root = root;
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
			return GetTree(Root, GetTreeParentLookups(type));
		}

		protected ContentMap ContentMap { get; private set; }

		protected WebPageNode Root { get; private set; }

		private DirectoryTreeNode GetTree()
		{
			return GetTree(Root, GetTreeParentLookups());
		}

		private DirectoryTreeNode GetTree(WebPageNode directory, ILookup<EntityReference, IGrouping<EntityReference, Tuple<Entity, DirectoryType>>> parentLookup)
		{
			if (directory == null) throw new ArgumentNullException("directory");

			var directoryEntityReference = directory.ToEntityReference();

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			Func<WebPageNode, bool> langFilter = page => page.IsRoot.HasValue && page.IsRoot.Value;

			var nodeSubTrees = directory.WebPages
				.Where(child => child.Website.Id == Website.Id)
				.Where(child => TryAssertSecurity(child, CrmEntityRight.Read))
				.Where(langContext.IsCrmMultiLanguageEnabled ? langFilter : child => true)
				.Select(child => GetTree(child, parentLookup));

			var entitySubTrees = parentLookup[directoryEntityReference]
				.SelectMany(grouping => grouping)
				.Where(child => TryAssertSecurity(child.Item1, CrmEntityRight.Read))
				.Select(child => GetTree(child.Item1, child.Item2, parentLookup));

			return new DirectoryTreeNode(directory.ToEntity())
			{
				name = string.IsNullOrWhiteSpace(directory.Title) ? directory.Name : directory.Title,
				hash = new DirectoryContentHash(directoryEntityReference, true).ToString(),
				read = true,
				write = TryAssertSecurity(directory, CrmEntityRight.Change),
				dirs = nodeSubTrees
					.Concat(entitySubTrees)
					.OrderBy(node => node.name)
					.ToArray()
			};
		}

		private DirectoryTreeNode GetTree(Entity directory, DirectoryType directoryType, ILookup<EntityReference, IGrouping<EntityReference, Tuple<Entity, DirectoryType>>> parentLookup)
		{
			if (directory == null) throw new ArgumentNullException("directory");
			if (directoryType == null) throw new ArgumentNullException("directoryType");

			var directoryEntityReference = directory.ToEntityReference();

			return new DirectoryTreeNode(directory)
			{
				name = directoryType.GetDirectoryName(directory),
				hash = new DirectoryContentHash(directory, true).ToString(),
				read = true,
				write = directoryType.SupportsUpload && TryAssertSecurity(directory, CrmEntityRight.Change),
				dirs = parentLookup[directoryEntityReference]
					.SelectMany(grouping => grouping)
					.Where(child => TryAssertSecurity(child.Item1, CrmEntityRight.Read))
					.Select(child => GetTree(child.Item1, child.Item2, parentLookup))
					.OrderBy(node => node.name)
					.ToArray()
			};
		}

		private bool TryAssertSecurity(EntityNode node, CrmEntityRight right)
		{
			return TryAssertSecurity(node.ToEntity(), right);
		}
	}

	internal class WebPageNodeDirectory : Directory
	{
		public WebPageNodeDirectory(ContentMapFileSystem fileSystem, ContentMap contentMap, WebPageNode node) : base(fileSystem)
		{
			if (contentMap == null) throw new ArgumentNullException("contentMap");
			if (node == null) throw new ArgumentNullException("node");

			ContentMapFileSystem = fileSystem;
			ContentMap = contentMap;
			Node = node;
			ContentMapUrlProvider = fileSystem.ContentMapUrlProvider;
		}

		public WebPageNode Node { get; private set; }

		public override EntityReference EntityReference
		{
			get { return Node.ToEntityReference(); }
		}

		public override bool SupportsUpload
		{
			get { return true; }
		}

		public override string WebFileForeignKeyAttribute
		{
			get { return "adx_parentpageid"; }
		}

		protected ContentMap ContentMap { get; private set; }

		protected ContentMapFileSystem ContentMapFileSystem { get; private set; }

		protected IContentMapEntityUrlProvider ContentMapUrlProvider { get; private set; }

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
				mime = "application/x-adx_webpage",
				size = 0,
			};

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			Func<WebPageNode, bool> langFilter = page => page.IsRoot.HasValue && page.IsRoot.Value;

			var childPages = Node.WebPages
				.Where(langContext.IsCrmMultiLanguageEnabled ? langFilter : page => true)
				.Select(GetDirectoryContent);
			var childFiles = Node.WebFiles.Select(GetDirectoryContent);
			var childBlogs = GetRelatedEntities(new Relationship("adx_webpage_blog")).Select(GetDirectoryContent);
			var childEvents = GetRelatedEntities(new Relationship("adx_webpage_event")).Select(GetDirectoryContent);
			var childForums = GetRelatedEntities(new Relationship("adx_webpage_communityforum")).Select(GetDirectoryContent);

			var children = childPages
				.Concat(childFiles)
				.Concat(childBlogs)
				.Concat(childEvents)
				.Concat(childForums)
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
				name = string.IsNullOrWhiteSpace(Node.Title) ? Node.Name : Node.Title,
				hash = new DirectoryContentHash(EntityReference, true).ToString(),
				mime = DirectoryMimeType,
				rel = Url,
				url = Url,
				size = 0,
				date = FormatDateTime(Node.ModifiedOn),
				read = CanRead,
				write = CanWrite,
				rm = false,
			};
		}

		protected override Entity GetEntity()
		{
			var entity = Node.ToEntity();

			if (!ServiceContext.IsAttached(entity))
			{
				entity = ServiceContext.MergeClone(entity);
			}

			return entity;
		}

		protected override string GetUrl()
		{
			try
			{
				return ContentMapUrlProvider.GetUrl(ContentMap, Node);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error getting URL for entity [adx_webpage:{0}]: {1}", Node.Id, e.ToString()));

                return null;
			}
		}

		protected virtual DirectoryContent GetDirectoryContent(WebFileNode node)
		{
			if (node == null)
			{
				return null;
			}

			var fileNote = node.Annotations
				.Where(e => e.IsDocument.GetValueOrDefault())
				.OrderByDescending(e => e.CreatedOn)
				.FirstOrDefault();

			if (fileNote == null)
			{
				return null;
			}
			
			string url;

			try
			{
				url = ContentMapUrlProvider.GetUrl(ContentMap, node);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error getting URL for entity [adx_webfile:{0}]: {1}", Node.Id, e.ToString()));

                return null;
			}

			if (url == null)
			{
				return null;
			}

			var entity = node.ToEntity();

			if (!ServiceContext.IsAttached(entity))
			{
				entity = ServiceContext.MergeClone(entity);
			}

			bool canWrite;

			try
			{
				if (!SecurityProvider.TryAssert(ServiceContext, entity, CrmEntityRight.Read))
				{
					return null;
				}

				canWrite = SecurityProvider.TryAssert(ServiceContext, entity, CrmEntityRight.Change);
			}
			catch (InvalidOperationException e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error validating security for entity [adx_webfile:{0}]: {1}", Node.Id, e.ToString()));

                return null;
			}

			var azure = new Regex(@"\.azure\.txt$").IsMatch(fileNote.FileName);

			return new DirectoryContent
			{
				hash = new DirectoryContentHash(entity).ToString(),
				name = node.Name,
				mime = azure ? "application/x-azure-blob" : fileNote.MimeType,
				size = azure ? 0 : fileNote.FileSize.GetValueOrDefault(),
				url = url,
				date = FormatDateTime(node.ModifiedOn),
				read = true,
				write = canWrite,
				rm = canWrite
			};
		}

		protected virtual DirectoryContent GetDirectoryContent(WebPageNode node)
		{
			if (node == null)
			{
				return null;
			}

			var info = new WebPageNodeDirectory(ContentMapFileSystem, ContentMap, node).Info;

			info.rel = null;

			return info.read && info.url != null ? info : null;
		}

		private IEnumerable<Entity> GetRelatedEntities(Relationship relationship)
		{
			try
			{
				return Entity.GetRelatedEntities(ServiceContext, relationship).ToArray();
			}
			catch
			{
				return new Entity[] { };
			}
		}
	}
}
