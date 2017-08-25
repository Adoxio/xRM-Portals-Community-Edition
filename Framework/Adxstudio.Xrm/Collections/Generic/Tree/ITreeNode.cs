/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Represents a node of a tree
	/// </summary>
	/// <typeparam name="T">The type of object for the node.</typeparam>
	public interface ITreeNode<T>
	{
		/// <summary>
		/// Root node of the tree
		/// </summary>
		ITreeNode<T> Root { get; }

		/// <summary>
		/// The parent node
		/// </summary>
		ITreeNode<T> Parent { get; set; }

		/// <summary>
		/// Object assigned to the node
		/// </summary>
		T Value { get; set; }

		/// <summary>
		/// Child nodes
		/// </summary>
		TreeNodeList<T> Children { get; }

		/// <summary>
		/// Any node from whom this node is descended. Recursively the parent of an ancestor (i.e. a grandparent, great-grandparent, great-great-grandparent, and so forth).
		/// </summary>
		IEnumerable<ITreeNode<T>> Ancestors { get; }

		/// <summary>
		/// The children, grandchildren, great-grandchildren, etc.
		/// </summary>
		IEnumerable<ITreeNode<T>> Descendants { get; }

		/// <summary>
		/// Indicates if this node is the root node.
		/// </summary>
		bool IsRoot { get; }
	}
}
