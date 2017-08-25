/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Class containing constants for the EventHubBasedInvalidation work
	/// </summary>
	public class Constants
	{
		public const string TimeTrackingTelemetry = "TimeTrackingTelemetry";

		// Cache Keys
		public const string DirtyTableKey = "DirtyTable_Key";
		public const string ProcessingTableKey = "ProcessingTable_Key";
		public const string TimeStampTableKey = "TimeStampTable_Key";
		public const string MetadataKey = "Metadata_Key";
		public const string NotificationUrlKey = "NotificationUrl_Key";

		// Message Names
		public const string RemovedOrDeleted = "Delete";
		public const string CreatedOrUpdated = "Update";
		public const string Metadata = "PublishAll";
		public const string Associate = "Associate";
		public const string Disassociate = "Disassociate";
	}
}
