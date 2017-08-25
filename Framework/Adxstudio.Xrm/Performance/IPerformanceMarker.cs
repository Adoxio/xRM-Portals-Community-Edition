/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Performance
{
	public interface IPerformanceMarker : IDisposable
	{
		PerformanceMarkerArea Area { get; }

		TimeSpan? Elapsed { get; }

		string Id { get; }

		string Name { get; }

		string RequestId { get; }

		string SessionId { get; }

		PerformanceMarkerSource Source { get; }

		DateTime Timestamp { get; }

		PerformanceMarkerType Type { get; }

		string Tag { get; }

		void Stop();
	}
}
