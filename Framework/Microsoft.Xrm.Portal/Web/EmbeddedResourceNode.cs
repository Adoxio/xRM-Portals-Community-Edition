/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// An embedded resource record.
	/// </summary>
	public class EmbeddedResourceNode
	{
		public EmbeddedResourceNode(string name, bool isDirectory, bool isFile, string virtualPath, string resourceName)
		{
			Name = name;
			IsDirectory = isDirectory;
			IsFile = isFile;
			VirtualPath = virtualPath;
			ResourceName = resourceName;
			_children = new Lazy<ICollection<EmbeddedResourceNode>>(() => new List<EmbeddedResourceNode>());
		}

		/// <summary>
		/// The local name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Indicates that the node is a directory.
		/// </summary>
		public bool IsDirectory { get; private set; }

		/// <summary>
		/// Indicates that the node is a file.
		/// </summary>
		public bool IsFile { get; private set; }
		
		/// <summary>
		/// The virtual path of the node.
		/// </summary>
		public string VirtualPath { get; private set; }

		/// <summary>
		/// The resource name.
		/// </summary>
		public string ResourceName { get; private set; }

		private readonly Lazy<ICollection<EmbeddedResourceNode>> _children;

		/// <summary>
		/// The child nodes.
		/// </summary>
		public ICollection<EmbeddedResourceNode> Children
		{
			get { return _children.Value; }
		}
	}
}
