/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Represents an assembly attribute that registers another assembly of statically served embedded resource files.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class EmbeddedResourceAssemblyAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of the assembly containing the embedded resources.
		/// </summary>
		public Assembly Assembly { get; private set; }

		/// <summary>
		/// Gets the default namespace.
		/// </summary>
		public string Namespace { get; private set; }

		/// <summary>
		/// Gets the virtual directory path pattern to match against.
		/// </summary>
		public string VirtualPathMask { get; private set; }

		/// <summary>
		/// Gets the flag allowing the previous virtual path provider to take precedence.
		/// </summary>
		public bool AllowOverride { get; set; }

		/// <summary>
		/// Gets the flag allowing a default document transfer to occur.
		/// </summary>
		public bool AllowDefaultDocument { get; set; }

		/// <summary>
		/// Gets the set of resources contained in the assembly.
		/// </summary>
		public EmbeddedResourceNode Resources { get; private set; }

		/// <summary>
		/// Gets a lookup from resource name to resource node.
		/// </summary>
		public IDictionary<string, EmbeddedResourceNode> ResourceLookup { get; private set; }

		public EmbeddedResourceAssemblyAttribute(string virtualPathMask, string assembly, string theNamespace)
		{
			VirtualPathMask = virtualPathMask;
			Namespace = theNamespace;
			AllowOverride = true;
			AllowDefaultDocument = true;

			try
			{
				// load the related assembly and resource tree

				Assembly = Assembly.Load(assembly);

				ResourceLookup = new Dictionary<string, EmbeddedResourceNode>(StringComparer.InvariantCultureIgnoreCase);
				Resources = new EmbeddedResourceNode(string.Empty, true, false, "~", Namespace);

				// convert resource names to virtual paths

				var paths = GetVirtualPaths(Assembly.GetManifestResourceNames());

				foreach (var path in paths)
				{
					AddResource(Resources, ResourceLookup, path.VirtualPath, path.ResourceName);
				}
			}
			catch (FileNotFoundException)
			{
				// Silently fail if the resource assembly isn't found.
			}
		}

		public EmbeddedResourceNode FindResource(string path)
		{
			if (ResourceLookup == null) return null;

			var resourceName = ConvertVirtualPathToResourceName(path);

			EmbeddedResourceNode node;
			
			if (ResourceLookup.TryGetValue(resourceName, out node))
			{
				return node;
			}

			return null;
		}

		private static readonly string[] _directoryDelimiters = new[] { "/", @"\", "~" };

		private static void AddResource(EmbeddedResourceNode resources, IDictionary<string, EmbeddedResourceNode> lookup, string path, string resourceName)
		{
			var parts = path.Split(_directoryDelimiters, StringSplitOptions.RemoveEmptyEntries);
			AddResource(resources, lookup, parts, resourceName);
		}

		private static void AddResource(EmbeddedResourceNode node, IDictionary<string, EmbeddedResourceNode> lookup, IEnumerable<string> path, string resourceName)
		{
			if (path.Any())
			{
				var name = path.First();

				var child = node.Children.SingleOrDefault(n => n.Name == name);
				
				if (child == null)
				{
					var isFile = path.Count() == 1;
					var resource = isFile ? resourceName : GetResourceName(node, name);

					child = new EmbeddedResourceNode(name, !isFile, isFile, GetVirtualPath(node, name), resource);

					node.Children.Add(child);
					lookup.Add(resource, child);
				}

				AddResource(child, lookup, path.Skip(1), resourceName);
			}
		}

		private static string GetVirtualPath(EmbeddedResourceNode node, string name)
		{
			return "{0}/{1}".FormatWith(node.VirtualPath, name);
		}

		private static string GetResourceName(EmbeddedResourceNode node, string name)
		{
			return "{0}.{1}".FormatWith(node.VirtualPath, name);
		}

		private struct Pair
		{
			public string VirtualPath;
			public string ResourceName;
		}

		private IEnumerable<Pair> GetVirtualPaths(IEnumerable<string> resourceNames)
		{
			var files = Assembly.GetCustomAttributes(typeof(EmbeddedResourceFileAttribute), true).Cast<EmbeddedResourceFileAttribute>();

			var lookup = new Dictionary<string, string>();

			foreach (var file in files)
			{
				lookup[file.ResourceName] = file.VirtualPath;
			}

			foreach (var resourceName in resourceNames)
			{
				var virtualPath = lookup.ContainsKey(resourceName)
					? lookup[resourceName]
					: ConvertResourceNameToVirtualPath(resourceName);
				yield return new Pair { VirtualPath = virtualPath, ResourceName = resourceName };
			}
		}

		private string ConvertResourceNameToVirtualPath(string resourceName)
		{
			// generic algorithm for turning a resource name into a virtual path
			// replace every dot with a slash except for the last dot
			// chop off the leading default namespace

			var index1 = resourceName.LastIndexOf('.');
			var index2 = index1 + 1;
			var head = Regex.Replace(resourceName.Substring(0, index1), "^" + Namespace, string.Empty, RegexOptions.IgnoreCase).Replace('.', '/');
			var tail = resourceName.Substring(index2, resourceName.Length - index2);

			return "~/{0}.{1}".FormatWith(head, tail);
		}

		private string ConvertVirtualPathToResourceName(string virtualPath)
		{
			// converting an entire path
			// for all parts: prepend an '_' if the name starts with a numeric character
			// replace all '/' or '\\' with '.'
			// prepend the default namespace
			// besides a leading underscore, filenames remain unchanged

			var parts = virtualPath.Split(_directoryDelimiters, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Any())
			{
				var partsWithUnderscores = parts.Select(p => Regex.IsMatch(p, @"^\d") ? "_" + p : p);
				var directories = partsWithUnderscores.Take(parts.Length - 1).Select(ConvertDirectoryToResourceName);
				var head = directories.Aggregate(Namespace, (h, d) => "{0}.{1}".FormatWith(h, d)).Replace('-', '_');
				var tail = partsWithUnderscores.Last();
				return "{0}.{1}".FormatWith(head, tail);
			}

			return null;
		}

		private static string ConvertDirectoryToResourceName(string directory)
		{
			// converting and individual directory
			// for all parts: prepend an '_' if the name starts with a numeric character
			// convert '-' to '_'

			var parts = directory.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Any())
			{
				var partsWithUnderscores = parts.Select(p => Regex.IsMatch(p, @"^\d") ? "_" + p : p);
				return string.Join(".", partsWithUnderscores.ToArray()).Replace('-', '_');
			}

			return null;
		}
	}
}
