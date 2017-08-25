/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Core.Telemetry.EventSources
{
	using System;
	using System.Diagnostics.Tracing;

	[EventSource(Guid = "CCA89716-D3EB-4255-A022-BB960941D578", Name = InternalName)]
	internal sealed class WebNotificationEventSource : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalWebNotification";

		private static readonly Lazy<WebNotificationEventSource> _instance = new Lazy<WebNotificationEventSource>();

		public static WebNotificationEventSource Log
		{
			get { return _instance.Value; }
		}

		private enum EventName
		{
			/// <summary>
			/// Authorization header not valid.
			/// </summary>
			AuthorizationHeaderInvalid = 1,

			/// <summary>
			/// Authorization header is not specified.
			/// </summary>
			AuthorizationHeaderMissing = 2,

			/// <summary>
			/// Failure to validate authorization header.
			/// </summary>
			AuthorizationValidationFailed = 3,

			/// <summary>
			/// Invalid content type.
			/// </summary>
			ContentTypeInvalid = 4,

			/// <summary>
			/// Deserialization of message failed.
			/// </summary>
			MessageDeserializationFailed = 5,

			/// <summary>
			/// Invalid body message.
			/// </summary>
			MessageInvalid = 6,

			/// <summary>
			/// Body message is null.
			/// </summary>
			MessageIsNull = 7,

			/// <summary>
			/// Secure token is not valid.
			/// </summary>
			SecureTokenInvalid = 8,

			/// <summary>
			/// Web Notification URL record could not be found.
			/// </summary>
			WebNotificationUrlRecordNotFound = 9,

			/// <summary>
			/// Web Notification URL record does not have a token specified.
			/// </summary>
			WebNotificationUrlTokenMissing = 10
		}

		/// <summary>
		/// Log authorization header is not valid.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string AuthorizationHeaderInvalid()
		{
			WriteEventAuthorizationHeaderInvalid(
				"Write Event Authorization Header invalid",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());


			return GetActivityId();
		}

		[Event((int)EventName.AuthorizationHeaderInvalid, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventAuthorizationHeaderInvalid(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.AuthorizationHeaderInvalid, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log authorization header is not specified.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string AuthorizationHeaderMissing()
		{
			WriteEventAuthorizationHeaderMissing(
				"Authorization Header Missing",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.AuthorizationHeaderMissing, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventAuthorizationHeaderMissing(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.AuthorizationHeaderMissing, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log validation of authorization header failed.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string AuthorizationValidationFailed()
		{
			WriteEventAuthorizationValidationFailed(
				"Authorization Validation failed",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.AuthorizationValidationFailed, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventAuthorizationValidationFailed(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.AuthorizationValidationFailed, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log occurence of invalid content type.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string ContentTypeInvalid()
		{
			WriteEventContentTypeInvalid(
				"Write Event Content Type invalid",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.ContentTypeInvalid, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventContentTypeInvalid(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.ContentTypeInvalid, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log occurence of failure to deserialize message.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string MessageDeserializationFailed()
		{
			WriteEventMessageDeserializationFailed(
				"Write Event Message deserialization failed",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.MessageDeserializationFailed, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventMessageDeserializationFailed(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.MessageDeserializationFailed, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log occurence of invalid body message.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string MessageInvalid()
		{
			WriteEventMessageInvalid(
				"Write Event Message is invalid",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.MessageInvalid, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventMessageInvalid(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.MessageInvalid, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log occurence of missing body message.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string MessageIsNull()
		{
			WriteEventMessageIsNull(
				"Write Event Message is null",
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.MessageIsNull, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventMessageIsNull(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.MessageIsNull, message, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log occurence of invalid secure token.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string SecureTokenInvalid(string token)
		{
			WriteEventSecureTokenInvalid(
				token,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.SecureTokenInvalid, Message = "Token : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventSecureTokenInvalid(string token, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.SecureTokenInvalid, token ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log authorization header is not specified.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string WebNotificationUrlRecordNotFound(Guid webNotificationUrlId)
		{
			WriteEventWebNotificationUrlRecordNotFound(
				webNotificationUrlId.ToString(),
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.WebNotificationUrlRecordNotFound, Message = "Web Notification Url Id : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventWebNotificationUrlRecordNotFound(string webNotificationUrlId, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.WebNotificationUrlRecordNotFound, webNotificationUrlId ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log authorization header is not specified.
		/// </summary>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string WebNotificationUrlTokenMissing(Guid webNotificationUrlId)
		{
			WriteEventWebNotificationUrlTokenMissing(
				webNotificationUrlId.ToString(),
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.WebNotificationUrlTokenMissing, Message = "Web Notification Url Id : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 2)]
		private void WriteEventWebNotificationUrlTokenMissing(string webNotificationUrlId, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.WebNotificationUrlTokenMissing, webNotificationUrlId ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}
	}
}
