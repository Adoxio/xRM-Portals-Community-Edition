/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Caching
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using System.Runtime.Caching;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Diagnostics.Trace;

	[EventSource(Guid = "7378F76F-1251-4F96-A5B6-B68EFC188C5A", Name = InternalName)]
	internal sealed class CacheEventSource : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalCache";

		/// <summary>
		/// The internal TraceSource.
		/// </summary>
		private static readonly TraceSource InternalTrace = new TraceSource(InternalName);

		/// <summary>
		/// The variable for lazy initialization of <see cref="CacheEventSource"/>.
		/// </summary>
		private static readonly Lazy<CacheEventSource> _instance = new Lazy<CacheEventSource>();

		public static CacheEventSource Log
		{
			get { return _instance.Value; }
		}

		private enum EventName
		{
			/// <summary>
			/// Insert item into cache.
			/// </summary>
			Insert = 1,

			/// <summary>
			/// Remove item from cache.
			/// </summary>
			Remove = 2,

			/// <summary>
			/// Remove all items from cache.
			/// </summary>
			RemoveAll = 3,

			/// <summary>
			/// Remove item from cache based on cache policy.
			/// </summary>
			RemovedCallback = 4
		}

		/// <summary>
		/// Log insert into cache.
		/// </summary>
		/// <param name="key">Cache dependency key.</param>
		/// <param name="region">Cache region name.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string CacheInsert(string key, string region)
		{
			WriteEventInsert(
				key,
				region,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Insert, Message = "Cache Key : {0} Region Name : {1} PortalUrl : {2} PortalVersion : {3} PortalProductionOrTrial : {4}  SessionId:{5} ElapsedTime:{6}", Level = EventLevel.Informational, Version = 3)]
		private void WriteEventInsert(string key, string region, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, TraceEventType.Information, (int)EventName.Insert,
				"Cache Key : {0} Region Name : {1}",
				key ?? string.Empty, region ?? string.Empty);

			WriteEvent(EventName.Insert, key ?? string.Empty, region ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log remove item from cache.
		/// </summary>
		/// <param name="key">Cache dependency key.</param>
		/// <param name="region">Cache region name.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string CacheRemove(string key, string region)
		{
			WriteEventRemove(
				key,
				region,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Remove, Message = "Cache Key : {0} Region Name : {1} PortalUrl : {2} PortalVersion : {3} PortalProductionOrTrial : {4}  SessionId:{5} ElapsedTime:{6}", Level = EventLevel.Informational, Version = 3)]
		private void WriteEventRemove(string key, string region, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, TraceEventType.Information, (int)EventName.Remove,
				"Cache Key : {0} Region Name : {1}",
				key ?? string.Empty, region ?? string.Empty);

			WriteEvent(EventName.Remove, key ?? string.Empty, region ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log remove all entries from cache.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string CacheRemoveAll()
		{
			WriteEventRemoveAll(
				"Write Event Remove All",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.RemoveAll, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3}  SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Informational, Version = 3)]
		private void WriteEventRemoveAll(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(TraceEventType.Information, (int)EventName.RemoveAll, "Write Event Remove All");

			WriteEvent(EventName.RemoveAll, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log remove item from cache based on cache policy.
		/// </summary>
		/// <param name="args">Cache removal arguments.</param>
		[NonEvent]
		public void OnRemovedCallback(CacheEntryRemovedArguments args)
		{
			CacheRemovedCallback(args.CacheItem.Key, args.CacheItem.RegionName, args.RemovedReason.ToString());
		}

		/// <summary>
		/// Log remove item from cache based on cache policy.
		/// </summary>
		/// <param name="key">Cache dependency key.</param>
		/// <param name="region">Cache region name.</param>
		/// <param name="reason">Reason for removal.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string CacheRemovedCallback(string key, string region, string reason = null)
		{
			WriteEventRemovedCallback(
				key,
				region,
				reason,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.RemovedCallback, Message = "Cache Key : {0} Region Name : {1} Reason : {2} PortalUrl : {3} PortalVersion : {4} PortalProductionOrTrial : {5}  SessionId:{6} ElapsedTime:{7}", Level = EventLevel.Informational, Version = 3)]
		private void WriteEventRemovedCallback(string key, string region, string reason, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, TraceEventType.Information, (int)EventName.RemovedCallback,
				"Cache Key : {0} Region Name : {1} Reason : {2}",
				key ?? string.Empty, region ?? string.Empty, reason ?? string.Empty);

			WriteEvent(EventName.RemovedCallback, key ?? string.Empty, region ?? string.Empty, reason ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}
	}
}
