/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebForms;
using System.Linq;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class FormActionMetadata
	{
		public IEnumerable<Action> Actions { get; set; }
		
		public string CssClass { get; set; }

		public string ActionNavbarCssClass { get; set; }

		public string TopContainerCssClass { get; set; }

		public string BottomContainerCssClass { get; set; }
		
		public ActionButtonStyle? ActionButtonStyle { get; set; }

		public ActionButtonPlacement? ActionButtonPlacement { get; set; }

		public ActionButtonAlignment? ActionButtonAlignment { get; set; }

		public List<LanguageResources> ActionButtonDropDownLabel { get; set; }

		public DeleteModal DeleteDialog { get; set; }

		public CloseIncidentModal CloseIncidentDialog { get; set; }

		public ResolveCaseModal ResolveCaseDialog { get; set; }

		public ReopenCaseModal ReopenCaseDialog { get; set; }

		public CancelCaseModal CancelCaseDialog { get; set; }

		public QualifyLeadModal QualifyLeadDialog { get; set; }

		public ConvertQuoteModal ConvertQuoteDialog { get; set; }

		public ConvertOrderModal ConvertOrderDialog { get; set; }

		public CalculateOpportunityModal CalculateOpportunityDialog { get; set; }

		public ActivateModal ActivateDialog { get; set; }

		public DeactivateModal DeactivateDialog { get; set; }

		public ActivateQuoteModal ActivateQuoteDialog { get; set; }

		public SetOpportunityOnHoldModal SetOpportunityOnHoldDialog { get; set; }

		public WinOpportunityModal WinOpportunityDialog { get; set; }

		public LoseOpportunityModal LoseOpportunityDialog { get; set; }

		public GenerateQuoteFromOpportunityModal GenerateQuoteFromOpportunityDialog { get; set; }

		public UpdatePipelinePhaseModal UpdatePipelinePhaseDialog { get; set; }

		public ReopenOpportunityModal ReopenOpportunityDialog { get; set; }

		public WorkflowModal WorkflowDialog { get; set; }

		public CreateRelatedRecordModal CreateRelatedRecordDialog { get; set; }

		public bool? ShowSaveChangesWarningOnExit { get; set; }

		public List<LanguageResources> SaveChangesWarningMessage { get; set; }


	}

	public enum ShowActionButtonContainer
	{
		Yes = 1,
		No = 0
	}

}
