/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Encapsulates constants pertaining to the BrokeredMessage
	/// </summary>
	internal class BrokeredMessageConstants
	{
		/// <summary>
		/// Time in UTC in which the message was enqueued in the Eventhub
		/// </summary>
		public const string EnqueuedEventhubTimeUtc = "EnqueuedEventhubTimeUtc";

		/// <summary>
		/// Time in UTC in which the message was dequeued from the Eventhub
		/// </summary>
		public const string DequeuedEventhubTimeUtc = "DequeuedEventhubTimeUtc";

		/// <summary>
		/// Time in UTC in which the message was enqueued in the Topic
		/// </summary>
		public const string EnqueuedTopicTimeUtc = "EnqueuedTopicTimeUtc";

		/// <summary>
		/// The Organization Id for which this message belongs
		/// </summary>
		public const string OrganizationId = "OrganizationId";
	}
}
