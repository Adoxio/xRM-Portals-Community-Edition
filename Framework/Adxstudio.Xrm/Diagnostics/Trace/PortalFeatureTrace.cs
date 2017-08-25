/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using System.Web;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Telemetry implementation for portal feature usage which will emit ETW events.
	/// </summary>
	[EventSource(Guid = "C1BEFC4D-46F4-4927-B20E-E8CF1D1F2DB5", Name = InternalName)]
	public class PortalFeatureTrace : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalFeatureTrace";

		/// <summary>
		/// The internal TraceSource.
		/// </summary>
		private static readonly TraceSource InternalTrace = new TraceSource(InternalName);

		/// <summary>
		/// The variable for lazy initialization of <see cref="PortalFeatureTrace"/>.
		/// </summary>
		private static readonly Lazy<PortalFeatureTrace> Instance = new Lazy<PortalFeatureTrace>();

		/// <summary>
		/// Lazy initialization of PortalFeatureTrace
		/// </summary>
		public static PortalFeatureTrace TraceInstance
		{
			get
			{
				return Instance.Value;
			}
		}

		/// <summary>
		/// Enum of feature usage level
		/// 1 - Emits portal feature usage events
		/// 2 - Emits portal authentication events
		/// 3 - Emits portal session events
        /// 4 - Emits portal search events
		/// </summary>
		private enum Feature
		{
			PortalFeatureUsage = 1,
			PortalAuthenticationEvent = 2,
			PortalSession = 3,
            PortalSearch = 4,

		}

		#region FeatureUsage
		/// <summary>
		/// capture portal feature usage events
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">User action</param>
		/// <param name="itemCount">Number of items read/acted upon</param>
		/// <param name="entity">portal area</param>
		/// <param name="cRED">Create, read, edit, delete action</param>
		[NonEvent]
		public void LogFeatureUsage(FeatureTraceCategory category, HttpContext context, string action, int itemCount, EntityReference entity, string cRED)
		{
			this.LogFeatureUsage(
				category,
				new HttpContextWrapper(context),
				action,
				itemCount,
				entity,
				cRED);
		}

		/// <summary>
		/// capture portal feature usage events
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">User action</param>
		/// <param name="itemCount">Number of items read/acted upon</param>
		/// <param name="entity">portal area</param>
		/// <param name="cRED">Create, read, edit, delete action</param>
		[NonEvent]
		public void LogFeatureUsage(FeatureTraceCategory category, HttpContextBase context, string action, int itemCount, EntityReference entity, string cRED)
		{
			var id = entity != null ? entity.Id.ToString() : string.Empty;
			var logicalName = entity != null ? entity.LogicalName : string.Empty;

			this.LogFeatureUsage(
				category,
				context,
				action,
				id,
				itemCount,
				logicalName,
				cRED);
		}

		/// <summary>
		/// capture portal feature usage events
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">User action</param>
		/// <param name="message">GUID for action item</param>
		/// <param name="itemCount">Number of items read/acted upon</param>
		/// <param name="entity">portal area</param>
		/// <param name="cRED">Create, read, edit, delete action</param>
		[NonEvent]
		public void LogFeatureUsage(FeatureTraceCategory category, HttpContext context, string action, string message, int itemCount, string entity, string cRED)
		{
			this.LogFeatureUsage(
				category,
				new HttpContextWrapper(context),
				action,
				message,
				itemCount,
				entity,
				cRED);
		}

		/// <summary>
		/// capture portal feature usage events
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">User action</param>
		/// <param name="message">GUID for action item</param>
		/// <param name="itemCount">Number of items read/acted upon</param>
		/// <param name="entity">portal area</param>
		/// <param name="cRED">Create, read, edit, delete action</param>
		[NonEvent]
		public void LogFeatureUsage(FeatureTraceCategory category, HttpContextBase context, string action, string message, int itemCount, string entity, string cRED)
		{
			try
			{
				var userId = HashPii.GetHashedUserId(context);
				var sessionId = context != null && context.Session != null ? context.Session.SessionID : string.Empty;
			    bool isPortalEntityAllowed = EntityNamePrivacy.IsPortalEntityAllowed(entity);

			    cRED = string.IsNullOrEmpty(cRED) ? "UnknownPortalAction" : cRED;
                entity = EntityNamePrivacy.GetEntityName(entity);
			    action = cRED + (!isPortalEntityAllowed ? "__CustomEntityHashedName:" : "__") + entity;

                // message = EntityGUID

				this.LogFeatureUsage(
					category,
					userId,
					sessionId,
					action,
					message,
					itemCount,
					entity,
					this.Lcid,
					this.CrmLcid,
					this.PortalUrl,
					this.PortalVersion,
					this.ProductionOrTrial,
					cRED,
					this.ElapsedTime());
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogFeatureUsage: received unexpected exception. Message: " + ex.Message);
			}
		}
		#endregion FeatureUsage

		#region Authentication
		/// <summary>
		/// Capture portal authentication events
		/// </summary>
		/// <param name="category">Area for the authentication emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">authentication event</param>
		/// <param name="entity">portal area</param>
		[NonEvent]
		public void LogAuthentication(FeatureTraceCategory category, HttpContext context, string action, string entity = "")
		{
			this.LogAuthentication(
				category,
				new HttpContextWrapper(context),
				action,
				entity);
		}

		/// <summary>
		/// Capture portal authentication events
		/// </summary>
		/// <param name="category">Area for the authentication emitted</param>
		/// <param name="context">web context</param>
		/// <param name="action">authentication event</param>
		/// <param name="entity">portal area</param>
		/// <param name="authenticationType">internal or external authentication</param>
		[NonEvent]
		public void LogAuthentication(FeatureTraceCategory category, HttpContextBase context, string action, string entity = "", string authenticationType = "")
		{
			try
			{
				var userId = HashPii.GetHashedUserId(context);
				var sessionId = context.Session.SessionID;

				this.LogAuthentication(
					category,
					userId,
					sessionId,
					action,
					entity,
					authenticationType);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogAuthentication: received unexpected exception. Message: " + ex.Message);
			}
		}

		/// <summary>
		/// Capture portal authentication events
		/// </summary>
		/// <param name="category">Area for the authentication emitted</param>
		/// <param name="userId">SHA256 hashed user ID</param>
		/// <param name="sessionId">Web Session ID</param>
		/// <param name="action">authentication event</param>
		/// <param name="entity">portal area</param>
		/// <param name="authenticationType">internal or external authentication</param>
		[NonEvent]
		public void LogAuthentication(FeatureTraceCategory category, string userId, string sessionId, string action, string entity = "", string authenticationType = "")
		{
			try
			{
				this.LogAuthentication(
					category,
					userId,
					sessionId,
					action,
					entity,
					this.PortalUrl,
					this.PortalVersion,
					this.ProductionOrTrial,
					authenticationType,
					this.ElapsedTime());
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogAuthentication: received unexpected exception. Message: " + ex.Message);
			}
		}
		#endregion Authentication

		#region Session
		/// <summary>
		/// Capture portal session events
		/// </summary>
		/// <param name="category">Area for the session emitted</param>
		[NonEvent]
        public void LogSessionInfo(FeatureTraceCategory category)
		{
			var userId = string.Empty;
			var authenticated = false;
			var userAgent = string.Empty;
			var ipAddress = string.Empty;
			var context = this.HttpContextBase;

			if (context != null)
			{
				try
				{
					authenticated = context.User.Identity.IsAuthenticated;
					userId = HashPii.GetHashedUserId(context);
					userAgent = context.Request.UserAgent;
					ipAddress = HashPii.GetHashedIpAddress(context);
				}
				catch
				{

				}
			}

			this.LogSessionInfo(category,
				userId,
				authenticated
					? "authenticated"
					: "nonauthenticated",
				this.SessionId,
				this.Lcid,
				this.CrmLcid,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				userAgent,
				ipAddress,
				this.ElapsedTime(),
				this.PersistentCookie);
		}
        #endregion Session

        #region Search

        /// <summary>
        /// Capture portal search events
        /// </summary>
        /// <param name="category">area for feature captured</param>
        /// <param name="totalHits">number of results returned</param>
        /// <param name="elapsedTime">time for search to complete</param>
        /// <param name="message">telemetry message</param>
        [NonEvent]
        public void LogSearch(FeatureTraceCategory category, int totalHits, long elapsedTime, string message)
        {

            this.LogSearch(category,
                message,
                totalHits,
                elapsedTime,
                this.UserId,
                this.SessionId,
                this.Lcid,
                this.CrmLcid,
                this.PortalUrl,
                this.PortalVersion,
                this.ProductionOrTrial);

        }

        #endregion
        /// <summary>
        /// Emit portal session event
        /// </summary>
        /// <param name="category">Area for feature emitted</param>
        /// <param name="userId">SHA256 hashed user ID</param>
        /// <param name="authenticated">String denoting whether or not a user is authenticated</param>
        /// <param name="sessionId">Web Session ID</param>
        /// <param name="lcid">LCID of the active context language.</param>
        /// <param name="crmLcid">CRM system LCID of the active context language.</param>
        /// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
        /// <param name="portalVersion">Version of the portal</param>
        /// <param name="portalProductionOrTrialType">Production or trial portal</param>
        /// <param name="userAgent">Request UserAgent</param>
        /// <param name="ipAddress">Request IP Address</param>
        /// <param name="elapsedTime">time elapsed within the current request</param>
        /// <param name="persistentCookie">value of the persistent cookie set in the browser</param>
        [Event((int)Feature.PortalSession, Level = EventLevel.Informational, Message = "Category:{0} UserID:{1} Authenticated:{2} SessionID:{3} Lcid:{4} CrmLcid:{5} PortalUrl:{6} PortalVersion:{7} PortalProductionOrTrial:{8} UserAgent:{9} IpAddress:{10} ElapsedTime:{11} persistentCookie:{12}", Version = 3)]
		private void LogSessionInfo(FeatureTraceCategory category, string userId, string authenticated, string sessionId, int lcid, int crmLcid, string portalUrl, string portalVersion, string portalProductionOrTrialType, string userAgent, string ipAddress, string elapsedTime, string persistentCookie)
		{
			try
			{
				InternalTrace.TraceEvent(
					PortalSettings.Instance, 
					TraceEventType.Information,
					(int)Feature.PortalSession,
					"Category:{0} UserID:{1} Authenticated:{2} SessionID:{3} Lcid:{4} CrmLcid:{5} PortalUrl:{6} PortalVersion:{7} PortalProductionOrTrial:{8} PortalID={9}, PortalEvent={10}, TenantID={11}, OrgID={12}, Geo={13}, PortalApp={14}, PortalType={15}, UserAgent={16}, IpAddress={17}, persistentCookie={18}",
					category,
					userId,
					authenticated,
					sessionId,
					lcid,
					crmLcid,
					portalUrl,
					portalVersion,
					portalProductionOrTrialType,
					PortalDetail.Instance.PortalId,
					PortalEvents.FeatureUsage,
					PortalDetail.Instance.TenantId,
					PortalDetail.Instance.OrgId,
					PortalDetail.Instance.Geo,
					PortalDetail.Instance.PortalApp,
					PortalDetail.Instance.PortalType,
					userAgent,
					ipAddress,
					persistentCookie);

				this.WriteEvent((int)Feature.PortalSession, category, userId, authenticated, sessionId, lcid, crmLcid, portalUrl, portalVersion, portalProductionOrTrialType, userAgent, ipAddress, elapsedTime, persistentCookie);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogSessionInfo: received unexpected exception. Message: " + ex.Message);
			}
		}

		/// <summary>
		/// Emit portal feature usage event
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="userId">SHA256 hashed user ID</param>
		/// <param name="sessionId">Web Session ID</param>
		/// <param name="action">User action</param>
		/// <param name="message">GUID for action item</param>
		/// <param name="itemCount">Number of items read/acted upon</param>
		/// <param name="entity">portal area</param>
		/// <param name="lcid">LCID of the active context language.</param>
		/// <param name="crmLcid">CRM system LCID of the active context language.</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="cRED">Create, read, edit, delete action</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)Feature.PortalFeatureUsage, Level = EventLevel.Informational, Message = "Category:{0} UserID:{1} SessionID:{2} Action:{3} Message:{4} ItemCount:{5} Entity:{6} Lcid:{7} CrmLcid:{8} PortalUrl:{9} PortalVersion:{10} PortalProductionOrTrial:{11} CRED:{12} ElapsedTime:{13}", Version = 4)]
		private void LogFeatureUsage(FeatureTraceCategory category, string userId, string sessionId, string action, string message, int itemCount, string entity, int lcid, int crmLcid, string portalUrl, string portalVersion, string portalProductionOrTrialType, string cRED, string elapsedTime)
		{
			try
			{
				InternalTrace.TraceEvent(
					PortalSettings.Instance,
					TraceEventType.Information,
					(int)Feature.PortalFeatureUsage,
					"Category:{0} UserID:{1} SessionID:{2} Action:{3} Message:{4} ItemCount:{5} Entity:{6} Lcid:{7} CrmLcid:{8} PortalURL:{9} PortalVersion:{10} PortalProductionOrTrial:{11} PortalID={12}, PortalEvent={13}, TenantID={14}, OrgID={15}, Geo={16}, PortalApp={17}, PortalType={18}",
					category,
					userId,
					sessionId,
					action,
					message,
					itemCount,
					entity,
					lcid,
					crmLcid,
					portalUrl,
					portalVersion,
					portalProductionOrTrialType,
					PortalDetail.Instance.PortalId,
					PortalEvents.FeatureUsage,
					PortalDetail.Instance.TenantId,
					PortalDetail.Instance.OrgId,
					PortalDetail.Instance.Geo,
					PortalDetail.Instance.PortalApp,
					PortalDetail.Instance.PortalType);

				this.WriteEvent((int)Feature.PortalFeatureUsage, category, userId, sessionId, action, message, itemCount, entity, lcid, crmLcid, portalUrl, portalVersion, portalProductionOrTrialType, cRED, elapsedTime);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogFeatureUsage: received unexpected exception. Message: " + ex.Message);
			}
		}

		/// <summary>
		/// Emit portal authentication events
		/// </summary>
		/// <param name="category">Area for feature emitted</param>
		/// <param name="userId">SHA256 hashed user ID</param>
		/// <param name="sessionId">Web Session ID</param>
		/// <param name="action">User action</param>
		/// <param name="entity">portal area</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="authenticationType">internal or external authentication</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)Feature.PortalAuthenticationEvent, Level = EventLevel.Informational, Message = "Category:{0} UserID:{1} SessionID:{2} Action:{3} Entity:{4} PortalUrl:{5} PortalVersion:{6} PortalProductionOrTrial:{7} AuthenticationType:{8} ElapsedTime:{9}", Version = 4)]
		private void LogAuthentication(FeatureTraceCategory category, string userId, string sessionId, string action, string entity, string portalUrl, string portalVersion, string portalProductionOrTrialType, string authenticationType, string elapsedTime)
		{
			try
			{
				InternalTrace.TraceEvent(
					PortalSettings.Instance,
					TraceEventType.Information,
					(int)Feature.PortalAuthenticationEvent,
					"Category:{0} UserID:{1} SessionID:{2} Action:{3} Entity:{4} PortalUrl:{5} PortalVersion:{6} PortalProductionOrTrialType:{7} PortalID={8}, PortalEvent={9}, TenantID={10}, OrgID={11}, Geo={12}, PortalApp={13}, PortalType={14}",
					category,
					userId,
					sessionId,
					action,
					entity,
					portalUrl,
					portalVersion,
					portalProductionOrTrialType,
					PortalDetail.Instance.PortalId,
					PortalEvents.Authentication,
					PortalDetail.Instance.TenantId,
					PortalDetail.Instance.OrgId,
					PortalDetail.Instance.Geo,
					PortalDetail.Instance.PortalApp,
					PortalDetail.Instance.PortalType);

				this.WriteEvent((int)Feature.PortalAuthenticationEvent, category, userId, sessionId, action, entity, portalUrl, portalVersion, portalProductionOrTrialType, authenticationType, elapsedTime);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogAuthentication: received unexpected exception. Message: " + ex.Message);
			}
		}

        /// <summary>
        /// Writes search event to JARVIS
        /// </summary>
        /// <param name="category">area for feature emitted</param>
        /// <param name="message">search telemetry message</param>
        /// <param name="totalHits">number of results returned</param>
        /// <param name="elapsedTime">time to complete search</param>
        /// <param name="userId">SHA256 HASH user ID</param>
        /// <param name="sessionId">user session ID</param>
        /// <param name="lcid">LCID of the active context language.</param>
        /// <param name="crmLcid">CRM system LCID of the active context language.</param>
        /// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
        /// <param name="portalVersion">portal version</param>
        /// <param name="portalProductionOrTrialType">production or trial portal</param>
        [Event((int)Feature.PortalSearch, Level = EventLevel.Informational, Message = "Category:{0} Message:{1} TotalHits:{2} ElapsedTime:{3} UserID:{4} SessionID:{5} LCID:{6} CRMLCID:{7} PortalURL:{8} PortalVersion:{9} ProductionOrTrial:{10}", Version = 1)]
        private void LogSearch(FeatureTraceCategory category, string message, int totalHits, long elapsedTime, string userId, string sessionId, int lcid, int crmLcid, string portalUrl, string portalVersion, string portalProductionOrTrialType)
        {
            try
            {
                InternalTrace.TraceEvent(
                    TraceEventType.Information,
                    (int)Feature.PortalSearch,
                    "Category:{0} Message:{1} TotalHits:{2} ElapsedTime:{3} UserID:{4} SessionID:{5} LCID:{6} CRMLCID:{7} PortalVersion:{8} PortalProductionOrTrialType:{9} PortalID={10}, PortalURL={11}, TenantID={12}, OrgID={13}, Geo={14}, PortalApp={15}, PortalType={16}",
                    category,
                    message,
                    totalHits,
                    elapsedTime,
                    userId,
                    sessionId,
                    lcid,
                    crmLcid,
                    portalVersion,
                    portalProductionOrTrialType,
                    PortalDetail.Instance.PortalId,
                    PortalDetail.Instance.AzurePortalUrl,
                    PortalDetail.Instance.TenantId,
                    PortalDetail.Instance.OrgId,
                    PortalDetail.Instance.Geo,
                    PortalDetail.Instance.PortalApp,
                    PortalDetail.Instance.PortalType);

                this.WriteEvent((int)Feature.PortalSearch, category, message, totalHits, elapsedTime, userId, sessionId, lcid, crmLcid, portalUrl, portalVersion, portalProductionOrTrialType);
            }
            catch (Exception ex)
            {
                ADXTrace.Instance.TraceError(TraceCategory.Exception, "LogSearch: received an unexpected exception. Message: " + ex.Message);
            }
        }
	}
}
