/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class GridMetadata
	{
		public IEnumerable<Action> ViewActions { get; set; }

		public IEnumerable<Action> ItemActions { get; set; }

		public IEnumerable<Action> ExtendedItemActions { get; set; }

		public string CssClass { get; set; }

		public string GridCssClass { get; set; }

		public EntityGridExtensions.GridColumnWidthStyle? GridColumnWidthStyle { get; set; }

		public List<LanguageResources> LoadingMessage { get; set; }

		public List<LanguageResources> ErrorMessage { get; set; }

		public List<LanguageResources> AccessDeniedMessage { get; set; }

		public List<LanguageResources> EmptyMessage { get; set; }

		public DetailsFormModal DetailsFormDialog { get; set; }

		public EditFormModal EditFormDialog { get; set; }

		public CreateFormModal CreateFormDialog { get; set; }

		public DeleteModal DeleteDialog { get; set; }

		public ErrorModal ErrorDialog { get; set; }

		public LookupModal LookupDialog { get; set; }

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

		public DisassociateModal DisassociateDialog { get; set; }

		public List<ViewColumn> ColumnOverrides { get; set; }

		public CreateRelatedRecordModal CreateRelatedRecordDialog { get; set; }
	}
}
