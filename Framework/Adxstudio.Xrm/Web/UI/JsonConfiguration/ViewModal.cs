/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Mvc.Html;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class ViewModal
	{
		public BootstrapExtensions.BootstrapModalSize? Size { get; set; }

		public string CssClass { get; set; }

		public string Title { get; set; }

		public string TitleCssClass { get; set; }

		public string PrimaryButtonText { get; set; }

		public string DismissButtonSrText { get; set; }

		public string CloseButtonText { get; set; }

		public string PrimaryButtonCssClass { get; set; }

		public string CloseButtonCssClass { get; set; }
	}

	public class ViewFormModal : ViewModal
	{
		public string LoadingMessage { get; set; }
	}

	public class ViewCreateFormModal : ViewFormModal { }

	public class ViewEditFormModal : ViewFormModal { }

	public class ViewDetailsFormModal : ViewFormModal { }

	public class ViewDeleteModal : ViewModal
	{
		public string Body { get; set; }
	}

	public class ViewErrorModal : ViewModal
	{
		public string Body { get; set; }
	}

	public class ViewAssociateModal : ViewModal
	{
		public string DefaultErrorMessage { get; set; }

		public string SelectRecordsTitle { get; set; }
	}

	public class ViewCloseIncidentModal : ViewModal { }

	public class ViewResolveCaseModal : ViewModal { }

	public class ViewReopenCaseModal : ViewModal { }

	public class ViewCancelCaseModal : ViewModal { }

    public class ViewQualifyLeadModal : ViewModal { }

	public class ViewConvertQuoteModal : ViewModal { }

	public class ViewConvertOrderModal : ViewModal { }

	public class ViewCalculateOpportunityModal : ViewModal { }

	public class ViewDeactivateModal : ViewModal { }

	public class ViewActivateModal : ViewModal { }

	public class ViewActivateQuoteModal : ViewModal { }

	public class ViewSetOpportunityOnHoldModal : ViewModal { }

	public class ViewWinOpportunityModal : ViewModal { }

	public class ViewLoseOpportunityModal : ViewModal { }

	public class ViewGenerateQuoteFromOpportunityModal : ViewModal { }

	public class ViewUpdatePipelinePhaseModal : ViewModal { }

	public class ViewWorkflowModal : ViewModal { }

	public class ViewDisassociateModal : ViewModal { }

	public class ViewReopenOpportunityModal : ViewModal { }

	public class ViewCreateRelatedRecordModal : ViewFormModal { }
}
