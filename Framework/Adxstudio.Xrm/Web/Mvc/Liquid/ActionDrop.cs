/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Action = Adxstudio.Xrm.Web.UI.JsonConfiguration.Action;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ActionDrop : PortalDrop
	{
		private readonly Lazy<string> _buttonLabel;
		private readonly Lazy<string> _buttonTooltip;
		private readonly Lazy<string> _type;

		public ActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext)
		{
			if (action == null) return;
			_buttonLabel = Localization.CreateLazyLocalizedString(action.ButtonLabel, languageCode);
			_buttonTooltip = Localization.CreateLazyLocalizedString(action.ButtonTooltip, languageCode);
			_type = new Lazy<string>(() => action.GetType().Name, LazyThreadSafetyMode.None);
		}

		public string ButtonLabel { get { return _buttonLabel.Value; } }

		public string ButtonTooltip { get { return _buttonTooltip.Value; } }

		public string Type { get { return _type.Value; } }
	}

	public class DetailsActionDrop : ActionDrop
	{
		public DetailsActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var detailsAction = action as DetailsAction;
			if (detailsAction == null) return;
			RecordIdQueryStringParameterName = detailsAction.RecordIdQueryStringParameterName;
			EntityFormId = detailsAction.EntityFormId;
		}

		public Guid? EntityFormId { get; set; }

		public string RecordIdQueryStringParameterName { get; set; }
	}

	public class EditActionDrop : ActionDrop
	{
		public EditActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var editAction = action as EditAction;
			if (editAction == null) return;
			RecordIdQueryStringParameterName = editAction.RecordIdQueryStringParameterName;
			EntityFormId = editAction.EntityFormId;
		}

		public Guid? EntityFormId { get; set; }

		public string RecordIdQueryStringParameterName { get; set; }
	}

	public class CreateActionDrop : ActionDrop
	{
		public CreateActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var createAction = action as CreateAction;
			if (createAction == null) return;
			EntityFormId = createAction.EntityFormId;
		}

		public Guid? EntityFormId { get; set; }
	}

	public class CreateRelatedRecordActionDrop : ActionDrop
	{
		public CreateRelatedRecordActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var createAction = action as CreateRelatedRecordAction;
			if (createAction == null) return;
			EntityName = createAction.EntityName;
			Relationship = createAction.Relationship;
			ParentRecord = createAction.ParentRecord;
			RecordIdQueryStringParameterName = createAction.RecordIdQueryStringParameterName;
		}
		public string EntityName { get; set; }
		public string Relationship { get; set; }
		public string ParentRecord { get; set; }
		public string RecordIdQueryStringParameterName { get; set; }
	}

	public class AssociateActionDrop : ActionDrop
	{
		public AssociateActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var associateAction = action as AssociateAction;
			if (associateAction == null) return;
			ViewId = associateAction.ViewId;
		}

		public Guid? ViewId { get; set; }
	}

	public class DisassociateActionDrop : ActionDrop
	{
		public DisassociateActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
		}
	}

	public class WorkflowActionDrop : ActionDrop
	{
		public WorkflowActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var workflowAction = action as WorkflowAction;
			if (workflowAction == null) return;
			WorkflowId = workflowAction.WorkflowId;
		}

		public Guid? WorkflowId { get; set; }
	}

	public class DeleteActionDrop : ActionDrop
	{
		private readonly Lazy<string> _confirmation;

		public DeleteActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var deleteAction = action as DeleteAction;
			if (deleteAction == null) return;
			RedirectWebpageId = deleteAction.RedirectWebpageId;
			RedirectUrl = deleteAction.RedirectUrl;
			_confirmation = Localization.CreateLazyLocalizedString(deleteAction.Confirmation, languageCode);
		}

		public Guid? RedirectWebpageId { get; set; }

		public string RedirectUrl { get; set; }

		public string Confirmation { get { return _confirmation.Value; } }
	}

	public class SearchActionDrop : ActionDrop
	{
		private readonly Lazy<string> _placeholderText;
		private readonly Lazy<string> _tooltipText;

		public SearchActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var searchAction = action as SearchAction;
			if (searchAction == null) return;
			_placeholderText = new Lazy<string>(() => searchAction.PlaceholderText, LazyThreadSafetyMode.None);
			_tooltipText = new Lazy<string>(() => searchAction.TooltipText, LazyThreadSafetyMode.None);
		}

		public string PlaceholderText { get { return _placeholderText.Value; } }

		public string TooltipText { get { return _tooltipText.Value; } }
	}

	public class DownloadActionDrop : ActionDrop
	{
		private readonly Lazy<string> _currentPageLabel;
		private readonly Lazy<string> _allPagesLabel;

		public DownloadActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
			: base(portalLiquidContext, action, languageCode)
		{
			var downloadAction = action as DownloadAction;
			if (downloadAction == null) return;
			_currentPageLabel = Localization.CreateLazyLocalizedString(downloadAction.CurrentPageLabel, languageCode);
			_allPagesLabel = Localization.CreateLazyLocalizedString(downloadAction.AllPagesLabel, languageCode);
		}

		public string CurrentPageLabel { get { return _currentPageLabel.Value; } }

		public string AllPagesLabel { get { return _allPagesLabel.Value; } }
	}
}
