/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Data structure that simulates a hierarchical tree, with a root value and subtrees of children, represented as a set of linked nodes.
	/// </summary>
	/// <typeparam name="T">Type of the object for the node</typeparam>
	public class Tree<T> : TreeNode<T> where T : new()
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public Tree()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rootValue">Root object</param>
		public Tree(T rootValue)
		{
			Value = rootValue;
		}
	}
}
