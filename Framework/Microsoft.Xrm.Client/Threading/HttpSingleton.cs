/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;

namespace Microsoft.Xrm.Client.Threading
{
	internal static class HttpSingleton<T> where T : class
	{
		private static readonly string _typeName = typeof(T).FullName;
		private static readonly object _lock = new object();

		private static T GetCurrent(string key)
		{
			return HttpContext.Current.Items[key] as T;
		}

		public static bool Enabled
		{
			get { return HttpContext.Current != null; }
		}

		public static T GetInstance(string name, Func<T> create)
		{
			var key = GetKey(name);
			var current = GetCurrent(key);

			if (current == null)
			{
				lock (_lock)
				{
					current = GetCurrent(key);

					if (current == null)
					{
						current = create();
						Thread.MemoryBarrier();
						HttpContext.Current.Items[key] = current;
					}
				}
			}

			return current;
		}

		public static void Dispose(string portalName)
		{
			var key = GetKey(portalName);
			var current = HttpContext.Current.Items[key] as IDisposable;

			if (current != null)
			{
				lock (_lock)
				{
					current = HttpContext.Current.Items[key] as IDisposable;

					if (current != null)
					{
						current.Dispose();
					}

					HttpContext.Current.Items[key] = null;
				}
			}
		}

		private static string GetKey(string name)
		{
			return "type={0}:name={1}".FormatWith(_typeName, name);
		}
	}
}
