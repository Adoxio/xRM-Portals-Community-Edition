/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Partner
{
	public class OpportunityHistory
	{
		public const string PipelinePhaseDeclined = "Declined";
		public const string PipelinePhaseDelivered = "Delivered";
		public const string PipelinePhaseAccepted = "Accepted";
		//public const string PipelinePhase1 = "3-Attempting to Contact";
		//public const string PipelinePhase2 = "4-Interest Confirmed";
		//public const string PipelinePhase3 = "5-Quoted";
		//public const string PipelinePhase4 = "6-Purchased";
		//public const string PipelinePhaseReturned = "7-Returned";

		public string Details { get; set; }

		public string Name { get; set; }

		public DateTime NoteCreatedOn { get; set; }

		public string PartnerAssignedTo { get; set; }

		//public string Status { get; set; }

		//public string StepName { get; set; }

		public OpportunityHistory(Entity opportunityNote)
		{
			opportunityNote.AssertEntityName("adx_opportunitynote");

			PartnerAssignedTo = opportunityNote.GetAttributeValue<string>("adx_assignedto");

			Name = opportunityNote.GetAttributeValue<string>("adx_name");

			Details = opportunityNote.GetAttributeValue<string>("adx_description");

			NoteCreatedOn = opportunityNote.GetAttributeValue<DateTime?>("adx_date").GetValueOrDefault();

			//NoteCreatedOn = opportunityNote.GetAttributeValue<DateTime?>("createdon").GetValueOrDefault();
		}
	}
}
