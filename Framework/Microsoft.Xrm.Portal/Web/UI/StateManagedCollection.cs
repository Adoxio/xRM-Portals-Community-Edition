/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Security.Permissions;
using System.Web;
using System.Collections;
using System;

namespace Microsoft.Xrm.Portal.Web.UI
{
	public interface IStateManagedItem : IStateManager
	{
		void SetDirty();
	}

	[System.Web.AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class StateManagedCollection<T> : StateManagedCollection where T : IStateManagedItem
	{
		public int Add(T parameter)
		{
			return ((IList)this).Add(parameter);
		}

		public bool Contains(T parameter)
		{
			return ((IList)this).Contains(parameter);
		}

		public void CopyTo(T[] parameterArray, int index)
		{
			base.CopyTo(parameterArray, index);
		}

		public int IndexOf(T parameter)
		{
			return ((IList)this).IndexOf(parameter);
		}

		public void Insert(int index, T parameter)
		{
			((IList)this).Insert(index, parameter);
		}

		protected override void OnValidate(object o)
		{
			base.OnValidate(o);

			if (!(o is T))
			{
				throw new ArgumentException("o");
			}
		}

		public void Remove(T parameter)
		{
			((IList)this).Remove(parameter);
		}

		public void RemoveAt(int index)
		{
			((IList)this).RemoveAt(index);
		}

		protected override void SetDirtyObject(object o)
		{
			((T)o).SetDirty();
		}

		public T this[int index]
		{
			get
			{
				return (T)this[index];
			}
			set
			{
				this[index] = value;
			}
		}
	}
}
