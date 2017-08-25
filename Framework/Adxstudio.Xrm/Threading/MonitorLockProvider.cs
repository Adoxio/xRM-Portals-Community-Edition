/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Diagnostics;
using System.Threading;
using Microsoft.Xrm.Client.Threading;

namespace Adxstudio.Xrm.Threading
{
	/// <summary>
	/// A lock provider based on the <see cref="Monitor"/> class.
	/// </summary>
	public class MonitorLockProvider : LockProvider
	{
		private readonly NamedLock _lock = new NamedLock();

		public override void Lock(string key, int millisecondsTimeout, System.Action action)
		{
			var timer = Stopwatch.StartNew();

			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, string.Format("Lock Requested: {0}, Duration: {1} ms", key, timer.ElapsedMilliseconds));

			using (_lock.Lock(key, millisecondsTimeout))
			{
				ADXTrace.Instance.TraceVerbose(TraceCategory.Application, string.Format("Lock Acquired: {0}, Duration: {1} ms", key, timer.ElapsedMilliseconds));

				action();
			}

			timer.Stop();

			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, string.Format("Lock Released: {0}, Duration: {1} ms", key, timer.ElapsedMilliseconds));
		}
	}
}
