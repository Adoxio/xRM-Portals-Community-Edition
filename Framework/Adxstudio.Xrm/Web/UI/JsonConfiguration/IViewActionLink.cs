/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Json.JsonConverter;
using Microsoft.Xrm.Portal.Web;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public interface IViewActionLink
	{
		/// <summary>
		/// URL target of the action link
		/// </summary>
		[JsonConverter(typeof(UrlBuilderConverter))]
		UrlBuilder URL { get; set; }

		/// <summary>
		/// Type of action
		/// </summary>
		LinkActionType Type { get; set; }

		/// <summary>
		/// Display label
		/// </summary>
		string Label { get; set; }

		/// <summary>
		/// Tooltip display text
		/// </summary>
		string Tooltip { get; set; }

		/// <summary>
		/// The name of the Query String parameter containing the record id. Not applicable to Type 'Insert'.
		/// </summary>
		string QueryStringIdParameterName { get; set; }

		/// <summary>
		/// True indicates the action is enabled otherwise disabled.
		/// </summary>
		bool Enabled { get; set; }

		int? ActionIndex { get; set; }

		ActionButtonAlignment? ActionButtonAlignment { get; set; }

		ActionButtonStyle? ActionButtonStyle { get; set; }

		ActionButtonPlacement? ActionButtonPlacement { get; set; }
	}

	/// <summary>
	/// Type of action
	/// </summary>
	public enum LinkActionType
	{
		/// <summary>
		/// Display details of a record
		/// </summary>
		Details = 1,
		/// <summary>
		/// Edit a record
		/// </summary>
		Edit = 2,
		/// <summary>
		/// Create a record
		/// </summary>
		Insert = 3,
		/// <summary>
		/// Delete a record
		/// </summary>
		Delete = 4,
		/// <summary>
		/// Associate a record to a parent
		/// </summary>
		Associate = 5,
		/// <summary>
		/// Disassociate a record from a parent
		/// </summary>
		Disassociate = 6,
		/// <summary>
		/// Execute a workflow on a record
		/// </summary>
		Workflow = 7,
		/// <summary>
		/// Download data
		/// </summary>
		Download = 8,
		/// <summary>
		/// Close Incident
		/// </summary>
		CloseIncident = 9,
		/// <summary>
		/// Qualify Lead
		/// </summary>
        QualifyLead = 10,
		/// <summary>
		/// Convert Quote
		/// </summary>
		ConvertQuote = 11,
		/// <summary>
		/// Convert Order
		/// </summary>
		ConvertOrder = 12,
		/// <summary>
		/// Calculate Opportunity
		/// </summary>
		CalculateOpportunity = 13,
		/// <summary>
		/// Resolve Case
		/// </summary>
		ResolveCase = 14,
		/// <summary>
		/// Reopen Case
		/// </summary>
		ReopenCase = 15,
		/// <summary>
		/// Cancel Case
		/// </summary>
		CancelCase = 16,
		/// <summary>
		/// Deactivate
		/// </summary>
		Deactivate = 17,
		/// <summary>
		/// Activate
		/// </summary>
		Activate = 18,
		/// <summary>
		/// Activate Quote
		/// </summary>
		ActivateQuote = 19,
		/// <summary>
		/// Set Opportunity On Hold
		/// </summary>
		SetOpportunityOnHold = 20,
		/// <summary>
		/// Win Opportunity
		/// </summary>
		WinOpportunity = 21,
		/// <summary>
		/// Lose Opportunity
		/// </summary>
		LoseOpportunity = 22,
		/// <summary>
		/// Generate Quote From Opportunity
		/// </summary>
		GenerateQuoteFromOpportunity = 23,
		/// <summary>
		/// Update Pipeline Phase
		/// </summary>
		UpdatePipelinePhase = 24,
		/// <summary>
		/// Reopen Opportunity
		/// </summary>
		ReopenOpportunity = 25,
		/// <summary>
		/// Submit
		/// </summary>
		Submit = 26,
		/// <summary>
		/// Next
		/// </summary>
		Next = 27,
		/// <summary>
		/// Previous 
		/// </summary>
		Previous = 28,
		/// <summary>
		/// Create related record action
		/// </summary>
		CreateRelatedRecord = 29
	}
}
