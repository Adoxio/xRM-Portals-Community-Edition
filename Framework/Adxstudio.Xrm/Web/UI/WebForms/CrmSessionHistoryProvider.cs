/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Session History Provider stores data to CRM
	/// </summary>
	public class CrmSessionHistoryProvider : ISessionHistoryProvider
	{
		/// <summary>
		/// Initialize the Session History
		/// </summary>
		/// <param name="webFormId">ID of the web form</param>
		/// <param name="currentStepId">ID of the current step</param>
		/// <param name="currentStepIndex">Index of the current step</param>
		/// <param name="recordId">ID of the target record</param>
		/// <param name="recordEntityLogicalName">Logical name of the target record entity</param>
		/// <param name="recordEntityPrimaryKeyLogicalName">Logical name of the primary key of the target record entity</param>
		/// <param name="contactId">ID of the authenticated user's contact record</param>
		/// <param name="systemUserId">ID of the authenticated user's system user record</param>
		/// <param name="anonymousIdentification">Identifier of the anonymous user. Requires a web.config section anonymousIdentification <see href="http://msdn.microsoft.com/en-us/library/91ka2e6a.aspx"/></param>
		/// <param name="userIdentityName">User's Identity Name</param>
		/// <param name="userHostAddress">IP Address of the user's computer</param>
		/// <returns>Session History</returns>
		public SessionHistory InitializeSessionHistory(Guid webFormId, Guid currentStepId, int currentStepIndex, Guid recordId, string recordEntityLogicalName, string recordEntityPrimaryKeyLogicalName, Guid? contactId, Guid? systemUserId, string anonymousIdentification, string userIdentityName, string userHostAddress)
		{
			var stepHistory = new List<SessionHistory.Step>();
			var referenceEntity = new SessionHistory.ReferenceEntity
									{
										ID = recordId,
										LogicalName = recordEntityLogicalName,
										PrimaryKeyLogicalName = recordEntityPrimaryKeyLogicalName
									};
			var step = new SessionHistory.Step { ID = currentStepId, Index = currentStepIndex, ReferenceEntity = referenceEntity };
			stepHistory.Add(step);
			return (new SessionHistory
			{
				Id = Guid.Empty,
				WebFormId = webFormId,
				CurrentStepId = currentStepId,
				CurrentStepIndex = currentStepIndex,
				PrimaryRecord = referenceEntity,
				ContactId = contactId ?? Guid.Empty,
				SystemUserId = systemUserId ?? Guid.Empty,
				AnonymousIdentification = anonymousIdentification,
				UserIdentityName = userIdentityName,
				UserHostAddress = userHostAddress,
				StepHistory = stepHistory
			});
		}

		/// <summary>
		/// Initialize the Session History
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">ID of the web form</param>
		/// <param name="stepId">ID of the current step</param>
		/// <param name="stepIndex">Index of the current step</param>
		/// <param name="recordId">ID of the target record</param>
		/// <param name="recordEntityLogicalName">Logical name of the target record entity</param>
		/// <param name="recordEntityPrimaryKeyLogicalName">Logical name of the primary key of the target record entity</param>
		/// <returns>Session History</returns>
		public SessionHistory InitializeSessionHistory(OrganizationServiceContext context, Guid webFormId, Guid stepId, int stepIndex, Guid recordId, string recordEntityLogicalName, string recordEntityPrimaryKeyLogicalName)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (webFormId == Guid.Empty)
			{
				throw new ArgumentNullException("webFormId");
			}

			if (stepId == Guid.Empty)
			{
				throw new ArgumentNullException("stepId");
			}

			if (string.IsNullOrWhiteSpace(recordEntityLogicalName))
			{
				throw new ArgumentNullException("recordEntityLogicalName");
			}

			if (string.IsNullOrWhiteSpace(recordEntityPrimaryKeyLogicalName))
			{
				throw new ArgumentNullException("recordEntityPrimaryKeyLogicalName");
			}

			var contactId = Guid.Empty;
			var systemUserId = Guid.Empty;
			var anonymousIdentification = string.Empty;
			var userIdentityName = string.Empty;
			var userHostName = string.Empty;

			if (HttpContext.Current.Request.IsAuthenticated)
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				if (portalContext.User == null)
				{
                    throw new ApplicationException("Couldn't load user record. Portal context User is null.");
				}
				switch (portalContext.User.LogicalName)
				{
					case "contact":
						contactId = portalContext.User.Id;
						break;
					case "systemuser":
						systemUserId = portalContext.User.Id;
						break;
					default:
						if (HttpContext.Current.User == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
						{
							throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portalContext.User.LogicalName));
						}
						break;
				}
			}
			else
			{
				if (HttpContext.Current.Profile != null && !string.IsNullOrWhiteSpace(HttpContext.Current.Profile.UserName))
				{
					anonymousIdentification = HttpContext.Current.Profile.UserName;
				}
			}

			if (HttpContext.Current.User != null && !string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
			{
				userIdentityName = HttpContext.Current.User.Identity.Name;
			}

			if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.UserHostName))
			{
				userHostName = HttpContext.Current.Request.UserHostName;
			}

			return InitializeSessionHistory(webFormId, stepId, stepIndex, recordId, recordEntityLogicalName, recordEntityPrimaryKeyLogicalName, contactId, systemUserId, anonymousIdentification, userIdentityName, userHostName);
		}
		
		/// <summary>
		/// Get Session History for the specified web form and contact
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">Unique Identifier of the Web Form</param>
		/// <param name="contactId">Unique Identifier of the contact</param>
		/// <returns>Session History</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public SessionHistory GetSessionHistoryByContact(OrganizationServiceContext context, Guid webFormId, Guid contactId)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0).OrderByDescending(s => s.GetAttributeValue<DateTime>("modifiedon")).FirstOrDefault(s => s.GetAttributeValue<EntityReference>("adx_webform") == new EntityReference("adx_webform", webFormId) && s.GetAttributeValue<EntityReference>("adx_contact") == new EntityReference("contact", contactId));

			return GetSessionHistory(webFormSession);
		}

		/// <summary>
		/// Get Session History for the specified web form and system user
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">Unique Identifier of the Web Form</param>
		/// <param name="systemUserId">Unique Identifier of the system user</param>
		/// <returns>Session History</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public SessionHistory GetSessionHistoryBySystemUser(OrganizationServiceContext context, Guid webFormId, Guid systemUserId)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0).OrderByDescending(s => s.GetAttributeValue<DateTime>("modifiedon")).FirstOrDefault(s => s.GetAttributeValue<EntityReference>("adx_webform") == new EntityReference("adx_webform", webFormId) && s.GetAttributeValue<EntityReference>("adx_systemuser") == new EntityReference("systemuser", systemUserId));

			return GetSessionHistory(webFormSession);
		}

		/// <summary>
		/// Get Session History for the specified web form and user identity name
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">Unique Identifier of the Web Form</param>
		/// <param name="userIdentityName">User Identity Name</param>
		/// <returns>Session History</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public SessionHistory GetSessionHistoryByUserIdentityName(OrganizationServiceContext context, Guid webFormId, string userIdentityName)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0).OrderByDescending(s => s.GetAttributeValue<DateTime>("modifiedon")).FirstOrDefault(s => s.GetAttributeValue<EntityReference>("adx_webform") == new EntityReference("adx_webform", webFormId) && s.GetAttributeValue<string>("adx_useridentityname") == userIdentityName);

			return GetSessionHistory(webFormSession);
		}

		/// <summary>
		/// Get Session History for the specified web form and anonymous user.
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">Unique Identifier of the Web Form</param>
		/// <param name="anonymousIdentification">Identification of the anonymous user</param>
		/// <returns>Session History</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public SessionHistory GetSessionHistoryByAnonymousIdentification(OrganizationServiceContext context, Guid webFormId, string anonymousIdentification)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0).OrderByDescending(s => s.GetAttributeValue<DateTime>("modifiedon")).FirstOrDefault(s => s.GetAttributeValue<EntityReference>("adx_webform") == new EntityReference("adx_webform", webFormId) && s.GetAttributeValue<string>("adx_anonymousidentification") == anonymousIdentification);

			return GetSessionHistory(webFormSession);
		}

		/// <summary>
		/// Get Session History for the specified record ID
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="webFormId">Unique Identifier of the Web Form</param>
		/// <param name="recordId">ID of the target record</param>
		/// <returns></returns>
		public SessionHistory GetSessionHistoryByPrimaryRecord(OrganizationServiceContext context, Guid webFormId, Guid recordId)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").FirstOrDefault(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && s.GetAttributeValue<EntityReference>("adx_webform") == new EntityReference("adx_webform", webFormId) && s.GetAttributeValue<string>("adx_primaryrecordid") == recordId.ToString());

			return GetSessionHistory(webFormSession);
		}

		/// <summary>
		/// Get Session History for the specified session history ID
		/// </summary>
		/// <param name="context">Context used to retrieve session history</param>
		/// <param name="sessionID">ID of the session history record</param>
		/// <returns></returns>
		public SessionHistory GetSessionHistory(OrganizationServiceContext context, Guid sessionID)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").FirstOrDefault(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && s.GetAttributeValue<Guid>("adx_webformsessionid") == sessionID);

			return GetSessionHistory(webFormSession);
		}

		private static SessionHistory GetSessionHistory(Entity webFormSession)
		{
			if (webFormSession == null)
			{
				return null;
			}

			var sessionHistory = new SessionHistory
									{
										Id = webFormSession.Id,
										WebFormId = webFormSession.GetAttributeValue<EntityReference>("adx_webform") == null ? Guid.Empty : webFormSession.GetAttributeValue<EntityReference>("adx_webform").Id
									};

			var currentStep = webFormSession.GetAttributeValue<EntityReference>("adx_currentwebformstep");

			if (currentStep == null)
			{
				throw new ApplicationException("adx_webformsession.adx_currentwebformstep is null.");
			}

			sessionHistory.CurrentStepId = currentStep.Id;

			var currentStepIndex = webFormSession.GetAttributeValue<int?>("adx_currentstepindex");

			var stepIndex = currentStepIndex ?? 0;

			if (currentStepIndex == null)
			{
				throw new ApplicationException("adx_webformsession.adx_currentwebformstep is null.");
			}

			sessionHistory.CurrentStepIndex = stepIndex;

			Guid recordGuid;
			var recordid = webFormSession.GetAttributeValue<string>("adx_primaryrecordid");

			sessionHistory.PrimaryRecord = new SessionHistory.ReferenceEntity();

			if (!string.IsNullOrWhiteSpace(recordid) && Guid.TryParse(recordid, out recordGuid))
			{
				sessionHistory.PrimaryRecord.ID = recordGuid;
			}

			sessionHistory.PrimaryRecord.LogicalName = webFormSession.GetAttributeValue<string>("adx_primaryrecordentitylogicalname") ?? string.Empty;

			sessionHistory.PrimaryRecord.PrimaryKeyLogicalName = webFormSession.GetAttributeValue<string>("adx_primaryrecordentitykeyname") ?? string.Empty;
			
			var contact = webFormSession.GetAttributeValue<EntityReference>("adx_contact");

			sessionHistory.ContactId = contact != null ? contact.Id : Guid.Empty;

			var quote = webFormSession.GetAttributeValue<EntityReference>("adx_quoteid");

			sessionHistory.QuoteId = quote != null ? quote.Id : Guid.Empty;

			var systemUser = webFormSession.GetAttributeValue<EntityReference>("adx_systemuser");

			sessionHistory.SystemUserId = systemUser != null ? systemUser.Id : Guid.Empty;

			sessionHistory.AnonymousIdentification = webFormSession.GetAttributeValue<string>("adx_anonymousidentification") ?? string.Empty;

			sessionHistory.StepHistory = ConvertJsonStringToList(webFormSession.GetAttributeValue<string>("adx_stephistory"));

			sessionHistory.UserHostAddress = webFormSession.GetAttributeValue<string>("adx_userhostaddress") ?? string.Empty;

			sessionHistory.UserIdentityName = webFormSession.GetAttributeValue<string>("adx_useridentityname") ?? string.Empty;

			return sessionHistory;
		}

		/// <summary>
		/// Persists the Session History
		/// </summary>
		/// <param name="context">Context used to save the session history</param>
		/// <param name="sessionHistory">Session History object</param>
		public Guid PersistSessionHistory(OrganizationServiceContext context, SessionHistory sessionHistory)
		{
			if (sessionHistory == null)
			{
				return Guid.Empty;
			}

			var webFormSession = sessionHistory.Id != Guid.Empty ? context.CreateQuery("adx_webformsession").FirstOrDefault(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && s.GetAttributeValue<Guid>("adx_webformsessionid") == sessionHistory.Id) : null;

			var addNew = webFormSession == null;

			if (addNew)
			{
				webFormSession = new Entity("adx_webformsession");

				if (sessionHistory.WebFormId != Guid.Empty)
				{
					webFormSession.Attributes["adx_webform"] = new EntityReference("adx_webform", sessionHistory.WebFormId);
				}
			}

			if (sessionHistory.PrimaryRecord != null && sessionHistory.PrimaryRecord.ID != Guid.Empty)
			{
				webFormSession.Attributes["adx_primaryrecordid"] = sessionHistory.PrimaryRecord.ID.ToString();
			}

			if (sessionHistory.CurrentStepId != Guid.Empty)
			{
				webFormSession.Attributes["adx_currentwebformstep"] = new EntityReference("adx_webformstep", sessionHistory.CurrentStepId);
			}

			if (sessionHistory.PrimaryRecord != null && !string.IsNullOrWhiteSpace(sessionHistory.PrimaryRecord.LogicalName))
			{
				webFormSession.Attributes["adx_primaryrecordentitylogicalname"] = sessionHistory.PrimaryRecord.LogicalName;
			}

			if (sessionHistory.PrimaryRecord != null && !string.IsNullOrWhiteSpace(sessionHistory.PrimaryRecord.PrimaryKeyLogicalName))
			{
				webFormSession.Attributes["adx_primaryrecordentitykeyname"] = sessionHistory.PrimaryRecord.PrimaryKeyLogicalName;
			}

			webFormSession.Attributes["adx_currentstepindex"] = sessionHistory.CurrentStepIndex;

			if (sessionHistory.ContactId != Guid.Empty)
			{
				webFormSession.Attributes["adx_contact"] = new EntityReference("contact", sessionHistory.ContactId);
			}

			if (sessionHistory.QuoteId != Guid.Empty)
			{
				webFormSession.Attributes["adx_quoteid"] = new EntityReference("quote", sessionHistory.QuoteId);
			}

			if (sessionHistory.SystemUserId != Guid.Empty)
			{
				webFormSession.Attributes["adx_systemuser"] = new EntityReference("systemuser", sessionHistory.SystemUserId);
			}

			if (!string.IsNullOrWhiteSpace(sessionHistory.AnonymousIdentification))
			{
				webFormSession.Attributes["adx_anonymousidentification"] = sessionHistory.AnonymousIdentification;
			}

			webFormSession.Attributes["adx_stephistory"] = ConvertListToJsonString(sessionHistory.StepHistory);

			if (!string.IsNullOrWhiteSpace(sessionHistory.UserHostAddress))
			{
				webFormSession.Attributes["adx_userhostaddress"] = sessionHistory.UserHostAddress;
			}

			if (!string.IsNullOrWhiteSpace(sessionHistory.UserIdentityName))
			{
				webFormSession.Attributes["adx_useridentityname"] = sessionHistory.UserIdentityName;
			}
			
			if (addNew)
			{
				context.AddObject(webFormSession);
			}
			else
			{
				context.UpdateObject(webFormSession);
			}

			context.SaveChanges();

			return webFormSession.Id;
		}

		/// <summary>
		/// Deactivates the Session History.
		/// </summary>
		/// <param name="context">Context used to deactivate the session history.</param>
		/// <param name="id">ID of the Session History object.</param>
		public void DeactivateSessionHistory(OrganizationServiceContext context, Guid id)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (id == Guid.Empty)
			{
				throw new ArgumentNullException("id");
			}

			var webFormSession = context.CreateQuery("adx_webformsession").FirstOrDefault(s => s.GetAttributeValue<Guid>("adx_webformsessionid") == id);

			if (webFormSession == null)
			{
				return;
			}

			context.SetState(1, 2, webFormSession.ToEntityReference());
		}

		private static string ConvertListToJsonString(List<SessionHistory.Step> history)
		{
			var stream = new MemoryStream();
			var serialiser = new DataContractJsonSerializer(typeof(List<SessionHistory.Step>));

			serialiser.WriteObject(stream, history);

			var json = Encoding.Default.GetString(stream.ToArray());

			stream.Close();

			return json;
		}

		private static List<SessionHistory.Step> ConvertJsonStringToList(string json)
		{
			List<SessionHistory.Step> result = null;

			if (!string.IsNullOrWhiteSpace(json))
			{
				var byteArray = Encoding.Unicode.GetBytes(json);
				var stream = new MemoryStream(byteArray);
				var serialiser = new DataContractJsonSerializer(typeof(List<SessionHistory.Step>));

				result = serialiser.ReadObject(stream) as List<SessionHistory.Step>;

				stream.Close();
			}

			return result;
		}
	}
}
