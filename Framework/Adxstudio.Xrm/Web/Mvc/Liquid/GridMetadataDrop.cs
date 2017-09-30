/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Action = Adxstudio.Xrm.Web.UI.JsonConfiguration.Action;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class GridMetadataDrop : PortalDrop
	{
		private readonly Lazy<string> _loadingMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _errorMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _accessDeniedMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _emptyMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<IEnumerable<ActionDrop>> _viewActions = new Lazy<IEnumerable<ActionDrop>>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<IEnumerable<ActionDrop>> _itemActions = new Lazy<IEnumerable<ActionDrop>>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<DetailsFormModalDrop> _detailsFormModal = new Lazy<DetailsFormModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<EditFormModalDrop> _editFormModal = new Lazy<EditFormModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<CreateFormModalDrop> _createFormModal = new Lazy<CreateFormModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<DeleteModalDrop> _deleteModal = new Lazy<DeleteModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<ErrorModalDrop> _errorModal = new Lazy<ErrorModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<LookupModalDrop> _lookupModal = new Lazy<LookupModalDrop>(() => null, LazyThreadSafetyMode.None);
		private readonly Lazy<CreateRelatedRecordModalDrop> _createRecordModal = new Lazy<CreateRelatedRecordModalDrop>(() => null, LazyThreadSafetyMode.None);

		public GridMetadataDrop(IPortalLiquidContext portalLiquidContext, GridMetadata gridMetadata, int languageCode) : base(portalLiquidContext)
		{
			if (gridMetadata == null) return;
			CssClass = gridMetadata.CssClass;
			GridCssClass = gridMetadata.GridCssClass;
			GridColumnWidthStyle = gridMetadata.GridColumnWidthStyle == null ? null : gridMetadata.GridColumnWidthStyle.GetValueOrDefault(EntityGridExtensions.GridColumnWidthStyle.Percent).ToString();
			ColumnOverrides = gridMetadata.ColumnOverrides != null ? gridMetadata.ColumnOverrides.Select(c => new ViewColumnDrop(portalLiquidContext, c)) : null;
			_viewActions = new Lazy<IEnumerable<ActionDrop>>(() => gridMetadata.ViewActions != null ? gridMetadata.ViewActions.Select(a => CreateActionDrop(portalLiquidContext, a, languageCode)) : null, LazyThreadSafetyMode.None);
			_itemActions = new Lazy<IEnumerable<ActionDrop>>(() => gridMetadata.ItemActions != null ? gridMetadata.ItemActions.Select(a => CreateActionDrop(portalLiquidContext, a, languageCode)) : null, LazyThreadSafetyMode.None);
			_loadingMessage = Localization.CreateLazyLocalizedString(gridMetadata.LoadingMessage, languageCode);
			_errorMessage = Localization.CreateLazyLocalizedString(gridMetadata.ErrorMessage, languageCode);
			_accessDeniedMessage = Localization.CreateLazyLocalizedString(gridMetadata.AccessDeniedMessage, languageCode);
			_emptyMessage = Localization.CreateLazyLocalizedString(gridMetadata.EmptyMessage, languageCode);
			_detailsFormModal = new Lazy<DetailsFormModalDrop>(() => new DetailsFormModalDrop(portalLiquidContext, gridMetadata.DetailsFormDialog, languageCode), LazyThreadSafetyMode.None);
			_editFormModal = new Lazy<EditFormModalDrop>(() => new EditFormModalDrop(portalLiquidContext, gridMetadata.EditFormDialog, languageCode), LazyThreadSafetyMode.None);
			_createFormModal = new Lazy<CreateFormModalDrop>(() => new CreateFormModalDrop(portalLiquidContext, gridMetadata.CreateFormDialog, languageCode), LazyThreadSafetyMode.None);
			_deleteModal = new Lazy<DeleteModalDrop>(() => new DeleteModalDrop(portalLiquidContext, gridMetadata.DeleteDialog, languageCode), LazyThreadSafetyMode.None);
			_errorModal = new Lazy<ErrorModalDrop>(() => new ErrorModalDrop(portalLiquidContext, gridMetadata.ErrorDialog, languageCode), LazyThreadSafetyMode.None);
			_lookupModal = new Lazy<LookupModalDrop>(() => new LookupModalDrop(portalLiquidContext, gridMetadata.LookupDialog, languageCode), LazyThreadSafetyMode.None);
			_createRecordModal = new Lazy<CreateRelatedRecordModalDrop>(() => new CreateRelatedRecordModalDrop(portalLiquidContext, gridMetadata.CreateRelatedRecordDialog, languageCode), LazyThreadSafetyMode.None);
		}

		public IEnumerable<ActionDrop> ViewActions { get { return _viewActions.Value; } }

		public IEnumerable<ActionDrop> ItemActions { get { return _itemActions.Value; } }

		public string CssClass { get; set; }

		public string GridCssClass { get; set; }

		public string GridColumnWidthStyle { get; set; }

		public string LoadingMessage { get { return _loadingMessage.Value; } }

		public string ErrorMessage { get { return _errorMessage.Value; } }

		public string AccessDeniedMessage { get { return _accessDeniedMessage.Value; } }

		public string EmptyMessage { get { return _emptyMessage.Value; } }

		public DetailsFormModalDrop DetailsFormDialog { get { return _detailsFormModal.Value; } }

		public EditFormModalDrop EditFormDialog { get { return _editFormModal.Value; } }

		public CreateFormModalDrop CreateFormDialog { get { return _createFormModal.Value; } }

		public DeleteModalDrop DeleteDialog { get { return _deleteModal.Value; } }

		public ErrorModalDrop ErrorDialog { get { return _errorModal.Value; } }

		public LookupModalDrop LookupDialog { get { return _lookupModal.Value; } }

		public IEnumerable<ViewColumnDrop> ColumnOverrides { get; set; }

		public CreateRelatedRecordModalDrop CreateRelatedRecordDialog { get { return _createRecordModal.Value; } }

		private static ActionDrop CreateActionDrop(IPortalLiquidContext portalLiquidContext, Action action, int languageCode)
		{
			if (action is CreateRelatedRecordAction)
			{
				return new CreateRelatedRecordActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is DetailsAction)
			{
				return new DetailsActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is EditAction)
			{
				return new EditActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is CreateAction)
			{
				return new CreateActionDrop(portalLiquidContext, action, languageCode);
			}
			
			if (action is WorkflowAction)
			{
				return new WorkflowActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is DeleteAction)
			{
				return new DeleteActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is AssociateAction)
			{
				return new AssociateActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is DisassociateAction)
			{
				return new DisassociateActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is SearchAction)
			{
				return new SearchActionDrop(portalLiquidContext, action, languageCode);
			}

			if (action is DownloadAction)
			{
				return new DownloadActionDrop(portalLiquidContext, action, languageCode);
			}
			
			return new ActionDrop(portalLiquidContext, action, languageCode);
		}
	}
}
