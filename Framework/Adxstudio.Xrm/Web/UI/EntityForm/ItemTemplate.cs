/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	public class ItemTemplate : ITemplate
	{
		private readonly string _validationGroup;
		private readonly bool _captchaIsRequired;
		private readonly bool _attachFile;
		private readonly bool _attachFileAllowMultiple;
		private readonly string _attachFileAccept;
		private readonly bool _attachFileRestrictAccept;
		private readonly string _attachFileAcceptErrorMessage;
		private readonly ulong? _attachFileSize;
		private readonly bool _attachFileRestrictSize;
		private readonly string _attachFileSizeErrorMessage;
		private readonly string _attachFileLabel;
		private readonly bool _attachFileIsRequired;
		private readonly string _attachFileRequiredErrorMessage;
		private readonly string _submitButtonID;
		private readonly string _submitButtonCommandName;
		private readonly string _submitButtonText;
		private readonly string _submitButtonCssClass;
		private readonly bool _submitButtonCauseValidation;
		private readonly bool _addSubmitButton;
		private readonly string _submitButtonBusyText;

		private readonly string DefaultAttachFileLabel = ResourceManager.GetString("Attach_A_File_DefaultText");

		public ItemTemplate(string validationGroup, bool captchaIsRequired = false, bool attachFile = false, bool attachFileAllowMultiple = false, string attachFileAccept = "", bool attachFileRestrictAccept = false, string attachFileAcceptErrorMessage = "", ulong? attachFileSize = 0, bool attachFileRestrictSize = false, string attachFileSizeErrorMessage = "", string attachFileLabel = "", bool attachFileIsRequired = false, string attachFileRequiredErrorMessage = "", bool addSubmitButton = false, string submitButtonID = "", string submitButtonCommmandName = "", string submitButtonText = "", string submitButtonCssClass = "", bool submitButtonCauseValidation = false, string submitButtonBusyText = "")
		{
			_validationGroup = validationGroup ?? string.Empty;
			_captchaIsRequired = captchaIsRequired;
			_attachFile = attachFile;
			_attachFileAllowMultiple = attachFileAllowMultiple;
			_attachFileAccept = attachFileAccept;
			_attachFileRestrictAccept = attachFileRestrictAccept;
			_attachFileLabel = string.IsNullOrWhiteSpace(attachFileLabel) ? DefaultAttachFileLabel : attachFileLabel;
			_attachFileSize = attachFileSize;
			_attachFileRestrictSize = attachFileRestrictSize;
			_attachFileIsRequired = attachFileIsRequired;
			_attachFileAcceptErrorMessage = string.IsNullOrWhiteSpace(attachFileAcceptErrorMessage) ? "{0} is not of the file type(s) \"{1}\".".FormatWith(_attachFileLabel, _attachFileAccept) : attachFileAcceptErrorMessage;
			_attachFileRequiredErrorMessage = string.IsNullOrWhiteSpace(attachFileRequiredErrorMessage) ? "{0} is a required field.".FormatWith(_attachFileLabel) : attachFileRequiredErrorMessage;
			_attachFileSizeErrorMessage = attachFileSizeErrorMessage;
			_addSubmitButton = addSubmitButton;
			_submitButtonID = submitButtonID;
			_submitButtonCommandName = submitButtonCommmandName;
			_submitButtonText = submitButtonText;
			_submitButtonCssClass = submitButtonCssClass;
			_submitButtonCauseValidation = submitButtonCauseValidation;
			_submitButtonBusyText = submitButtonBusyText;
		}

		public void InstantiateIn(Control container)
		{
			if (_attachFile)
			{
				var row = new HtmlGenericControl("div");
				row.Attributes.Add("class", "tr");

				var cell = new HtmlGenericControl("div");
				cell.Attributes.Add("class", "cell file-cell");

				var info = new HtmlGenericControl("div");
				info.Attributes.Add("class", _attachFileIsRequired ? "info required" : "info");

				var label = new Label { ID = "AttachFileLabel", Text = _attachFileLabel, AssociatedControlID = "AttachFile" };
				info.Controls.Add(label);

				var ctl = new HtmlGenericControl("div");
				ctl.Attributes.Add("class", "control");

				var file = new FileUpload { ID = "AttachFile", AllowMultiple = _attachFileAllowMultiple };

				file.Attributes.Add("accept", _attachFileAccept);

				ctl.Controls.Add(file);
				
				if (_attachFileRestrictAccept)
				{
					var validator = new CustomValidator
					{
						ID = string.Format("AttachFileAcceptValidator{0}", file.ID),
						ControlToValidate = file.ID,
						ValidationGroup = _validationGroup,
						Display = ValidatorDisplay.None,
						ErrorMessage = _attachFileAcceptErrorMessage
					};
					validator.ServerValidate += (sender, args) => ValidateFileAccept(file, args);

					ctl.Controls.Add(validator);
				}

				if (_attachFileRestrictSize)
				{
					var validator = new CustomValidator
					{
						ID = string.Format("AttachFileSizeValidator{0}", file.ID),
						ControlToValidate = file.ID,
						ValidationGroup = _validationGroup,
						Display = ValidatorDisplay.None,
						ErrorMessage = _attachFileSizeErrorMessage
					};
					validator.ServerValidate += (sender, args) => ValidateFileSize(file, args);

					ctl.Controls.Add(validator);
				}

				if (_attachFileIsRequired)
				{
					ctl.Controls.Add(new RequiredFieldValidator
					{
						ID = string.Format("RequiredFieldValidator{0}", file.ID),
						ControlToValidate = file.ID,
						ValidationGroup = _validationGroup,
						Display = ValidatorDisplay.None,
						ErrorMessage = _attachFileRequiredErrorMessage
					});
				}

				cell.Controls.Add(info);
				cell.Controls.Add(ctl);

				row.Controls.Add(cell);

				container.Controls.Add(row);
			}

			if (_captchaIsRequired)
			{
#if TELERIKWEBUI
				var row = new HtmlGenericControl("div");
				row.Attributes.Add("class", "tr");

				var cell = new HtmlGenericControl("div");
				cell.Attributes.Add("class", "cell");
				cell.Attributes.Add("class", "captcha-cell");

				RadCaptcha.RenderCaptcha(cell, "captcha", _validationGroup);

				row.Controls.Add(cell);

				container.Controls.Add(row);
#else
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Captcha is enabled; however, Telerik.Web.UI.dll could not be found.");
#endif
			}

			if (_addSubmitButton)
			{
				container.Controls.Add(new Button
				{
					ID = _submitButtonID,
					CommandName = _submitButtonCommandName,
					Text = _submitButtonText,
					ValidationGroup = _validationGroup,
					CssClass = _submitButtonCssClass,
					CausesValidation = _submitButtonCauseValidation,
					OnClientClick = "javascript:if(typeof entityFormClientValidate === 'function'){if(entityFormClientValidate()){if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + _validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{return false;}}else{if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + _validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}",
					UseSubmitBehavior = false
				});
			}
		}

		private void ValidateFileSize(FileUpload fileUpload, ServerValidateEventArgs args)
		{
			args.IsValid = true;

			if (!_attachFileSize.HasValue) return;

			if (!fileUpload.HasFiles) return;

			foreach (var uploadedFile in fileUpload.PostedFiles)
			{
				args.IsValid = Convert.ToUInt64(uploadedFile.ContentLength) <= _attachFileSize;
				if (!args.IsValid)
				{
					break;
				}
			}
		}

		private void ValidateFileAccept(FileUpload fileUpload, ServerValidateEventArgs args)
		{
			args.IsValid = true;

			if (!fileUpload.HasFiles) return;

			var regex = AnnotationDataAdapter.GetAcceptRegex(_attachFileAccept);
			foreach (var uploadedFile in fileUpload.PostedFiles)
			{
			    var path = System.IO.Path.GetExtension(uploadedFile.FileName);
				args.IsValid = regex.IsMatch(uploadedFile.ContentType) || regex.IsMatch(path);
				if (!args.IsValid)
				{
					break;
				}
			}
		}
	}
}
