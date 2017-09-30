/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ModalDrop : PortalDrop
	{
		private readonly Lazy<string> _closeButtonText = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _dismissButtonSrText = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _primaryButtonText = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _title = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _type = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		
		public ModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext)
		{
			if (modal == null) return;
			CssClass = modal.CssClass;
			TitleCssClass = modal.TitleCssClass;
			PrimaryButtonCssClass = modal.PrimaryButtonCssClass;
			CloseButtonCssClass = modal.CloseButtonCssClass;
			Size = modal.Size == null ? null : modal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default).ToString();
			SizeCssClass = GetCssClassForModalSize(modal.Size);
			_title = Localization.CreateLazyLocalizedString(modal.Title, languageCode);
			_primaryButtonText = Localization.CreateLazyLocalizedString(modal.PrimaryButtonText, languageCode);
			_dismissButtonSrText = Localization.CreateLazyLocalizedString(modal.DismissButtonSrText, languageCode);
			_closeButtonText = Localization.CreateLazyLocalizedString(modal.CloseButtonText, languageCode);
			_type = new Lazy<string>(() => modal.GetType().Name, LazyThreadSafetyMode.None);
		}

		public string CloseButtonCssClass { get; set; }

		public string CloseButtonText { get { return _closeButtonText.Value; } }
		
		public string CssClass { get; set; }

		public string DismissButtonSrText { get { return _dismissButtonSrText.Value; } }

		public string PrimaryButtonCssClass { get; set; }

		public string PrimaryButtonText { get { return _primaryButtonText.Value; } }
		
		public string Size { get; set; }

		public string SizeCssClass { get; set; }

		public string Title { get { return _title.Value; } }

		public string TitleCssClass { get; set; }

		public string Type { get { return _type.Value; } }

		private string GetCssClassForModalSize(BootstrapExtensions.BootstrapModalSize? size)
		{
			if (size == null) return null;

			switch (size)
			{
				case BootstrapExtensions.BootstrapModalSize.Large:
					return "modal-lg";
				case BootstrapExtensions.BootstrapModalSize.Small:
					return "modal-sm";
				default:
					return null;
			}
		}
	}

	public class ErrorModalDrop : ModalDrop
	{
		private readonly Lazy<string> _body = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);

		public ErrorModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
			var errorModal = modal as ErrorModal;
			if (errorModal == null) return;
			_body = Localization.CreateLazyLocalizedString(errorModal.Body, languageCode);
		}

		public string Body { get { return _body.Value; } }
	}

	public class DeleteModalDrop : ModalDrop
	{
		private readonly Lazy<string> _confirmation = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);

		public DeleteModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode)
			: base(portalLiquidContext, modal, languageCode)
		{
			var deleteModal = modal as DeleteModal;
			if (deleteModal == null) return;
			_confirmation = Localization.CreateLazyLocalizedString(deleteModal.Confirmation, languageCode);
		}

		public string Confirmation { get { return _confirmation.Value; } }
	}

	public class FormModalDrop : ModalDrop
	{
		private readonly Lazy<string> _loadingMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);

		public FormModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
			var formModal = modal as FormModal;
			if (formModal == null) return;
			_loadingMessage = Localization.CreateLazyLocalizedString(formModal.LoadingMessage, languageCode);
		}

		public string LoadingMessage { get { return _loadingMessage.Value; } }
	}

	public class CreateFormModalDrop : FormModalDrop
	{
		public CreateFormModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
		}
	}

	public class CreateRelatedRecordModalDrop : FormModalDrop
	{
		public CreateRelatedRecordModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
			
		}
	}

	public class DetailsFormModalDrop : FormModalDrop
	{
		public DetailsFormModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode)
			: base(portalLiquidContext, modal, languageCode)
		{
		}
	}

	public class EditFormModalDrop : FormModalDrop
	{
		public EditFormModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode)
			: base(portalLiquidContext, modal, languageCode)
		{
		}
	}

	public class NoteModalDrop : ModalDrop
	{
		private readonly Lazy<string> _attachFileLabel = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _noteFieldLabel = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _privacyOptionFieldLabel = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		
		public NoteModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
			var noteModal = modal as NoteModal;
			if (noteModal == null) return;
			AttachFileAccept = noteModal.AttachFileAccept;
			DisplayAttachFile = noteModal.DisplayAttachFile.GetValueOrDefault(true);
			DisplayPrivacyOptionField = noteModal.DisplayPrivacyOptionField.GetValueOrDefault(false);
			LeftColumnCSSClass = noteModal.LeftColumnCSSClass;
			NoteFieldColumns = noteModal.NoteFieldColumns;
			NoteFieldRows = noteModal.NoteFieldRows;
			PrivacyOptionFieldDefaultValue = noteModal.PrivacyOptionFieldDefaultValue.GetValueOrDefault(false);
			RightColumnCSSClass = noteModal.RightColumnCSSClass;
			_attachFileLabel = Localization.CreateLazyLocalizedString(noteModal.AttachFileLabel, languageCode);
			_noteFieldLabel = Localization.CreateLazyLocalizedString(noteModal.NoteFieldLabel, languageCode);
			_privacyOptionFieldLabel = Localization.CreateLazyLocalizedString(noteModal.PrivacyOptionFieldLabel, languageCode);
		}
		
		public string AttachFileAccept { get; set; }

		public string AttachFileLabel { get { return _attachFileLabel.Value; } }

		public bool? DisplayAttachFile { get; set; }

		public bool? DisplayPrivacyOptionField { get; set; }

		public string LeftColumnCSSClass { get; set; }

		public int? NoteFieldColumns { get; set; }

		public string NoteFieldLabel { get { return _noteFieldLabel.Value; } }

		public int? NoteFieldRows { get; set; }

		public string PrivacyOptionFieldLabel { get { return _privacyOptionFieldLabel.Value; } }

		public bool? PrivacyOptionFieldDefaultValue { get; set; }

		public string RightColumnCSSClass { get; set; }
	}

	public class LookupModalDrop : ModalDrop
	{
		private readonly Lazy<string> _defaultErrorMessage = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<string> _selectRecordsTitle = new Lazy<string>(() => string.Empty, LazyThreadSafetyMode.None);
		private readonly Lazy<GridOptionsDrop> _gridOptions = new Lazy<GridOptionsDrop>(() => null, LazyThreadSafetyMode.None);

		public LookupModalDrop(IPortalLiquidContext portalLiquidContext, Modal modal, int languageCode) : base(portalLiquidContext, modal, languageCode)
		{
			var lookupModal = modal as LookupModal;
			if (lookupModal == null) return;
			_defaultErrorMessage = Localization.CreateLazyLocalizedString(lookupModal.DefaultErrorMessage, languageCode);
			_selectRecordsTitle = Localization.CreateLazyLocalizedString(lookupModal.SelectRecordsTitle, languageCode);
			_gridOptions = new Lazy<GridOptionsDrop>(() => new GridOptionsDrop(portalLiquidContext, lookupModal.GridSettings, languageCode), LazyThreadSafetyMode.None);
		}

		public string DefaultErrorMessage { get { return _defaultErrorMessage.Value; } }

		public string SelectRecordsTitle { get { return _selectRecordsTitle.Value; } }

		public GridOptionsDrop GridOptions { get { return _gridOptions.Value; } }
	}
}
