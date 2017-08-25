/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class Modal
	{
		public BootstrapExtensions.BootstrapModalSize? Size { get; set; }

		public string CssClass { get; set; }

		public List<LanguageResources> Title { get; set; }

		public string TitleCssClass { get; set; }

		public List<LanguageResources> PrimaryButtonText { get; set; }

		public List<LanguageResources> DismissButtonSrText { get; set; }

		public List<LanguageResources> CloseButtonText { get; set; }

		public string PrimaryButtonCssClass { get; set; }

		public string CloseButtonCssClass { get; set; }
	}

	public class NoteModal : Modal
	{
		public List<LanguageResources> NoteFieldLabel { get; set; }

		public bool? DisplayPrivacyOptionField { get; set; }

		public List<LanguageResources> PrivacyOptionFieldLabel { get; set; }

		public bool? PrivacyOptionFieldDefaultValue { get; set; }

		public bool? DisplayAttachFile { get; set; }

		public List<LanguageResources> AttachFileLabel { get; set; }

		public string AttachFileAccept { get; set; }

		public string LeftColumnCSSClass { get; set; }

		public string RightColumnCSSClass { get; set; }

		public int? NoteFieldColumns { get; set; }

		public int? NoteFieldRows { get; set; }
	}

	public class TimelineModal : NoteModal
	{
	}

	public class SharePointAddFilesModal : Modal
	{
		public List<LanguageResources> AttachFileLabel { get; set; }

		public string AttachFileAccept { get; set; }

		public bool? DisplayOverwriteField { get; set; }

		public List<LanguageResources> OverwriteFieldLabel { get; set; }

		public bool? OverwriteFieldDefaultValue { get; set; }

		public List<LanguageResources> DestinationFolderLabel { get; set; }

		public string LeftColumnCSSClass { get; set; }

		public string RightColumnCSSClass { get; set; }
	}

	public class SharePointAddFolderModal : Modal
	{
		public List<LanguageResources> NameLabel { get; set; }

		public List<LanguageResources> DestinationFolderLabel { get; set; }

		public string LeftColumnCSSClass { get; set; }

		public string RightColumnCSSClass { get; set; }
	}

	public class LookupModal : Modal
	{
		public List<LanguageResources> DefaultErrorMessage { get; set; }

		public List<LanguageResources> SelectRecordsTitle { get; set; }

		public GridOptions GridSettings { get; set; }
	}

	public class ErrorModal : Modal
	{
		public List<LanguageResources> Body { get; set; }
	}

	public class DeleteModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class CloseIncidentModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class WorkflowModal : Modal { }

	public class DisassociateModal : Modal { }

	public class ResolveCaseModal : Modal { }

	public class ReopenCaseModal : Modal { }

	public class CancelCaseModal : Modal { }

	public class ActivateModal : Modal { }

	public class DeactivateModal : Modal { }

	public class ActivateQuoteModal : Modal { }

	public class SetOpportunityOnHoldModal : Modal { }

	public class ReopenOpportunityModal : Modal { }

	public class WinOpportunityModal : Modal { }

	public class LoseOpportunityModal : Modal { }

	public class GenerateQuoteFromOpportunityModal : Modal { }

	public class UpdatePipelinePhaseModal : Modal { }

	public class QualifyLeadModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class ConvertQuoteModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class ConvertOrderModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class CalculateOpportunityModal : Modal
	{
		public List<LanguageResources> Confirmation { get; set; }
	}

	public class FormModal : Modal
	{
		public List<LanguageResources> LoadingMessage { get; set; }
	}

	public class EditFormModal : FormModal { }

	public class DetailsFormModal : FormModal { }

	public class CreateFormModal : FormModal { }

	public class CreateRelatedRecordModal : FormModal { }
}
