/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Performance
{
	public interface IPerformanceProfiler
	{
		void AddMarker(string name, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null);

		IPerformanceMarker StartMarker(string name, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null);

		void StopMarker(IPerformanceMarker marker);

		TimeSpan UsingMarker(string name, Action<IPerformanceMarker> action, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null);

		TimeSpan UsingMarker(string name, Action action, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null);
	}
}
