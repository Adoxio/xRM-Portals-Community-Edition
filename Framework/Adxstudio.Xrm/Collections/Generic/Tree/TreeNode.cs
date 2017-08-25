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
	/// A node of a tree data structure
	/// </summary>
	/// <typeparam name="T">Type of the object for the node</typeparam>
	public class TreeNode<T> : ITreeNode<T>, IDisposable where T : new()
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public TreeNode()
		{
			_parent = null;
			_children = new TreeNodeList<T>(this);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Generic object of any value type</param>
		public TreeNode(T value)
		{
			_value = value;
			_parent = null;
			_children = new TreeNodeList<T>(this);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Generic object of any value type</param>
		/// <param name="parent">Parent tree node</param>
		public TreeNode(T value, TreeNode<T> parent)
		{
			_value = value;
			_parent = parent;
			_children = new TreeNodeList<T>(this);
		}

		private ITreeNode<T> _parent;
		public ITreeNode<T> Parent
		{
			get { return _parent; }
			set
			{
				if (value == _parent)
				{
					return;
				}

				if (_parent != null)
				{
					_parent.Children.Remove(this);
				}

				if (value != null && !value.Children.Contains(this))
				{
					value.Children.Add(this);
				}

				_parent = value;
			}
		}

		public ITreeNode<T> Root
		{
			get
			{
				return (Parent == null) ? this : Parent.Root;
			}
		}

		private readonly TreeNodeList<T> _children;
		public TreeNodeList<T> Children
		{
			get { return _children; }
		}

		private T _value;
		public T Value
		{
			get { return _value; }
			set
			{
				if (value == null && _value == null)
					return;

				if (value != null && _value != null && value.Equals(_value))
					return;

				_value = value;
			}
		}

		public void Dispose()
		{
			if (!(Value is IDisposable)) return;

			foreach (var node in Children.Cast<TreeNode<T>>())
			{
				node.Dispose();
			}

			(Value as IDisposable).Dispose();
		}

		public IEnumerable<ITreeNode<T>> Ancestors
		{
			get
			{
				if (Parent == null)
					yield break;

				yield return Parent;

				foreach (var node in Parent.Ancestors)
					yield return node;
			}
		}

		public IEnumerable<ITreeNode<T>> Descendants
		{
			get
			{
				foreach (var node in Children)
				{
					yield return node;

					foreach (var child in node.Descendants)
					{
						yield return child;
					}
				}
			}
		}

		public bool IsRoot
		{
			get { return Parent == null; }
		}
	}
}
