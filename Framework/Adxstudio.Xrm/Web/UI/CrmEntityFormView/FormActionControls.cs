/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public static class FormActionControls
	{
		private static readonly string DefaultModalDeleteTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultModalDeleteBody = ResourceManager.GetString("Record_Deletion_Confirmation_Message");
		private static readonly string DefaultModalDeletePrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultModalDeleteCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static readonly string DefaultModalProcessingText = ResourceManager.GetString("Default_Modal_Processing_Text");

		public static  PlaceHolder FormActionLinkListItem(HtmlHelper html, ViewActionLink viewActionLink, ActionButtonStyle actionButtonStyle, bool autoGenerateSteps = false,
			string submitButtonID = "SubmitButton", string submitButtonCommandName = "", string validationGroup = null, string submitButtonBusyText = null,
			string actionNavbarCssClass = null, string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			submitButtonBusyText = submitButtonBusyText == null ? DefaultModalProcessingText : submitButtonBusyText;
			var placeHolder = new PlaceHolder();

			if (actionButtonStyle == ActionButtonStyle.DropDown)
			{
				placeHolder.Controls.Add(
					new LiteralControl("<li>"));
			}

			switch (viewActionLink.Type)
			{
				case LinkActionType.Submit:
					var control = FormActionSubmit(viewActionLink, submitButtonID, submitButtonCommandName, validationGroup, submitButtonBusyText, actionButtonStyle);
					placeHolder.Controls.Add(control);
					break;
				case LinkActionType.Next:
					var nextcontrol = FormActionNext(viewActionLink, nextButtonId, validationGroup, actionButtonStyle);
					placeHolder.Controls.Add(nextcontrol);
					break;
				case LinkActionType.Previous:
					var previouscontrol = FormActionPrevious(viewActionLink, previousButtonId, validationGroup, actionButtonStyle);
					placeHolder.Controls.Add(previouscontrol);
					break;
				default:
					var link = html.FormActionLink(viewActionLink, actionButtonStyle);
					placeHolder.Controls.Add(new LiteralControl(link.ToString()));
					break;
			}

			if (actionButtonStyle == ActionButtonStyle.DropDown)
			{
				placeHolder.Controls.Add(new LiteralControl("</li>"));
			}
			
			placeHolder.Controls.Add(new LiteralControl("\r\n")); // ensures proper whitespace between buttons
			
			return placeHolder;
		}

		public static Button FormActionSubmit(ViewActionLink viewActionLink, string submitButtonID = "SubmitButton",
			string submitButtonCommandName = "", string validationGroup = null, string submitButtonBusyText = null,
			ActionButtonStyle actionButtonStyle = ActionButtonStyle.ButtonGroup)
		{
			submitButtonBusyText = submitButtonBusyText ?? DefaultModalProcessingText;
			if (!string.IsNullOrEmpty(viewActionLink.BusyText))
			{
				submitButtonBusyText = viewActionLink.BusyText;
			}
			if (actionButtonStyle == ActionButtonStyle.DropDown) //This will no longer be a form-wdie setting in a future version
			{
				if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "btn btn-primary")
				{
					viewActionLink.ButtonCssClass = "submit-btn-link submit-btn";
				}
				else
				{
					viewActionLink.ButtonCssClass = "submit-btn-link submit-btn " + viewActionLink.ButtonCssClass;
				}
				
			}
			else if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "button submit"
				|| viewActionLink.ButtonCssClass == "btn btn-primary" || viewActionLink.ButtonCssClass == "btn-primary")
			{
				viewActionLink.ButtonCssClass = "btn btn-primary button submit-btn";
			}
			else
			{
				viewActionLink.ButtonCssClass = viewActionLink.ButtonCssClass + " submit-btn";
			}

			var submitButton = new Button
			{
				ID = submitButtonID,
				CommandName = submitButtonCommandName,
				Text = viewActionLink.Label,
				ValidationGroup = validationGroup,
				CssClass = viewActionLink.ButtonCssClass,
				CausesValidation = true,
				OnClientClick =
					"javascript:if(typeof entityFormClientValidate === 'function'){if(entityFormClientValidate()){if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" +
					validationGroup + "')){clearIsDirty();disableButtons();this.value = '" +
					submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" +
					submitButtonBusyText +
					"';}}else{return false;}}else{if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" +
					validationGroup + "')){clearIsDirty();disableButtons();this.value = '" +
					submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" +
					submitButtonBusyText + "';}}",
				UseSubmitBehavior = false,
				ToolTip = viewActionLink.Tooltip
			};

			return submitButton;
		}

		public static Button FormActionNext(ViewActionLink viewActionLink, string nextButtonID = "NextButton",
			string validationGroup = null, ActionButtonStyle actionButtonStyle = ActionButtonStyle.ButtonGroup)
		{
			if (actionButtonStyle == ActionButtonStyle.DropDown) //This will no longer be a form-wide setting in a future version
			{
				if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "btn btn-primary")
				{
					viewActionLink.ButtonCssClass = "submit-btn-link next-btn";
				}
				else
				{
					viewActionLink.ButtonCssClass = "submit-btn-link next-btn " + viewActionLink.ButtonCssClass;
				}

			}
			else if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "button next"
				|| viewActionLink.ButtonCssClass == "btn btn-primary" || viewActionLink.ButtonCssClass == "btn-primary")
			{
				viewActionLink.ButtonCssClass = "btn btn-primary button next next-btn";
			}
			else
			{
				viewActionLink.ButtonCssClass = viewActionLink.ButtonCssClass + " next-btn";
			}

			var nextButton = new Button
			{
				ID = nextButtonID,
				CommandName = "Next",
				Text = string.IsNullOrWhiteSpace(viewActionLink.Label) ? viewActionLink.Label : "Next",
				ValidationGroup = validationGroup,
				CausesValidation = true,
				CssClass = viewActionLink.ButtonCssClass
			};

			return nextButton;
		}

		public static Button FormActionPrevious(ViewActionLink viewActionLink, string previousButtonID = "PreviousButton",
			string validationGroup = null, ActionButtonStyle actionButtonStyle = ActionButtonStyle.ButtonGroup)
		{
			if (actionButtonStyle == ActionButtonStyle.DropDown) //This will no longer be a form-wide setting in a future version
			{
				if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "btn btn-default")
				{
					viewActionLink.ButtonCssClass = "submit-btn-link previous-btn";
				}
				else
				{
					viewActionLink.ButtonCssClass = "submit-btn-link previous-btn " + viewActionLink.ButtonCssClass;
				}

			}
			else if (string.IsNullOrEmpty(viewActionLink.ButtonCssClass) || viewActionLink.ButtonCssClass == "button next" || viewActionLink.ButtonCssClass == "button previous"
				|| viewActionLink.ButtonCssClass == "btn btn-default" || viewActionLink.ButtonCssClass == "btn-default")
			{
				viewActionLink.ButtonCssClass = "btn btn-default button previous previous-btn";
			}
			else
			{
				viewActionLink.ButtonCssClass = viewActionLink.ButtonCssClass + " previous-btn";
			}

			var previousButton = new Button
			{
				ID = previousButtonID,
				CommandName = "Previous",
				Text = string.IsNullOrWhiteSpace(viewActionLink.Label) ? viewActionLink.Label : "Previous",
				ValidationGroup = validationGroup,
				CausesValidation = false,
				CssClass = viewActionLink.ButtonCssClass,
				UseSubmitBehavior = false
			};

			return previousButton;
		}

		public static WebControl FormActionDropDownMenuControl()
		{
			var dropDownMenu = new WebControl(HtmlTextWriterTag.Ul);

			dropDownMenu.AddClass("dropdown-menu");
			dropDownMenu.Attributes.Add("role", "menu");

			return dropDownMenu;
		}

		public static WebControl FormActionDefaultNavBar(HtmlHelper html, FormConfiguration formConfiguration, WebControl leftContainer, WebControl rightContainer,
			ActionButtonPlacement actionButtonPlacement, string submitButtonID = "SubmitButton", string submitButtonCommandName = "", string validationGroup = null,
			string submitButtonBusyText = null, string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			submitButtonBusyText = submitButtonBusyText == null ? DefaultModalProcessingText : submitButtonBusyText;

			var dropDownLeft = FormActions(html, formConfiguration, leftContainer, actionButtonPlacement, ActionButtonAlignment.Left, submitButtonID, submitButtonCommandName, validationGroup, submitButtonBusyText, nextButtonId, previousButtonId);

			var dropDownRight = FormActions(html, formConfiguration, rightContainer, actionButtonPlacement, ActionButtonAlignment.Right, submitButtonID, submitButtonCommandName, validationGroup, submitButtonBusyText, nextButtonId, previousButtonId);

			var containerFluid = FormActionNavbarInnerHtml(html, dropDownLeft, dropDownRight, formConfiguration, actionButtonPlacement);

			var navBar = FormActionNavbarContainerControl(formConfiguration);

			navBar.Controls.Add(containerFluid);

			AddActionModalWindows(html, formConfiguration, navBar, actionButtonPlacement);

			return navBar;
		}

		public static WebControl FormActionNoNavBar(HtmlHelper html, FormConfiguration formConfiguration, WebControl leftContainer, WebControl rightContainer, 
			ActionButtonPlacement actionButtonPlacement, string submitButtonID = "SubmitButton", string submitButtonCommandName = "", string validationGroup = null,
			string submitButtonBusyText = null, string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			submitButtonBusyText = submitButtonBusyText == null ? DefaultModalProcessingText : submitButtonBusyText;

			var dropDownLeft = FormActions(html, formConfiguration, leftContainer, actionButtonPlacement, ActionButtonAlignment.Left, submitButtonID, submitButtonCommandName, validationGroup, submitButtonBusyText, nextButtonId, previousButtonId);

			var dropDownRight = FormActions(html, formConfiguration, rightContainer, actionButtonPlacement, ActionButtonAlignment.Right, submitButtonID, submitButtonCommandName, validationGroup, submitButtonBusyText, nextButtonId, previousButtonId);

			var navBar = FormActionNavbarContainerControl(formConfiguration);

			navBar.Controls.Add(dropDownLeft);

			navBar.Controls.Add(dropDownRight);

			AddActionModalWindows(html, formConfiguration, navBar, actionButtonPlacement);

			return navBar;
		}

		public static WebControl FormActions(HtmlHelper html, FormConfiguration formConfiguration, WebControl container,
			ActionButtonPlacement actionButtonPlacement, ActionButtonAlignment actionButtonAlignment, string submitButtonID = "SubmitButton",  
			string submitButtonCommandName = "", string validationGroup = null, string submitButtonBusyText = null, 
			string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			submitButtonBusyText = submitButtonBusyText == null ? DefaultModalProcessingText : submitButtonBusyText;

			var buttonActionLinks = new List<Control>();

			var dropdownActionLinks = new List<Control>();

			var actionLinks = (actionButtonPlacement == ActionButtonPlacement.AboveForm)
				? formConfiguration.TopFormActionLinks
				: formConfiguration.BottomFormActionLinks;

			foreach (var action in actionLinks)
			{
				if (action is WorkflowActionLink && action.Enabled && (action.ActionButtonAlignment ?? ActionButtonAlignment.Left)  == actionButtonAlignment)
				{
					if ((action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup) == ActionButtonStyle.DropDown)
					{
						var workflowHtml = FormActionLinkListItem(html, action as WorkflowActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup);
						dropdownActionLinks.Add(workflowHtml);
					}
					else
					{
						var workflowHtml = FormActionLinkListItem(html, action as WorkflowActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup);
						buttonActionLinks.Add(workflowHtml);
					}
				}

				else if (action is SubmitActionLink && action.Enabled && (action.ActionButtonAlignment ?? ActionButtonAlignment.Left) == actionButtonAlignment)
				{
					if ((action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup) == ActionButtonStyle.DropDown)
					{
						var submitHtml = FormActionLinkListItem(html, action as SubmitActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup,
							formConfiguration.AutoGenerateSteps, submitButtonID, submitButtonCommandName, validationGroup,
							submitButtonBusyText, formConfiguration.ActionNavbarCssClass);
						dropdownActionLinks.Add(submitHtml);
					}
					else
					{
						var submitHtml = FormActionLinkListItem(html, action as SubmitActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup,
							formConfiguration.AutoGenerateSteps, submitButtonID, submitButtonCommandName, validationGroup,
							submitButtonBusyText, formConfiguration.ActionNavbarCssClass);
						buttonActionLinks.Add(submitHtml);
					}
				}

				else if (action is PreviousActionLink && action.Enabled && (action.ActionButtonAlignment ?? ActionButtonAlignment.Left) == actionButtonAlignment)
				{
					if ((action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup) == ActionButtonStyle.DropDown)
					{
						var previousHtml = FormActionLinkListItem(html, action as PreviousActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup,
							formConfiguration.AutoGenerateSteps, null, null, validationGroup, null, formConfiguration.ActionNavbarCssClass,
							null, previousButtonId);
						dropdownActionLinks.Add(previousHtml);
					}
					else
					{
						var previousHtml = FormActionLinkListItem(html, action as PreviousActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup,
							formConfiguration.AutoGenerateSteps, null, null, validationGroup, null, formConfiguration.ActionNavbarCssClass,
							null, previousButtonId);
						buttonActionLinks.Add(previousHtml);
					}
				}

				else if (action is NextActionLink && action.Enabled && (action.ActionButtonAlignment ?? ActionButtonAlignment.Left) == actionButtonAlignment)
				{
					if ((action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup) == ActionButtonStyle.DropDown)
					{
						var nextHtml = FormActionLinkListItem(html, action as NextActionLink,
							action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup,
							formConfiguration.AutoGenerateSteps, null, null, validationGroup, null, formConfiguration.ActionNavbarCssClass,
							nextButtonId);
						dropdownActionLinks.Add(nextHtml);
					}
					else
					{
						var nextHtml = FormActionLinkListItem(html, action as NextActionLink, action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup, formConfiguration.AutoGenerateSteps, null, null, validationGroup, null, formConfiguration.ActionNavbarCssClass, nextButtonId);
						buttonActionLinks.Add(nextHtml);
					}
				}

				else if (action.Enabled && (action.ActionButtonAlignment ?? ActionButtonAlignment.Left) == actionButtonAlignment)
				{
					if ((action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup) == ActionButtonStyle.DropDown)
					{
						var actionHtml = FormActionLinkListItem(html, action, action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup);
						dropdownActionLinks.Add(actionHtml);
					}
					else
					{
						var actionHtml = FormActionLinkListItem(html, action, action.ActionButtonStyle ?? formConfiguration.ActionButtonStyle ?? ActionButtonStyle.ButtonGroup);
						buttonActionLinks.Add(actionHtml);
					}
				}
			}

			var buttonContainer = new WebControl(HtmlTextWriterTag.Div);

			buttonContainer.AddClass(actionButtonAlignment == ActionButtonAlignment.Left
				? "form-action-container-left"
				: "form-action-container-right");

			container.Controls.Add(buttonContainer);

			if (buttonActionLinks.Any())
			{
				foreach (var link in buttonActionLinks)
				{
					buttonContainer.Controls.Add(link);
				}
			}
			if (dropdownActionLinks.Any())
			{
				var dropDownMenu = FormActionDropDownMenuControl();

				foreach (var link in dropdownActionLinks) dropDownMenu.Controls.Add(link);

				var dropDown = FormActionDropDownControl(formConfiguration);

				dropDown.Controls.Add(dropDownMenu);
				dropDown.Controls.Add(new LiteralControl("</div>"));

				buttonContainer.Controls.Add(dropDown);
			}

			return container;
		}

		public static Control FormActionDropDownControl(IFormConfiguration formConfiguration)
		{
			var placeHolder = new PlaceHolder();

			placeHolder.Controls.Add(new LiteralControl("<div role=\"group\" class=\"btn-group\">"));

			var linkButton = new HtmlGenericControl("button");

			linkButton.AddClass("dropdown-toggle");
			linkButton.AddClass("btn");
			linkButton.AddClass("btn-default");
			linkButton.Attributes.Add("aria-haspopup", "true");
			linkButton.Attributes.Add("data-toggle", "dropdown");
			linkButton.Attributes.Add("role", "button");
			linkButton.Attributes.Add("aria-expanded", "false");
			linkButton.InnerHtml += !string.IsNullOrEmpty(formConfiguration.ActionButtonDropDownLabel) ? formConfiguration.ActionButtonDropDownLabel : "<span class='fa fa-bars fa-fw' aria-hidden='true'></span> Actions ";

			var buttonInner = new TagBuilder("span");

			buttonInner.AddCssClass("caret");

			linkButton.InnerHtml += buttonInner.ToString();

			placeHolder.Controls.Add(linkButton);

			return placeHolder;
		}

		public static WebControl ActionNavBarControl(ActionButtonAlignment? actionButtonAlignment)
		{
			var container = new WebControl(HtmlTextWriterTag.Div);

			container.AddClass("col-sm-6");
			container.AddClass("clearfix");

			return container;
		}

		public static WebControl FormActionNavbarInnerHtml(HtmlHelper html, WebControl containerLeft, WebControl containerRight, IFormConfiguration formConfiguration,
			ActionButtonPlacement actionButtonPlacement)
		{
			var collapsedNavbar = new Panel { ID = "form-navbar-collapse" + actionButtonPlacement.ToString() };

			collapsedNavbar.AddClass("collapse");
			collapsedNavbar.AddClass("navbar-collapse");

			var status = new HtmlGenericControl("p");
			status.AddClass("navbar-text");
			status.AddClass("pull-right");
			status.AddClass("action-status");
			status.AddClass("hidden");
			status.Attributes.Add("style", "margin-right:0;");
			var statusIcon = new TagBuilder("span");
			statusIcon.Attributes.Add("aria-hidden", "true");
			statusIcon.AddCssClass("fa fa-fw");
			status.InnerHtml += statusIcon.ToString();
			collapsedNavbar.Controls.Add(status);

			collapsedNavbar.Controls.Add(containerLeft);
			collapsedNavbar.Controls.Add(containerRight);

			var navbarHeader = new HtmlGenericControl("div");

			navbarHeader.AddClass("navbar-header");

			var collapseButton = new TagBuilder("button");
			collapseButton.AddCssClass("navbar-toggle");
			collapseButton.AddCssClass("collapsed");
			collapseButton.Attributes["type"] = "button";
			collapseButton.Attributes["data-toggle"] = "collapse";
			collapseButton.Attributes["data-target"] = "#form-navbar-collapse" + actionButtonPlacement.ToString();
			var srOnly = new TagBuilder("span");
			srOnly.AddCssClass("sr-only");
			collapseButton.InnerHtml += srOnly;
			var collapseButtonBar = new TagBuilder("span");
			collapseButtonBar.AddCssClass("icon-bar");
			collapseButton.InnerHtml += collapseButtonBar;
			collapseButton.InnerHtml += collapseButtonBar;
			collapseButton.InnerHtml += collapseButtonBar;

			navbarHeader.InnerHtml += collapseButton.ToString();

			var containerFluid = new Panel();

			containerFluid.AddClass("container-fluid");

			containerFluid.Controls.Add(navbarHeader);

			containerFluid.Controls.Add(collapsedNavbar);

			return containerFluid;
		}

		public static WebControl FormActionNavbarContainerControl(FormConfiguration formConfiguration)
		{
			var navBar = new Panel();

			navBar.AddClass("row");

			navBar.AddClass("form-custom-actions");

			return navBar;
		}

		public static void AddActionModalWindows(HtmlHelper html, FormConfiguration formConfiguration, WebControl container,
			ActionButtonPlacement actionButtonPlacement)
		{
			var deleteActionLink = formConfiguration.DeleteActionLink;
			var closeCaseActionLink = formConfiguration.CloseIncidentActionLink;
			var resolveCaseActionLink = formConfiguration.ResolveCaseActionLink;
			var reopenCaseActionLink = formConfiguration.ReopenCaseActionLink;
			var cancelCaseActionLink = formConfiguration.CancelCaseActionLink;
			var qualifyLeadActionLink = formConfiguration.QualifyLeadActionLink;
			var convertQuoteActionLink = formConfiguration.ConvertQuoteToOrderActionLink;
			var convertOrderActionLink = formConfiguration.ConvertOrderToInvoiceActionLink;
			var calculateOpportunityActionLink = formConfiguration.CalculateOpportunityActionLink;
			var deactivateActionLink = formConfiguration.DeactivateActionLink;
			var activateActionLink = formConfiguration.ActivateActionLink;
			var activateQuoteActionLink = formConfiguration.ActivateQuoteActionLink;
			var opportunityOnHoldActionLink = formConfiguration.SetOpportunityOnHoldActionLink;
			var reopenOpportunityActionLink = formConfiguration.ReopenOpportunityActionLink;
			var winOpportunityActionLink = formConfiguration.WinOpportunityActionLink;
			var loseOpportunityActionLink = formConfiguration.LoseOpportunityActionLink;
			var generateQuoteFromOpportunityActionLink = formConfiguration.GenerateQuoteFromOpportunityActionLink;
			var createRelatedRecordActionLink = formConfiguration.CreateRelatedRecordActionLink;

			DisassociateActionLink disassociateAction = null;

			var actionLinks = (actionButtonPlacement == ActionButtonPlacement.AboveForm)
				? formConfiguration.TopFormActionLinks
				: formConfiguration.BottomFormActionLinks;

			WorkflowActionLink firstWorkflow = actionLinks.OfType<WorkflowActionLink>().Select(action => action).FirstOrDefault();

			if (deleteActionLink != null && deleteActionLink.Enabled)
			{
				container.Controls.Add(new LiteralControl(html.DeleteModal(deleteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
					deleteActionLink.Modal.CssClass, deleteActionLink.Modal.Title.GetValueOrDefault(DefaultModalDeleteTitle),
					deleteActionLink.Confirmation.GetValueOrDefault(DefaultModalDeleteBody),
					deleteActionLink.Modal.DismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText),
					deleteActionLink.Modal.PrimaryButtonText.GetValueOrDefault(DefaultModalDeletePrimaryButtonText),
					deleteActionLink.Modal.CloseButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText),
					deleteActionLink.Modal.TitleCssClass, deleteActionLink.Modal.PrimaryButtonCssClass,
					deleteActionLink.Modal.CloseButtonCssClass).ToString())); 
			}

			if (qualifyLeadActionLink != null && qualifyLeadActionLink.Enabled && (qualifyLeadActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.QualifyLeadModal(qualifyLeadActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						qualifyLeadActionLink.Modal.CssClass, qualifyLeadActionLink.Modal.Title, qualifyLeadActionLink.Confirmation,
						qualifyLeadActionLink.Modal.DismissButtonSrText, qualifyLeadActionLink.Modal.PrimaryButtonText,
						qualifyLeadActionLink.Modal.CloseButtonText, qualifyLeadActionLink.Modal.TitleCssClass,
						qualifyLeadActionLink.Modal.PrimaryButtonCssClass, qualifyLeadActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (closeCaseActionLink != null && closeCaseActionLink.Enabled && (closeCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.CloseCaseModal(closeCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						closeCaseActionLink.Modal.CssClass, closeCaseActionLink.Modal.Title, closeCaseActionLink.Confirmation,
						null, null, closeCaseActionLink.Modal.DismissButtonSrText, closeCaseActionLink.Modal.PrimaryButtonText,
						closeCaseActionLink.Modal.CloseButtonText, closeCaseActionLink.Modal.TitleCssClass,
						closeCaseActionLink.Modal.PrimaryButtonCssClass, closeCaseActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (resolveCaseActionLink != null && resolveCaseActionLink.Enabled)
			{
				container.Controls.Add(new LiteralControl(html.ResolveCaseModal(resolveCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						resolveCaseActionLink.Modal.CssClass, resolveCaseActionLink.Modal.Title, resolveCaseActionLink.Confirmation,
						resolveCaseActionLink.SubjectLabel, resolveCaseActionLink.DescriptionLabel,
						resolveCaseActionLink.Modal.DismissButtonSrText, resolveCaseActionLink.Modal.PrimaryButtonText,
						resolveCaseActionLink.Modal.CloseButtonText, resolveCaseActionLink.Modal.TitleCssClass,
						resolveCaseActionLink.Modal.PrimaryButtonCssClass, resolveCaseActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (reopenCaseActionLink != null && reopenCaseActionLink.Enabled && (reopenCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ReopenCaseModal(reopenCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenCaseActionLink.Modal.CssClass, reopenCaseActionLink.Modal.Title, reopenCaseActionLink.Confirmation,
						null, null, reopenCaseActionLink.Modal.DismissButtonSrText, reopenCaseActionLink.Modal.PrimaryButtonText,
						reopenCaseActionLink.Modal.CloseButtonText, reopenCaseActionLink.Modal.TitleCssClass,
						reopenCaseActionLink.Modal.PrimaryButtonCssClass, reopenCaseActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (cancelCaseActionLink != null && cancelCaseActionLink.Enabled && (cancelCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.CancelCaseModal(cancelCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						cancelCaseActionLink.Modal.CssClass, cancelCaseActionLink.Modal.Title, cancelCaseActionLink.Confirmation,
						null, null, cancelCaseActionLink.Modal.DismissButtonSrText, cancelCaseActionLink.Modal.PrimaryButtonText,
						cancelCaseActionLink.Modal.CloseButtonText, cancelCaseActionLink.Modal.TitleCssClass,
						cancelCaseActionLink.Modal.PrimaryButtonCssClass, cancelCaseActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (convertQuoteActionLink != null && convertQuoteActionLink.Enabled && (convertQuoteActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ConvertQuoteModal(convertQuoteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertQuoteActionLink.Modal.CssClass, convertQuoteActionLink.Modal.Title, convertQuoteActionLink.Confirmation,
						convertQuoteActionLink.Modal.DismissButtonSrText, convertQuoteActionLink.Modal.PrimaryButtonText,
						convertQuoteActionLink.Modal.CloseButtonText, convertQuoteActionLink.Modal.TitleCssClass,
						convertQuoteActionLink.Modal.PrimaryButtonCssClass, convertQuoteActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (convertOrderActionLink != null && convertOrderActionLink.Enabled && (convertOrderActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ConvertOrderModal(closeCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertOrderActionLink.Modal.CssClass, convertOrderActionLink.Modal.Title, convertOrderActionLink.Confirmation,
						convertOrderActionLink.Modal.DismissButtonSrText, convertOrderActionLink.Modal.PrimaryButtonText,
						convertOrderActionLink.Modal.CloseButtonText, convertOrderActionLink.Modal.TitleCssClass,
						convertOrderActionLink.Modal.PrimaryButtonCssClass, convertOrderActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (calculateOpportunityActionLink != null && calculateOpportunityActionLink.Enabled && (calculateOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.CalculateOpportunityModal(
						calculateOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						calculateOpportunityActionLink.Modal.CssClass, calculateOpportunityActionLink.Modal.Title,
						calculateOpportunityActionLink.Confirmation, calculateOpportunityActionLink.Modal.DismissButtonSrText,
						calculateOpportunityActionLink.Modal.PrimaryButtonText, calculateOpportunityActionLink.Modal.CloseButtonText,
						calculateOpportunityActionLink.Modal.TitleCssClass, calculateOpportunityActionLink.Modal.PrimaryButtonCssClass,
						calculateOpportunityActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (deactivateActionLink != null && deactivateActionLink.Enabled && (deactivateActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.DeactivateModal(deactivateActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						deactivateActionLink.Modal.CssClass, deactivateActionLink.Modal.Title, deactivateActionLink.Confirmation,
						deactivateActionLink.Modal.DismissButtonSrText, deactivateActionLink.Modal.PrimaryButtonText,
						deactivateActionLink.Modal.CloseButtonText,
						deactivateActionLink.Modal.TitleCssClass, deactivateActionLink.Modal.PrimaryButtonCssClass,
						deactivateActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (activateActionLink != null && activateActionLink.Enabled && (activateActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ActivateModal(activateActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateActionLink.Modal.CssClass, activateActionLink.Modal.Title, activateActionLink.Confirmation,
						activateActionLink.Modal.DismissButtonSrText, activateActionLink.Modal.PrimaryButtonText,
						activateActionLink.Modal.CloseButtonText,
						activateActionLink.Modal.TitleCssClass, activateActionLink.Modal.PrimaryButtonCssClass,
						activateActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (activateQuoteActionLink != null && activateQuoteActionLink.Enabled && (activateQuoteActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ActivateQuoteModal(activateQuoteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateQuoteActionLink.Modal.CssClass, activateQuoteActionLink.Modal.Title, activateQuoteActionLink.Confirmation,
						activateQuoteActionLink.Modal.DismissButtonSrText, activateQuoteActionLink.Modal.PrimaryButtonText,
						activateQuoteActionLink.Modal.CloseButtonText,
						activateQuoteActionLink.Modal.TitleCssClass, activateQuoteActionLink.Modal.PrimaryButtonCssClass,
						activateQuoteActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (opportunityOnHoldActionLink != null && opportunityOnHoldActionLink.Enabled && (opportunityOnHoldActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.SetOpportunityOnHoldModal(
						opportunityOnHoldActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						opportunityOnHoldActionLink.Modal.CssClass, opportunityOnHoldActionLink.Modal.Title,
						opportunityOnHoldActionLink.Confirmation, opportunityOnHoldActionLink.Modal.DismissButtonSrText,
						opportunityOnHoldActionLink.Modal.PrimaryButtonText, opportunityOnHoldActionLink.Modal.CloseButtonText,
						opportunityOnHoldActionLink.Modal.TitleCssClass, opportunityOnHoldActionLink.Modal.PrimaryButtonCssClass,
						opportunityOnHoldActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (reopenOpportunityActionLink != null && reopenOpportunityActionLink.Enabled && (reopenOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.ReopenOpportunityModal(reopenOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenOpportunityActionLink.Modal.CssClass, reopenOpportunityActionLink.Modal.Title,
						reopenOpportunityActionLink.Confirmation, reopenOpportunityActionLink.Modal.DismissButtonSrText,
						reopenOpportunityActionLink.Modal.PrimaryButtonText, reopenOpportunityActionLink.Modal.CloseButtonText,
						reopenOpportunityActionLink.Modal.TitleCssClass, reopenOpportunityActionLink.Modal.PrimaryButtonCssClass,
						reopenOpportunityActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (winOpportunityActionLink != null && winOpportunityActionLink.Enabled && (winOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.WinOpportunityModal(winOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						winOpportunityActionLink.Modal.CssClass, winOpportunityActionLink.Modal.Title, winOpportunityActionLink.Confirmation,
						winOpportunityActionLink.Modal.DismissButtonSrText, winOpportunityActionLink.Modal.PrimaryButtonText,
						winOpportunityActionLink.Modal.CloseButtonText,
						winOpportunityActionLink.Modal.TitleCssClass, winOpportunityActionLink.Modal.PrimaryButtonCssClass,
						winOpportunityActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (loseOpportunityActionLink != null && loseOpportunityActionLink.Enabled && (loseOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.LoseOpportunityModal(loseOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						loseOpportunityActionLink.Modal.CssClass, loseOpportunityActionLink.Modal.Title,
						loseOpportunityActionLink.Confirmation, loseOpportunityActionLink.Modal.DismissButtonSrText,
						loseOpportunityActionLink.Modal.PrimaryButtonText, loseOpportunityActionLink.Modal.CloseButtonText,
						loseOpportunityActionLink.Modal.TitleCssClass, loseOpportunityActionLink.Modal.PrimaryButtonCssClass,
						loseOpportunityActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (generateQuoteFromOpportunityActionLink != null && generateQuoteFromOpportunityActionLink.Enabled &&
				(generateQuoteFromOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.Controls.Add(new LiteralControl(html.GenerateQuoteFromOpportunityModal(
						generateQuoteFromOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						generateQuoteFromOpportunityActionLink.Modal.CssClass, generateQuoteFromOpportunityActionLink.Modal.Title,
						generateQuoteFromOpportunityActionLink.Confirmation,
						generateQuoteFromOpportunityActionLink.Modal.DismissButtonSrText,
						generateQuoteFromOpportunityActionLink.Modal.PrimaryButtonText,
						generateQuoteFromOpportunityActionLink.Modal.CloseButtonText,
						generateQuoteFromOpportunityActionLink.Modal.TitleCssClass,
						generateQuoteFromOpportunityActionLink.Modal.PrimaryButtonCssClass,
						generateQuoteFromOpportunityActionLink.Modal.CloseButtonCssClass).ToString()));
			}

			if (firstWorkflow != null && firstWorkflow.Enabled)
			{
				container.Controls.Add(new LiteralControl(html.WorkflowModal(firstWorkflow.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
					firstWorkflow.Modal.CssClass, firstWorkflow.Modal.Title, firstWorkflow.Confirmation,
					firstWorkflow.Modal.DismissButtonSrText, firstWorkflow.Modal.PrimaryButtonText, firstWorkflow.Modal.CloseButtonText,
					firstWorkflow.Modal.TitleCssClass, firstWorkflow.Modal.PrimaryButtonCssClass, firstWorkflow.Modal.CloseButtonCssClass).ToString()));
			}

			if (disassociateAction != null && disassociateAction.Enabled)
			{
				container.Controls.Add(new LiteralControl(html.DissassociateModal(disassociateAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						disassociateAction.Modal.CssClass, disassociateAction.Modal.Title, disassociateAction.Confirmation,
						disassociateAction.Modal.DismissButtonSrText, disassociateAction.Modal.PrimaryButtonText,
						disassociateAction.Modal.CloseButtonText, disassociateAction.Modal.TitleCssClass,
						disassociateAction.Modal.PrimaryButtonCssClass, disassociateAction.Modal.CloseButtonCssClass).ToString()));
			}

			var createRelatedRecordActionLinks = actionLinks.OfType<CreateRelatedRecordActionLink>().Select(action => action);

			foreach (var createRelatedRecordAction in createRelatedRecordActionLinks)
			{
				var htmlAttributes = new Dictionary<string, string>
				{
					{ "data-filtercriteriaid", createRelatedRecordAction.FilterCriteriaId.ToString() }
				};


				if (createRelatedRecordAction.ShowModal != ShowModal.Yes)
				{
					container.Controls.Add(
						new LiteralControl(
							html.CreateRelatedRecordModal(
								createRelatedRecordAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
								createRelatedRecordAction.Modal.CssClass, createRelatedRecordAction.Modal.Title,
								createRelatedRecordAction.Confirmation,
								createRelatedRecordAction.Modal.DismissButtonSrText, createRelatedRecordAction.Modal.PrimaryButtonText,
								createRelatedRecordAction.Modal.CloseButtonText,
								createRelatedRecordAction.Modal.TitleCssClass, createRelatedRecordAction.Modal.PrimaryButtonCssClass,
								createRelatedRecordAction.Modal.CloseButtonCssClass, htmlAttributes).ToString()));
				}
				else
				{
					container.Controls.Add(
						new LiteralControl(html.CreateRelatedRecordModal(BootstrapExtensions.BootstrapModalSize.Large,
						createRelatedRecordAction.Modal.CssClass, createRelatedRecordAction.Modal.Title,
						createRelatedRecordAction.Modal.DismissButtonSrText, createRelatedRecordAction.Modal.TitleCssClass,
						null, htmlAttributes, null).ToString()));
				}
			}
		}
	}
}
