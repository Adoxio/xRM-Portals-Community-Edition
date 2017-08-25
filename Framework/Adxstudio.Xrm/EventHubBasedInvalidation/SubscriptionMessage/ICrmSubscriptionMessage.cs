/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;

	/// <summary>
	/// Interface for for the ICrmSubscriptionMessage types
	/// </summary>
	public interface ICrmSubscriptionMessage
	{
		/// <summary>
		/// OrganizationId
		/// </summary>
		Guid OrganizationId { get; }

		/// <summary>
		/// MessageId
		/// </summary>
		Guid MessageId { get; }

		/// <summary>
		/// MessageName
		/// 	Create, Update, Delete, Associate, Dissociate
		/// </summary>
		string MessageName { get; }

		/// <summary>
		/// DateTime in UTC when this message was received
		/// </summary>
		DateTime Received { get; }

		/// <summary>
		/// Type of this message as an enum
		/// </summary>
		MessageType MessageType { get; }

		/// <summary>
		/// Time in UTC in which the message was enqueued in the Topic
		/// </summary>
		DateTime EnqueuedTopicTimeUtc { get; }

		/// <summary>
		/// Time in UTC in which the message was dequeued from the Topic
		/// </summary>
		DateTime DequeuedTopicTimeUtc { get; }

		/// <summary>
		/// Time in UTC in which the message was enqueued in the Eventhub
		/// </summary>
		DateTime EnqueuedEventhubTimeUtc { get; }

		/// <summary>
		/// Time in UTC in which the message was dequeued from the Eventhub
		/// </summary>
		DateTime DequeuedEventhubTimeUtc { get; }
	}
}
