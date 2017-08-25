/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// List of tree nodes
	/// </summary>
	/// <typeparam name="T">Type of the object for the node.</typeparam>
	public class TreeNodeList<T> : List<ITreeNode<T>>, ITreeNodeList<T>
	{
		/// <summary>
		/// Parent tree node
		/// </summary>
		public ITreeNode<T> Parent { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Parent tree node</param>
		public TreeNodeList(ITreeNode<T> parent)
		{
			Parent = parent;
		}

		public new ITreeNode<T> Add(ITreeNode<T> node)
		{
			base.Add(node);
			node.Parent = Parent;
			return node;
		}

		public new bool Remove(ITreeNode<T> node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (!Contains(node))
				return false;

			var result = base.Remove(node);

			return result;
		}

		/// <summary>
		/// Adds the elements of the specified collection to the end of the list
		/// </summary>
		/// <param name="nodes">The collection whose elements should be added to the end of the List</param>
		public new void AddRange(IEnumerable<ITreeNode<T>> nodes)
		{
			var treeNodes = nodes as ITreeNode<T>[] ?? nodes.ToArray();
			base.AddRange(treeNodes);
			foreach (var node in treeNodes)
			{
				node.Parent = Parent;
			}
		}

		public override string ToString()
		{
			return "Count=" + Count;
		}
	}
}
