/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Container class for the Time Tracking Telemetry entry
	/// </summary>
	internal sealed class TimeTrackingEntry
	{
		public TimeTrackingEntry(string entity, DateTime pushed, DateTime modified, DateTime received)
		{
			entity.ThrowOnNullOrWhitespace("entity");
			pushed.ThrowOnNull("pushed");
			modified.ThrowOnNull("modified");
			received.ThrowOnNull("received");

			this.EntityLogicalName = entity;
			this.PushedToCache = pushed;
			this.ModifiedInCrm = modified;
			this.ReceivedInPortal = received;
		}

		public string EntityLogicalName { get; private set; }

		public DateTime PushedToCache { get; private set; }

		public DateTime ModifiedInCrm { get; private set; }

		public DateTime ReceivedInPortal { get; private set; }

		/// <summary>
		/// Overall Delta from the change in CRM until the cache is updated in the portal
		/// </summary>
		public TimeSpan OverallDelta
		{
			get { return this.PushedToCache - this.ModifiedInCrm; }
		}

		/// <summary>
		/// Delta from the change in CRM until the portal was notified
		/// </summary>
		public TimeSpan AzureProcessingDelta
		{
			get { return this.ReceivedInPortal - this.ModifiedInCrm; }
		}

		/// <summary>
		/// Delta from the change in CRM until the portal was notified
		/// </summary>
		public TimeSpan InvalidationDelta
		{
			get { return this.PushedToCache - this.ReceivedInPortal; }
		}
	}
}
