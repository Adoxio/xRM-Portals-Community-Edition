/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class Action
	{
		public List<LanguageResources> ButtonLabel { get; set; }

		public List<LanguageResources> ButtonTooltip { get; set; }

		public string ButtonCssClass { get; set; }

		public int? ActionIndex { get; set; }

		public List<LanguageResources> SuccessMessage { get; set; }

		public ActionButtonAlignment? ActionButtonAlignment { get; set; }

		public ActionButtonStyle? ActionButtonStyle { get; set; }

		public ActionButtonPlacement? ActionButtonPlacement { get; set; }

		public virtual bool IsConfigurationValid()
		{
			return true;
		}

		public List<LanguageResources> Confirmation { get; set; }

		public ShowModal? ShowModal { get; set; }

		/// <summary>
		/// Filter Criteria to show/hide button.
		/// </summary>
		public string FilterCriteria { get; set; }

		/// <summary>
		/// Text which displayed when button clicked.
		/// </summary>
		public List<LanguageResources> ButtonBusyLabel { get; set; }
	}

	public class NotSupportedAction : Action { }

	public class SubmitAction : Action { }

	public class NextAction : Action { }

	public class PreviousAction : Action { }

	public class EditAction : RecordFormViewAction
	{
	}

	public class DetailsAction : RecordFormViewAction
	{
	}

	public class CreateAction : FormViewAction
	{
	}

	public class AssociateAction : RedirectAction
	{
		public Guid ViewId { get; set; }

		public override bool IsConfigurationValid()
		{
			return ViewId != Guid.Empty;
		}
	}

	public class DisassociateAction : RedirectAction
	{
	}

	public class SearchAction : Action
	{
		public string PlaceholderText { get; set; }

		public string TooltipText { get; set; }
	}

	public class DownloadAction : Action
	{
		public List<LanguageResources> CurrentPageLabel { get; set; }

		public List<LanguageResources> AllPagesLabel { get; set; }
	}

	public class ReopenCaseAction : RedirectAction
	{
	}

	public class ActivateAction : RedirectAction
	{
	}

	public class DeactivateAction : RedirectAction
	{
	}

	public class ActivateQuoteAction : RedirectAction
	{
	}

	public class SetOpportunityOnHoldAction : RedirectAction
	{
	}

	public class ReopenOpportunityAction : RedirectAction
	{
	}

	public class UpdatePipelinePhaseAction : RedirectAction
	{
		public List<LanguageResources> StepNameLabel { get; set; }

		public List<LanguageResources> DescriptionLabel { get; set; }
	}

	public class RecordFormViewAction : FormViewAction
	{
		public string RecordIdQueryStringParameterName { get; set; }
	}

	public class FormViewAction : RedirectAction
	{
		public Guid? EntityFormId { get; set; }

		public TargetType? TargetType { get; set; }

		public override bool IsConfigurationValid()
		{
			switch (TargetType.GetValueOrDefault(JsonConfiguration.TargetType.EntityForm))
			{
				case JsonConfiguration.TargetType.EntityForm:
					return EntityFormId != Guid.Empty;
				case JsonConfiguration.TargetType.WebPage:
					return RedirectWebpageId != Guid.Empty;
				case JsonConfiguration.TargetType.Url:
					return !string.IsNullOrEmpty(RedirectUrl);
				default:
					return false;
			}
		}
	}

	public class RedirectAction : Action
	{
		public OnComplete? OnComplete { get; set; }

		public Guid? RedirectWebpageId { get; set; }

		public string RedirectUrl { get; set; }

		public virtual string GetRedirectWebPageUrl(IPortalContext context, string portalName = null)
		{
			if (context == null || RedirectWebpageId == null || RedirectWebpageId == Guid.Empty) return null;

			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(portalName);

			var page = contentMapProvider.Using(contentMap => Select(RedirectWebpageId.GetValueOrDefault(), contentMap));

			if (page == null) return null;

			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IContentMapEntityUrlProvider>();

			return contentMapProvider.Using(contentMap => urlProvider.GetUrl(contentMap, page));
		}

		private EntityNode Select(Guid id, ContentMap contentMap)
		{
			EntityNode page;

			return !contentMap.TryGetValue(new EntityReference("adx_webpage", id), out page) ? null : page;
		}
	}

	public class WorkflowAction : RedirectAction
	{
		public Guid WorkflowId { get; set; }

		public List<LanguageResources> WorkflowDialogTitle { get; set; }

		public List<LanguageResources> WorkflowDialogPrimaryButtonText { get; set; }

		public List<LanguageResources> WorkflowDialogCloseButtonText { get; set; }

		public override bool IsConfigurationValid()
		{
			return WorkflowId != Guid.Empty;
		}
	}

	public class DeleteAction : RedirectAction
	{
	}

	public class CloseIncidentAction : RedirectAction
	{
		public string DefaultResolution { get; set; }

		public string DefaultResolutionDescription { get; set; }
	}

	public class ResolveCaseAction : RedirectAction
	{
		public List<LanguageResources> SubjectLabel { get; set; }

		public List<LanguageResources> DescriptionLabel { get; set; }
	}

	public class CancelCaseAction : RedirectAction
	{
	}

	public class QualifyLeadAction : RedirectAction
	{
	}

	public class ConvertQuoteToOrderAction : RedirectAction
	{
	}

	public class ConvertOrderToInvoiceAction : RedirectAction
	{
	}

	public class CalculateOpportunityAction : RedirectAction
	{
	}

	public class WinOpportunityAction : RedirectAction
	{
	}

	public class LoseOpportunityAction : RedirectAction
	{
	}

	public class GenerateQuoteFromOpportunityAction : RedirectAction
	{
	}

	public class CreateRelatedRecordAction : RecordFormViewAction
	{
		public string EntityName { get; set; }
		public string Relationship { get; set; }
		public string ParentRecord { get; set; }
	}

	public enum ShowModal
	{
		No = 0,
		Yes = 1
	}

	public enum ActionButtonAlignment
	{
		Left = 0,
		Right = 1
	}

	public enum ActionButtonStyle
	{
		ButtonGroup = 0,
		DropDown = 1
	}

	public enum ActionButtonPlacement
	{
		AboveForm = 0,
		BelowForm = 1
	}

	public enum TargetType
	{
		EntityForm = 0,
		WebPage = 1,
		Url = 2
	}

	public enum OnComplete
	{
		Refresh = 0,
		RedirectToWebPage = 1,
		RedirectToUrl = 2
	}
}
