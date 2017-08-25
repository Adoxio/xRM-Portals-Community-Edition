/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;
using Microsoft.Xrm.Client.Diagnostics;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// A path provider for serving static files from embedded resources of a registered assembly.
	/// </summary>
	public sealed class EmbeddedResourceVirtualPathProvider : VirtualPathProvider
	{
		private class EmbeddedResourceVirtualFile : VirtualFile
		{
			private readonly Assembly _assembly;
			private readonly string _resourceName;

			public EmbeddedResourceVirtualFile(string virtualPath, Assembly assembly, EmbeddedResourceNode resources)
				: this(virtualPath, assembly, resources.ResourceName)
			{
			}

			public EmbeddedResourceVirtualFile(string virtualPath, Assembly assembly, string resourceName)
				: base(virtualPath)
			{
				_assembly = assembly;
				_resourceName = resourceName;
			}

			public override Stream Open()
			{
				Tracing.FrameworkInformation("EmbeddedResourceVirtualFile", "Open", "_resourceName={0}", _resourceName);

				return _assembly.GetManifestResourceStream(_resourceName);
			}
		}

		private class EmbeddedResourceVirtualDirectory : VirtualDirectory
		{
			private readonly Assembly _assembly;
			private readonly EmbeddedResourceNode _resources;

			public EmbeddedResourceVirtualDirectory(string virtualPath, Assembly assembly, EmbeddedResourceNode resources)
				: base(virtualPath)
			{
				_assembly = assembly;
				_resources = resources;
			}

			private IEnumerable _children;

			public override IEnumerable Children
			{
				get
				{
					if (_children == null)
					{
						_children = LoadChildren();
					}

					return _children;
				}
			}

			private IEnumerable _directories;

			public override IEnumerable Directories
			{
				get
				{
					if (_directories == null)
					{
						_directories = (
							from object child in Children
							where child is EmbeddedResourceVirtualDirectory
							select child).ToList();
					}

					return _directories;
				}
			}

			private IEnumerable _files;

			public override IEnumerable Files
			{
				get
				{
					if (_files == null)
					{
						_files = (
							from object child in Children
							where child is EmbeddedResourceVirtualFile
							select child).ToList();
					}

					return _files;
				}
			}

			private IEnumerable LoadChildren()
			{
				return (
					from resource in _resources.Children
					let child = CreateChild(resource)
					where child != null
					select child).ToList();
			}

			private object CreateChild(EmbeddedResourceNode resource)
			{
				if (resource.IsDirectory)
				{
					var virtualPath = Path.Combine(VirtualPath, resource.Name);
					return new EmbeddedResourceVirtualDirectory(virtualPath, _assembly, resource);
				}

				if (resource.IsFile)
				{
					var virtualPath = Path.Combine(VirtualPath, resource.Name);
					return new EmbeddedResourceVirtualFile(virtualPath, _assembly, resource.ResourceName);
				}

				return null;
			}
		}

		public IEnumerable<EmbeddedResourceAssemblyAttribute> Mappings { get; private set; }

		protected override void Initialize()
		{
			base.Initialize();

			// load the mappings from all available assemblies

			Mappings = Enumerable.ToList<EmbeddedResourceAssemblyAttribute>(Utility.GetEmbeddedResourceMappingAttributes());
		}

		public override bool FileExists(string virtualPath)
		{
			var exists =
				ResourceExists(virtualPath, (assembly, resources) => resources.IsFile)
					|| Previous.FileExists(virtualPath);

			Tracing.FrameworkInformation("EmbeddedResourceVirtualPathProvider", "FileExists", "virtualPath={0}, exists={1}", virtualPath, exists);

			return exists;
		}

		public override bool DirectoryExists(string virtualDir)
		{
			var exists =
				ResourceExists(virtualDir, (assembly, resources) => resources.IsDirectory)
					|| Previous.DirectoryExists(virtualDir);

			Tracing.FrameworkInformation("EmbeddedResourceVirtualPathProvider", "DirectoryExists", "virtualDir={0}, exists={1}", virtualDir, exists);

			return exists;
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			// file remains null if it exists in the filesystem and AllowOverrides is true

			VirtualFile file = null;

			ResourceExists(
				virtualPath,
				(assembly, resources) =>
				{
					file = new EmbeddedResourceVirtualFile(virtualPath, assembly, resources);
					return true;
				});

			file = file ?? Previous.GetFile(virtualPath);

			return file;
		}

		public override VirtualDirectory GetDirectory(string virtualDir)
		{
			// directory remains null if it exists in the filesystem and AllowOverrides is true

			VirtualDirectory directory = null;

			ResourceExists(
				virtualDir,
				(assembly, resources) =>
				{
					directory = new EmbeddedResourceVirtualDirectory(virtualDir, assembly, resources);
					return true;
				});

			directory = directory ?? Previous.GetDirectory(virtualDir);

			return directory;
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			Tracing.FrameworkInformation("EmbeddedResourceVirtualPathProvider", "GetCacheDependency", "virtualPath={0}, utcStart={1}", virtualPath, utcStart);

			Assembly files = null;

			ResourceExists(virtualPath, (assembly, resources) => { files = assembly; return true; });

			// create a dependency on the assembly containing the embedded resources

			return files != null ? new CacheDependency(files.Location, utcStart) : null;
		}

		private bool ResourceExists(string virtualPath, Func<Assembly, EmbeddedResourceNode, bool> onResources)
		{
			var mapping = Mappings.Match(virtualPath);

			if (mapping != null)
			{
				// found a matching mapping

				// check if this file exists on the filesystem

				if (mapping.AllowOverride && Previous.FileExists(virtualPath))
				{
					return true;
				}

				// check if this file exists as an embedded resource

				var resources = mapping.FindResource(virtualPath);

				if (resources != null)
				{
					return onResources(mapping.Assembly, resources);
				}
			}

			return false;
		}
	}
}
