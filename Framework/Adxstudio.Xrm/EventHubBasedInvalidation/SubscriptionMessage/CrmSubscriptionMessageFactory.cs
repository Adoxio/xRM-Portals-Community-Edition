/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System.Collections.Generic;
	using Microsoft.ServiceBus.Messaging;
	using Adxstudio.Xrm.Cms;
	using Newtonsoft.Json;

	/// <summary>
	/// Factory class to generate ICrmSubscriptionMessages
	/// </summary>
	internal sealed class CrmSubscriptionMessageFactory
	{
		/// <summary>
		/// Factory method that generates an ICrmSubscriptionMessage
		/// </summary>
		/// <param name="message">BrokeredMessage</param>
		/// <returns>ICrmSubscriptionMessage</returns>
		public static ICrmSubscriptionMessage Create(BrokeredMessage message)
		{
			if (message == null)
				return null;

			string messageBody = message.GetBody<string>();

			if (messageBody == null)
				return null;
			
			ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Message Body for Subscription message is {0} ", messageBody.ToString()));

			ICrmSubscriptionMessage subscriptionMessage = CrmSubscriptionMessageFactory.Create(messageBody, message);

			if (subscriptionMessage != null)
			{
				CmsEventSource.Log.LatencyInfo(subscriptionMessage);
			}

			return subscriptionMessage;
		}

		private static ICrmSubscriptionMessage Create(string messageBody, BrokeredMessage message)
		{
			Dictionary<string, string> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(messageBody);

			if (!jsonDictionary.ContainsKey("MessageName"))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Unexpected message format. MessageId: {0} ", message.MessageId));
				return null;
			}

			switch (jsonDictionary["MessageName"])
			{
				case "MetadataChange":
					NotificationUpdateManager.Instance.MetadataDirty = true;
					return MetadataMessage.DeserializeMessage(messageBody, message);
				case "Create":
				case "Update":
				case "Delete":
					return EntityRecordMessage.DeserializeMessage(messageBody, message);
				case "AssociateEntities":
				case "DisassociateEntities":
					return AssociateDisassociateMessage.DeserializeMessage(messageBody, message);
				default:
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Unexpected message type: {0} ", jsonDictionary["MessageName"]));
					return null;
			}
		}
	}
}
