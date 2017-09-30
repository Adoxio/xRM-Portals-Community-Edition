/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics;
using System.Web;
using Adxstudio.Xrm.Performance.AggregateEvent;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Performance
{
	public sealed class PerformanceProfiler : IPerformanceProfiler
	{
		private static readonly Lazy<PerformanceProfiler> _instance = new Lazy<PerformanceProfiler>(CreateProfiler);

		public static PerformanceProfiler Instance
		{
			get { return _instance.Value; }
		}

		private static PerformanceProfiler CreateProfiler()
		{
			return new PerformanceProfiler(new PerformanceAggregateLogger(new EventSourcePerformanceLogger()), new GuidIdGenerator());
		}

		private readonly IPerformanceLogger _logger;
		private readonly IPerformanceMarkerIdGenerator _idGenerator;

		private PerformanceProfiler(IPerformanceLogger logger, IPerformanceMarkerIdGenerator idGenerator)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			if (idGenerator == null) throw new ArgumentNullException("idGenerator");

			_logger = logger;
			_idGenerator = idGenerator;
		}

		public void AddMarker(string name, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
		{
			_logger.Log(new PerformanceMarker(GetId(), name, GetTimestamp(), GetRequestId(), GetSessionId(), area, tag));
		}

		public IPerformanceMarker StartMarker(string name, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
		{
			return new StopwatchPerformanceMarker(this, GetId(), name, GetTimestamp(), GetRequestId(), GetSessionId(), area, tag);
		}

		public void StopMarker(IPerformanceMarker marker)
		{
			if (marker == null)
			{
				// Err on the side of not having performance code throw any errors.
				return;
			}

			marker.Stop();
			_logger.Log(marker);
		}

		public TimeSpan UsingMarker(string name, Action<IPerformanceMarker> action, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
		{
			var marker = StartMarker(name, area, tag);

			using (marker)
			{
				action(marker);
			}
			return marker.Elapsed.GetValueOrDefault();
		}

		public TimeSpan UsingMarker(string name, Action action, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
		{
			var marker = StartMarker(name, area, tag);

			using (marker)
			{
				action();
			}
			return marker.Elapsed.GetValueOrDefault();
		}

		private string GetId()
		{
			return _idGenerator.GetId();
		}

		private string GetRequestId()
		{
			try
			{
				var context = HttpContext.Current;

				return context == null ? null : context.Request.AnonymousID;
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericWarningException(e);
			}

			return null;
		}

		private string GetSessionId()
		{
			var context = HttpContext.Current;

			return context == null || context.Session == null ? null : context.Session.SessionID;
		}

		private DateTime GetTimestamp()
		{
			return DateTime.UtcNow;
		}

		private class PerformanceMarker : IPerformanceMarker
		{
			public PerformanceMarker(string id, string name, DateTime timestamp, string requestId, string sessionId, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
			{
				Id = id;
				Name = name;
				Timestamp = timestamp;
				RequestId = requestId;
				SessionId = sessionId;
				Area = area;
				Tag = tag;
			}

			public PerformanceMarkerArea Area { get; private set; }

			public virtual TimeSpan? Elapsed
			{
				get { return null; }
				protected set { }
			}

			public string Id { get; private set; }

			public string Name { get; private set; }

			public string RequestId { get; private set; }

			public string SessionId { get; private set; }

			public PerformanceMarkerSource Source
			{
				get { return PerformanceMarkerSource.Server; }
			}

			public DateTime Timestamp { get; private set; }

			public virtual PerformanceMarkerType Type
			{
				get { return PerformanceMarkerType.Marker; }
			}

			public string Tag { get; private set; }

			public virtual void Dispose() { }

			public virtual void Stop() { }
		}

		private class StopwatchPerformanceMarker : PerformanceMarker
		{
			private readonly IPerformanceProfiler _profiler;
			private bool _stopped;
			private readonly Stopwatch _stopwatch;

			public StopwatchPerformanceMarker(IPerformanceProfiler profiler, string id, string name, DateTime timestamp, string requestId, string sessionId, PerformanceMarkerArea area = PerformanceMarkerArea.Unknown, string tag = null)
				: base(id, name, timestamp, requestId, sessionId, area, tag)
			{
				if (profiler == null) throw new ArgumentNullException("profiler");

				_profiler = profiler;
				_stopwatch = Stopwatch.StartNew();
			}

			public override TimeSpan? Elapsed { get; protected set; }

			public override PerformanceMarkerType Type
			{
				get { return PerformanceMarkerType.Stopwatch; }
			}

			public override void Dispose()
			{
				_profiler.StopMarker(this);
			}

			public override void Stop()
			{
				if (_stopped)
				{
					return;
				}

				_stopwatch.Stop();

				Elapsed = _stopwatch.Elapsed;

				_stopped = true;
			}
		}
	}
}
