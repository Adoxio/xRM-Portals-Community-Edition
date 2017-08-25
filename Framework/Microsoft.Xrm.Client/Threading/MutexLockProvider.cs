/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;

namespace Microsoft.Xrm.Client.Threading
{
	/// <summary>
	/// A lock provider based on the <see cref="Mutex"/> class.
	/// </summary>
	public class MutexLockProvider : LockProvider
	{
		public override void Lock(string key, int millisecondsTimeout, Action action)
		{
			MutexExtensions.Lock(key, millisecondsTimeout, _ => action());
		}
	}
}
