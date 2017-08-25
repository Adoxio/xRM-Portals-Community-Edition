/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Web Form Session State Provider Interface.
	/// </summary>
	public interface ISessionHistoryProvider
	{
		/// <summary>
		/// Initialize the Session History.
		/// </summary>
		/// <param name="webFormId">ID of the web form.</param>
		/// <param name="currentStepId">ID of the current step.</param>
		/// <param name="currentStepIndex">Index of the current step.</param>
		/// <param name="currentRecordId">ID of the current record.</param>
		/// <param name="currentRecordEntityLogicalName">Logical name of the current record entity.</param>
		/// <param name="currentRecordEntityPrimaryKeyLogicalName">Logical name of the primary key of the current record entity.</param>
		/// <param name="contactId">ID of the authenticated user's contact record.</param>
		/// <param name="systemUserId">ID of the authenticated user's system user record.</param>
		/// <param name="anonymousIdentification">Identifier of the anonymous user. Requires a web.config section anonymousIdentification <see href="http://msdn.microsoft.com/en-us/library/91ka2e6a.aspx"/></param>
		/// <param name="userHostAddress">IP Address of the user's computer.</param>
		/// <param name="userIdentityName">User's Identity Name.</param>
		/// <returns>Session History</returns>
		SessionHistory InitializeSessionHistory(Guid webFormId, Guid currentStepId, int currentStepIndex, Guid currentRecordId, string currentRecordEntityLogicalName, string currentRecordEntityPrimaryKeyLogicalName, Guid? contactId, Guid? systemUserId, string anonymousIdentification, string userHostAddress, string userIdentityName);

		/// <summary>
		/// Get Session History for the specified record ID.
		/// </summary>
		/// <param name="context">Context used to retrieve session history.</param>
		/// <param name="currentRecordId">ID of the current record.</param>
		/// <returns>Session History</returns>
		SessionHistory GetSessionHistory(OrganizationServiceContext context, Guid currentRecordId);

		/// <summary>
		/// Persists the Session History.
		/// </summary>
		/// <param name="context">Context used to save the session history.</param>
		/// <param name="sessionHistory">Session History object.</param>
		/// <returns>ID of the session history record.</returns>
		Guid PersistSessionHistory(OrganizationServiceContext context, SessionHistory sessionHistory);
	}
}
