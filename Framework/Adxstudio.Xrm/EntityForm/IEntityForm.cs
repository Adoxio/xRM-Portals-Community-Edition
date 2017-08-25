/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.EntityForm
{
	public interface IEntityForm
	{
		Entity entity { get; set; }

		IEnumerable<Entity> EntityFormMetadata { get; set; }

		string EntityName { get; set; }

		string FormName { get; set; }

		string TabName { get; set; }

		bool RecommendedFieldsRequired { get; set; }

		bool RenderWebResourcesInline { get; set; }

		bool ShowOwnerFields { get; set; }

		bool ShowUnsupportedFields { get; set; }

		bool ToolTipEnabled { get; set; }

		bool AutoGenerateStepsFromTabs { get; set; }

		bool ForceAllFieldsRequired { get; set; }

		bool HideFormOnSuccess { get; set; }

		string ValidationGroup { get; set; }

		bool ValidationSummaryLinksEnabled { get; set; }

		string ValidationSummaryCssClass { get; set; }

		string LocalizedValidationSummaryHeaderText { get; set; }

		bool CaptchaRequired { get; set; }

		bool AttachFile { get; set; }

		bool AttachFileAllowMultiple { get; set; }

		string AttachFileAccept { get; set; }

        string AttachFileAcceptExtensions { get; set; }
		bool AttachFileRestrictAccept { get; set; }

		string AttachFileTypeErrorMessage { get; set; }

		int? AttachFileMaxSize { get; set; }

		bool AttachFileRestrictSize { get; set; }

		string AttachFileSizeErrorMessage { get; set; }

		bool AttachFileRequired { get; set; }

		string LocalizedAttachFileLabel { get; set; }

		string AttachFileLabel { get; set; }

		string AttachFileRequiredErrorMessage { get; set; }

		OptionSetValue AttachFileStorageLocation { get; set; }

		string EntityformPreviousButtonCssClass { get; set; }

		string EntityformNextButtonCssClass { get; set; }

		string EntityformSubmitButtonCssClass { get; set; }

		string LocalizedPreviousButtonText { get; set; }

		string LocalizedNextButtonText { get; set; }

		string LocalizedSubmitButtonText { get; set; }

		string LocalizedSubmitButtonBusyText { get; set; }

		string SuccessMessage { get; set; }

		string PreviousButtonCssClass { get; set; }

		string NextButtonCssClass { get; set; }

		string SubmitButtonCssClass { get; set; }

		string PreviousButtonText { get; set; }

		string NextButtonText { get; set; }

		string SubmitButtonText { get; set; }

		string SubmitButtonBusyText { get; set; }

		bool PopulateReferenceEntityLookupField { get; set; }

		string TargetAttributeName { get; set; }

		string ReferenceEntityLogicalName { get; set; }

		string ReferenceQueryStringName { get; set; }

		bool QuerystringIsPrimaryKey { get; set; }

		string ReferenceEntityPrimaryKeyLogicalName { get; set; }

		string ReferenceQueryAttributeName { get; set; }
	}
}
