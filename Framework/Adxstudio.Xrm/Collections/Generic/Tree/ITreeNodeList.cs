/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Represents the tree as a list
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ITreeNodeList<T> : IList<ITreeNode<T>>
	{
		/// <summary>
		/// Add a node
		/// </summary>
		new ITreeNode<T> Add(ITreeNode<T> node);
	}
}
