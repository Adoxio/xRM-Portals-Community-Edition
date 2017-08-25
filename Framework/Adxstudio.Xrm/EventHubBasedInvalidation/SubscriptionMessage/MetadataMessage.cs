/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using Microsoft.ServiceBus.Messaging;

	/// <summary>
	/// CrmSubscriptionMessage wrapping a BrokeredMessage indicating a CRM Metadata change
	/// </summary>
	public sealed class MetadataMessage : CrmSubscriptionMessage
	{
		/// <summary>
		/// Deserialize the BrokeredMessage message body into a MetadataMessage
		/// </summary>
		/// <param name="message">BrokeredMessage message body</param>
		/// <param name="brokeredMessage">BrokeredMessage message</param>
		/// <returns>ICrmSubscriptionMessage</returns>
		internal static ICrmSubscriptionMessage DeserializeMessage(string message, BrokeredMessage brokeredMessage)
		{
			MetadataMessage metadataMessage = (MetadataMessage)CrmSubscriptionMessage.DeserializeMessage(message, typeof(MetadataMessage));

			if (metadataMessage != null && metadataMessage.ValidMessage)
			{
				metadataMessage.AppendProperties(brokeredMessage);
				return metadataMessage;
			}

			return null;
		}
	}
}
