/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.CrmEntityFormView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	public static class FormActionExtensions
	{
		private const string DefaultModalDeleteTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> Delete";
		private const string DefaultModalDeleteBody = "<p>Are you sure you want to delete this record?</p>";
		private const string DefaultModalDeletePrimaryButtonText = "Delete";
		private const string DefaultModalDeleteCancelButtonText = "Cancel";
		private const string DefaultModalDismissButtonSrText = "Close";

		public static IHtmlString FormActionLinkListItem(this HtmlHelper html, ViewActionLink viewActionLink, ActionButtonStyle actionButtonStyle)
		{
			var listItem = new TagBuilder("li");

			listItem.Attributes["role"] = "group";

			var link = html.FormActionLink(viewActionLink, actionButtonStyle);

			listItem.InnerHtml += link.ToString();

			return new HtmlString(listItem.ToString());
		}

		public static IHtmlString FormActionLink(this HtmlHelper html, ViewActionLink viewActionLink, ActionButtonStyle actionButtonStyle)
		{
			TagBuilder link;

			if (actionButtonStyle == ActionButtonStyle.DropDown)
			{
				link = new TagBuilder("a");

				link.Attributes["role"] = "menuitem";
				link.Attributes["tabindex"] = "-1";
				link.Attributes["title"] = string.Empty;

				if (!string.IsNullOrEmpty(viewActionLink.ButtonCssClass)) link.AddCssClass(viewActionLink.ButtonCssClass);
			}
			else if (actionButtonStyle == ActionButtonStyle.ButtonGroup)
			{
				link = new TagBuilder("button");

				link.Attributes["type"] = "button";

				link.AddCssClass("btn");

				link.AddCssClass(!string.IsNullOrEmpty(viewActionLink.ButtonCssClass) ? viewActionLink.ButtonCssClass : "btn-default");
			}
			else
			{
				throw new InvalidDataException("ActionButtonStyle must be one of ButtonGroup or DropDown");
			}

			link.InnerHtml += viewActionLink.Label;

			switch (viewActionLink.Type)
			{
				case LinkActionType.Activate:
					link.AddCssClass("activate-link"); 
					break;
				case LinkActionType.ActivateQuote:
					link.AddCssClass("activate-quote-link"); 
					break;
				case LinkActionType.CalculateOpportunity:
					link.AddCssClass("calculate-opportunity-link");
					break;
				case LinkActionType.CancelCase:
					link.AddCssClass("cancel-case-link");
					break;
				case LinkActionType.CloseIncident:
					link.AddCssClass("close-case-link");

					var closeCaseActionLink = viewActionLink as CloseIncidentActionLink;

					link.MergeAttribute("data-resolution", closeCaseActionLink.DefaultResolution);
					link.MergeAttribute("data-description", closeCaseActionLink.DefaultResolutionDescription);

					break;
				case LinkActionType.ConvertOrder:
					link.AddCssClass("convert-order-link");
					break;
				case LinkActionType.ConvertQuote:
					link.AddCssClass("convert-quote-link");
					break;
				case LinkActionType.Deactivate:
					link.AddCssClass("deactivate-link");
					break;
				case LinkActionType.Delete:
					link.AddCssClass("delete-link");
					break;
				case LinkActionType.GenerateQuoteFromOpportunity:
					link.AddCssClass("generate-quote-from-opportunity-link");
					break;
				case LinkActionType.LoseOpportunity:
					link.AddCssClass("lose-opportunity-link");
					break;
				case LinkActionType.QualifyLead:
					link.AddCssClass("qualify-lead-link");
					break;
				case LinkActionType.ReopenCase:
					link.AddCssClass("reopen-case-link");
					break;
				case LinkActionType.ResolveCase:
					link.AddCssClass("resolve-case-link");
					break;
				case LinkActionType.SetOpportunityOnHold:
					link.AddCssClass("set-opportunity-on-hold-link");
					break;
				case LinkActionType.ReopenOpportunity:
					link.AddCssClass("reopen-opportunity-link");
					break;
				case LinkActionType.UpdatePipelinePhase:
					link.AddCssClass("update-pipeline-phase-link");
					break;
				case LinkActionType.WinOpportunity:
					link.AddCssClass("win-opportunity-link");
					break;
				case LinkActionType.Workflow:
					link.AddCssClass("workflow-link");

					var workflowActionLink = viewActionLink as WorkflowActionLink;

					var id = workflowActionLink.Workflow.Id;

					link.MergeAttribute("data-workflowid", id.ToString());
					if (workflowActionLink.CustomizeModal)
					{
						link.MergeAttribute("data-modal-confirmation", workflowActionLink.Confirmation);
						link.MergeAttribute("data-modal-title", workflowActionLink.Modal.Title);
						link.MergeAttribute("data-modal-primary-action", workflowActionLink.Modal.PrimaryButtonText);
						link.MergeAttribute("data-modal-cancel-action", workflowActionLink.Modal.CloseButtonText);
					}
					break;
				case LinkActionType.CreateRelatedRecord:
					link.AddCssClass("create-related-record-link");
					link.MergeAttribute("data-filtercriteriaid", viewActionLink.FilterCriteriaId.ToString());
					break;
				default:
					throw new Exception("LinkActionType is not a valid value");
			}

			link.Attributes["href"] = viewActionLink.URL != null ? viewActionLink.URL.PathWithQueryString : null;

			link.MergeAttribute("data-url", viewActionLink.URL != null ? viewActionLink.URL.PathWithQueryString : null);

			return new HtmlString(link.ToString());
		}

		public static TagBuilder FormActionDropDownMenu(this HtmlHelper html)
		{
			var dropDownMenu = new TagBuilder("ul");

			dropDownMenu.AddCssClass("dropdown-menu");
			dropDownMenu.Attributes["role"] = "menu";

			return dropDownMenu;
		}

		public static IHtmlString FormActionDefaultNavBar(this HtmlHelper html, FormConfiguration formConfiguration, TagBuilder container)
		{
			var dropDown = html.FormActions(formConfiguration, container);

			var containerFluid = html.FormActionNavbarInnerHtml(dropDown, formConfiguration);

			var navBar = FormActionNavbar(formConfiguration);

			navBar.InnerHtml += containerFluid;

			return new HtmlString(navBar.ToString());
		}

		public static IHtmlString FormActionNoNavBar(this HtmlHelper html, FormConfiguration formConfiguration, TagBuilder container)
		{
			var dropDown = html.FormActions(formConfiguration, container);

			//var containerFluid = html.FormActionNavbarInnerHtml(dropDown, formConfiguration);

			var navBar = FormActionNavbar(formConfiguration);

			navBar.InnerHtml += dropDown;

			return new HtmlString(navBar.ToString());
		}

		
		public static TagBuilder FormActions(this HtmlHelper html, FormConfiguration formConfiguration, TagBuilder container)
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
			var updatePipelinePhaseAction = formConfiguration.UpdatePipelinePhaseActionLink;
			var createRelatedRecordAction = formConfiguration.CreateRelatedRecordActionLink;

			DisassociateActionLink disassociateAction = null;
			WorkflowActionLink firstWorkflow = null;


			List<string> actionLinks = new List<string> { };

			foreach (var action in formConfiguration.TopFormActionLinks)
			{ 

				if (action is DeleteActionLink && deleteActionLink.Enabled)
				{
					var deleteHtml = html.FormActionLinkListItem(deleteActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(deleteHtml.ToString());
				}

				if (action is CloseIncidentActionLink && closeCaseActionLink.Enabled)
				{
					var closeCaseHtml = html.FormActionLinkListItem(closeCaseActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(closeCaseHtml.ToString());
				}

				if (action is ResolveCaseActionLink && resolveCaseActionLink.Enabled)
				{
					var resolveCaseHtml = html.FormActionLinkListItem(resolveCaseActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(resolveCaseHtml.ToString());
				}

				if (action is ReopenCaseActionLink && reopenCaseActionLink.Enabled)
				{
					var reopenCaseHtml = html.FormActionLinkListItem(reopenCaseActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(reopenCaseHtml.ToString());
				}

				if (action is CancelCaseActionLink && cancelCaseActionLink.Enabled)
				{
					var cancelCaseHtml = html.FormActionLinkListItem(cancelCaseActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(cancelCaseHtml.ToString());
				}

				if (action is QualifyLeadActionLink && qualifyLeadActionLink.Enabled)
				{
					var qualifyLeadHtml = html.FormActionLinkListItem(qualifyLeadActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(qualifyLeadHtml.ToString());
				}

				if (action is ConvertQuoteToOrderActionLink && convertQuoteActionLink.Enabled)
				{
					var convertQuoteActionHtml = html.FormActionLinkListItem(convertQuoteActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(convertQuoteActionHtml.ToString());
				}

				if (action is ConvertOrderToInvoiceActionLink && convertOrderActionLink.Enabled)
				{
					var convertOrderHtml = html.FormActionLinkListItem(convertOrderActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(convertOrderHtml.ToString());
				}

				if (action is CalculateOpportunityActionLink && calculateOpportunityActionLink.Enabled)
				{
					var calculateOpportunityHtml = html.FormActionLinkListItem(calculateOpportunityActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(calculateOpportunityHtml.ToString());
				}

				if (action is DeactivateActionLink && deactivateActionLink.Enabled)
				{
					var deactivateHtml = html.FormActionLinkListItem(deactivateActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(deactivateHtml.ToString());
				}

				if (action is ActivateActionLink && activateActionLink.Enabled)
				{
					var activateHtml = html.FormActionLinkListItem(activateActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(activateHtml.ToString());
				}

				if (action is ActivateQuoteActionLink && activateQuoteActionLink.Enabled)
				{
					var activateQuoteHtml = html.FormActionLinkListItem(activateQuoteActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(activateQuoteHtml.ToString());
				}

				if (action is SetOpportunityOnHoldActionLink && opportunityOnHoldActionLink.Enabled)
				{
					var setOpportunityOnHoldHtml = html.FormActionLinkListItem(opportunityOnHoldActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(setOpportunityOnHoldHtml.ToString());
				}

				if (action is ReopenOpportunityActionLink && reopenOpportunityActionLink.Enabled)
				{
					var reopenOpportunityHtml = html.FormActionLinkListItem(reopenOpportunityActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(reopenOpportunityHtml.ToString());
				}

				if (action is WinOpportunityActionLink && winOpportunityActionLink.Enabled)
				{
					var winOpportunityHtml = html.FormActionLinkListItem(winOpportunityActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(winOpportunityHtml.ToString());
				}

				if (action is LoseOpportunityActionLink && loseOpportunityActionLink.Enabled)
				{
					var resolveCaseHtml = html.FormActionLinkListItem(loseOpportunityActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(resolveCaseHtml.ToString());
				}

				if (action is GenerateQuoteFromOpportunityActionLink && generateQuoteFromOpportunityActionLink.Enabled)
				{
					var generateQuoteFromOpportunityHtml = html.FormActionLinkListItem(generateQuoteFromOpportunityActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(generateQuoteFromOpportunityHtml.ToString());
				}

				if (action is UpdatePipelinePhaseActionLink && updatePipelinePhaseAction.Enabled)
				{
					var updatePipelinePhaseHtml = html.FormActionLinkListItem(updatePipelinePhaseAction, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(updatePipelinePhaseHtml.ToString());
				}

				if (action is WorkflowActionLink && action.Enabled)
				{
					var workflowHtml = html.FormActionLinkListItem(action as WorkflowActionLink, formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(workflowHtml.ToString());

					firstWorkflow = action as WorkflowActionLink;
				}

				if (action is DisassociateActionLink && action.Enabled)
				{
					disassociateAction = action as DisassociateActionLink;
					var disassociateHtml = html.FormActionLinkListItem(disassociateAction,
						formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(disassociateHtml.ToString());
				}

				if (action is CreateRelatedRecordActionLink && action.Enabled)
				{
					var createRelatedRecordHtml = html.FormActionLinkListItem(createRelatedRecordAction,
						formConfiguration.ActionButtonStyle ?? ActionButtonStyle.DropDown);
					actionLinks.Add(createRelatedRecordHtml.ToString());
				}
			}

			if (formConfiguration.ActionButtonStyle == ActionButtonStyle.ButtonGroup)
			{
				foreach (var link in actionLinks)
				{
					container.InnerHtml += link;
				}
			}
			else //if (formConfiguration.ActionButtonStyle == ActionButtonStyle.DropDown)
			{
				//Create the links  as a dropdownmenu 
				var dropDownMenu = html.FormActionDropDownMenu();

				foreach (var link in actionLinks)
				{
					dropDownMenu.InnerHtml += link;
				}

				//the meat of the dropdown
				var dropDown = html.FormActionDropDown(formConfiguration);

				//add the items to the dropdown
				dropDown.InnerHtml += dropDownMenu.ToString();

				//add the dropdown to the container
				container.InnerHtml += dropDown;
			}

			html.ActionModalWindows(container, deleteActionLink, qualifyLeadActionLink, closeCaseActionLink, resolveCaseActionLink, reopenCaseActionLink, 
				cancelCaseActionLink, convertQuoteActionLink, convertOrderActionLink, calculateOpportunityActionLink, deactivateActionLink, 
				activateActionLink, activateQuoteActionLink, opportunityOnHoldActionLink, reopenOpportunityActionLink, winOpportunityActionLink, 
				loseOpportunityActionLink, generateQuoteFromOpportunityActionLink, updatePipelinePhaseAction, firstWorkflow, disassociateAction, createRelatedRecordAction);

			return container;
		}

		public static TagBuilder FormActionDropDown(this HtmlHelper html, IFormConfiguration formConfiguration)
		{
			var dropDown = new TagBuilder("li");

			dropDown.AddCssClass("dropdown");
			dropDown.AddCssClass("action");

			var linkButton = new TagBuilder("a");

			linkButton.AddCssClass("dropdown-toggle");

			linkButton.Attributes["href"] = "#";
			linkButton.Attributes["data-toggle"] = "dropdown";
			linkButton.Attributes["role"] = "button";
			linkButton.Attributes["aria-expanded"] = "false";

			linkButton.InnerHtml += !string.IsNullOrEmpty(formConfiguration.ActionButtonDropDownLabel) ? formConfiguration.ActionButtonDropDownLabel : "<span class='fa fa-bars fa-fw' aria-hidden='true'></span> Actions";

			var buttonInner = new TagBuilder("span");

			buttonInner.AddCssClass("caret");

			linkButton.InnerHtml += buttonInner.ToString();

			dropDown.InnerHtml += linkButton.ToString();

			return dropDown;
		}

		public static TagBuilder FormActionNavbarInnerHtml(this HtmlHelper html, TagBuilder container, IFormConfiguration formConfiguration)
		{
			var collapsedNavbar = new TagBuilder("div");

			collapsedNavbar.GenerateId("form-navbar-collapse");

			collapsedNavbar.AddCssClass("collapse");
			collapsedNavbar.AddCssClass("navbar-collapse");

			var status = new TagBuilder("p");
			status.AddCssClass("navbar-text");
			status.AddCssClass("pull-right");
			status.AddCssClass("action-status");
			status.Attributes.Add("style", "margin-right:0;");
			var statusIcon = new TagBuilder("span");
			statusIcon.Attributes.Add("aria-hidden", "true");
			statusIcon.AddCssClass("fa fa-fw");
			status.InnerHtml += statusIcon;
			collapsedNavbar.InnerHtml += status;

			collapsedNavbar.InnerHtml += container;

			var navbarHeader = new TagBuilder("div");

			navbarHeader.AddCssClass("navbar-header");

			var collapseButton = new TagBuilder("button");
			collapseButton.AddCssClass("navbar-toggle");
			collapseButton.AddCssClass("collapsed");
			collapseButton.Attributes["type"] = "button";
			collapseButton.Attributes["data-toggle"] = "collapse";
			collapseButton.Attributes["data-target"] = "#form-navbar-collapse";
			var srOnly = new TagBuilder("span");
			srOnly.AddCssClass("sr-only");
			collapseButton.InnerHtml += srOnly;
			var collapseButtonBar = new TagBuilder("span");
			collapseButtonBar.AddCssClass("icon-bar");
			collapseButton.InnerHtml += collapseButtonBar;
			collapseButton.InnerHtml += collapseButtonBar;
			collapseButton.InnerHtml += collapseButtonBar;

			navbarHeader.InnerHtml += collapseButton;

			var containerFluid = new TagBuilder("div");

			containerFluid.AddCssClass("container-fluid");

			containerFluid.InnerHtml += navbarHeader;

			containerFluid.InnerHtml += collapsedNavbar;

			return containerFluid;
		}

		private static TagBuilder FormActionNavbar(IFormConfiguration formConfiguration)
		{
			var navBar = new TagBuilder("div");

			navBar.AddCssClass("navbar");

			if (formConfiguration.ActionNavbarCssClass.Contains("navbar-default"))
			{
				//navBar.AddCssClass("navbar-default");
				navBar.Attributes["style"] = "display: none;";
			}

			navBar.AddCssClass(!string.IsNullOrEmpty(formConfiguration.ActionNavbarCssClass)
				? formConfiguration.ActionNavbarCssClass
				: "Actions");
			return navBar;
		}

		public static void ActionModalWindows(this HtmlHelper html, TagBuilder container, DeleteActionLink deleteActionLink,
			QualifyLeadActionLink qualifyLeadActionLink, CloseIncidentActionLink closeCaseActionLink,
			ResolveCaseActionLink resolveCaseActionLink, ReopenCaseActionLink reopenCaseActionLink,
			CancelCaseActionLink cancelCaseActionLink, ConvertQuoteToOrderActionLink convertQuoteActionLink,
			ConvertOrderToInvoiceActionLink convertOrderActionLink, CalculateOpportunityActionLink calculateOpportunityActionLink,
			DeactivateActionLink deactivateActionLink, ActivateActionLink activateActionLink,
			ActivateQuoteActionLink activateQuoteActionLink, SetOpportunityOnHoldActionLink opportunityOnHoldActionLink,
			ReopenOpportunityActionLink reopenOpportunityActionLink, WinOpportunityActionLink winOpportunityActionLink,
			LoseOpportunityActionLink loseOpportunityActionLink,
			GenerateQuoteFromOpportunityActionLink generateQuoteFromOpportunityActionLink,
			UpdatePipelinePhaseActionLink updatePipelinePhaseAction, WorkflowActionLink firstWorkflow,
			DisassociateActionLink disassociateAction, CreateRelatedRecordActionLink createRelatedRecordAction)
		{
			if (deleteActionLink.Enabled)
			{
				container.InnerHtml += html.DeleteModal(deleteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
					deleteActionLink.Modal.CssClass, deleteActionLink.Modal.Title.GetValueOrDefault(DefaultModalDeleteTitle),
					deleteActionLink.Confirmation.GetValueOrDefault(DefaultModalDeleteBody),
					deleteActionLink.Modal.DismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText),
					deleteActionLink.Modal.PrimaryButtonText.GetValueOrDefault(DefaultModalDeletePrimaryButtonText),
					deleteActionLink.Modal.CloseButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText),
					deleteActionLink.Modal.TitleCssClass, deleteActionLink.Modal.PrimaryButtonCssClass,
					deleteActionLink.Modal.CloseButtonCssClass);
			}

			if (qualifyLeadActionLink.Enabled && (qualifyLeadActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.QualifyLeadModal(qualifyLeadActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						qualifyLeadActionLink.Modal.CssClass, qualifyLeadActionLink.Modal.Title, qualifyLeadActionLink.Confirmation,
						qualifyLeadActionLink.Modal.DismissButtonSrText, qualifyLeadActionLink.Modal.PrimaryButtonText,
						qualifyLeadActionLink.Modal.CloseButtonText, qualifyLeadActionLink.Modal.TitleCssClass,
						qualifyLeadActionLink.Modal.PrimaryButtonCssClass, qualifyLeadActionLink.Modal.CloseButtonCssClass);
			}

			if (closeCaseActionLink.Enabled && (closeCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.CloseCaseModal(closeCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						closeCaseActionLink.Modal.CssClass, closeCaseActionLink.Modal.Title, closeCaseActionLink.Confirmation,
						null, null, closeCaseActionLink.Modal.DismissButtonSrText, closeCaseActionLink.Modal.PrimaryButtonText,
						closeCaseActionLink.Modal.CloseButtonText, closeCaseActionLink.Modal.TitleCssClass,
						closeCaseActionLink.Modal.PrimaryButtonCssClass, closeCaseActionLink.Modal.CloseButtonCssClass);
			}

			if (resolveCaseActionLink.Enabled)
			{
				container.InnerHtml +=
					html.ResolveCaseModal(resolveCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						resolveCaseActionLink.Modal.CssClass, resolveCaseActionLink.Modal.Title, resolveCaseActionLink.Confirmation,
						resolveCaseActionLink.SubjectLabel, resolveCaseActionLink.DescriptionLabel,
						resolveCaseActionLink.Modal.DismissButtonSrText, resolveCaseActionLink.Modal.PrimaryButtonText,
						resolveCaseActionLink.Modal.CloseButtonText, resolveCaseActionLink.Modal.TitleCssClass,
						resolveCaseActionLink.Modal.PrimaryButtonCssClass, resolveCaseActionLink.Modal.CloseButtonCssClass);
			}

			if (reopenCaseActionLink.Enabled && (reopenCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ReopenCaseModal(reopenCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenCaseActionLink.Modal.CssClass, reopenCaseActionLink.Modal.Title, reopenCaseActionLink.Confirmation,
						null, null, reopenCaseActionLink.Modal.DismissButtonSrText, reopenCaseActionLink.Modal.PrimaryButtonText,
						reopenCaseActionLink.Modal.CloseButtonText, reopenCaseActionLink.Modal.TitleCssClass,
						reopenCaseActionLink.Modal.PrimaryButtonCssClass, reopenCaseActionLink.Modal.CloseButtonCssClass);
			}

			if (cancelCaseActionLink.Enabled && (cancelCaseActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.CancelCaseModal(cancelCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						cancelCaseActionLink.Modal.CssClass, cancelCaseActionLink.Modal.Title, cancelCaseActionLink.Confirmation,
						null, null, cancelCaseActionLink.Modal.DismissButtonSrText, cancelCaseActionLink.Modal.PrimaryButtonText,
						cancelCaseActionLink.Modal.CloseButtonText, cancelCaseActionLink.Modal.TitleCssClass,
						cancelCaseActionLink.Modal.PrimaryButtonCssClass, cancelCaseActionLink.Modal.CloseButtonCssClass);
			}

			if (convertQuoteActionLink.Enabled && (convertQuoteActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ConvertQuoteModal(convertQuoteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertQuoteActionLink.Modal.CssClass, convertQuoteActionLink.Modal.Title, convertQuoteActionLink.Confirmation,
						convertQuoteActionLink.Modal.DismissButtonSrText, convertQuoteActionLink.Modal.PrimaryButtonText,
						convertQuoteActionLink.Modal.CloseButtonText, convertQuoteActionLink.Modal.TitleCssClass,
						convertQuoteActionLink.Modal.PrimaryButtonCssClass, convertQuoteActionLink.Modal.CloseButtonCssClass);
			}

			if (convertOrderActionLink.Enabled && (convertOrderActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ConvertOrderModal(closeCaseActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertOrderActionLink.Modal.CssClass, convertOrderActionLink.Modal.Title, convertOrderActionLink.Confirmation,
						convertOrderActionLink.Modal.DismissButtonSrText, convertOrderActionLink.Modal.PrimaryButtonText,
						convertOrderActionLink.Modal.CloseButtonText, convertOrderActionLink.Modal.TitleCssClass,
						convertOrderActionLink.Modal.PrimaryButtonCssClass, convertOrderActionLink.Modal.CloseButtonCssClass);
			}

			if (calculateOpportunityActionLink.Enabled && (calculateOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.CalculateOpportunityModal(
						calculateOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						calculateOpportunityActionLink.Modal.CssClass, calculateOpportunityActionLink.Modal.Title,
						calculateOpportunityActionLink.Confirmation, calculateOpportunityActionLink.Modal.DismissButtonSrText,
						calculateOpportunityActionLink.Modal.PrimaryButtonText, calculateOpportunityActionLink.Modal.CloseButtonText,
						calculateOpportunityActionLink.Modal.TitleCssClass, calculateOpportunityActionLink.Modal.PrimaryButtonCssClass,
						calculateOpportunityActionLink.Modal.CloseButtonCssClass);
			}

			if (deactivateActionLink.Enabled && (deactivateActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.DeactivateModal(deactivateActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						deactivateActionLink.Modal.CssClass, deactivateActionLink.Modal.Title, deactivateActionLink.Confirmation,
						deactivateActionLink.Modal.DismissButtonSrText, deactivateActionLink.Modal.PrimaryButtonText,
						deactivateActionLink.Modal.CloseButtonText,
						deactivateActionLink.Modal.TitleCssClass, deactivateActionLink.Modal.PrimaryButtonCssClass,
						deactivateActionLink.Modal.CloseButtonCssClass);
			}

			if (activateActionLink.Enabled && (activateActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ActivateModal(activateActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateActionLink.Modal.CssClass, activateActionLink.Modal.Title, activateActionLink.Confirmation,
						activateActionLink.Modal.DismissButtonSrText, activateActionLink.Modal.PrimaryButtonText,
						activateActionLink.Modal.CloseButtonText,
						activateActionLink.Modal.TitleCssClass, activateActionLink.Modal.PrimaryButtonCssClass,
						activateActionLink.Modal.CloseButtonCssClass);
			}

			if (activateQuoteActionLink.Enabled && (activateQuoteActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ActivateQuoteModal(activateQuoteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateQuoteActionLink.Modal.CssClass, activateQuoteActionLink.Modal.Title, activateQuoteActionLink.Confirmation,
						activateQuoteActionLink.Modal.DismissButtonSrText, activateQuoteActionLink.Modal.PrimaryButtonText,
						activateQuoteActionLink.Modal.CloseButtonText,
						activateQuoteActionLink.Modal.TitleCssClass, activateQuoteActionLink.Modal.PrimaryButtonCssClass,
						activateQuoteActionLink.Modal.CloseButtonCssClass);
			}

			if (opportunityOnHoldActionLink.Enabled && (opportunityOnHoldActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.SetOpportunityOnHoldModal(
						opportunityOnHoldActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						opportunityOnHoldActionLink.Modal.CssClass, opportunityOnHoldActionLink.Modal.Title,
						opportunityOnHoldActionLink.Confirmation, opportunityOnHoldActionLink.Modal.DismissButtonSrText,
						opportunityOnHoldActionLink.Modal.PrimaryButtonText, opportunityOnHoldActionLink.Modal.CloseButtonText,
						opportunityOnHoldActionLink.Modal.TitleCssClass, opportunityOnHoldActionLink.Modal.PrimaryButtonCssClass,
						opportunityOnHoldActionLink.Modal.CloseButtonCssClass);
			}

			if (reopenOpportunityActionLink.Enabled && (reopenOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.ReopenOpportunityModal(reopenOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenOpportunityActionLink.Modal.CssClass, reopenOpportunityActionLink.Modal.Title,
						reopenOpportunityActionLink.Confirmation, reopenOpportunityActionLink.Modal.DismissButtonSrText,
						reopenOpportunityActionLink.Modal.PrimaryButtonText, reopenOpportunityActionLink.Modal.CloseButtonText,
						reopenOpportunityActionLink.Modal.TitleCssClass, reopenOpportunityActionLink.Modal.PrimaryButtonCssClass,
						reopenOpportunityActionLink.Modal.CloseButtonCssClass);
			}

			if (winOpportunityActionLink.Enabled && (winOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.WinOpportunityModal(winOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						winOpportunityActionLink.Modal.CssClass, winOpportunityActionLink.Modal.Title, winOpportunityActionLink.Confirmation,
						winOpportunityActionLink.Modal.DismissButtonSrText, winOpportunityActionLink.Modal.PrimaryButtonText,
						winOpportunityActionLink.Modal.CloseButtonText,
						winOpportunityActionLink.Modal.TitleCssClass, winOpportunityActionLink.Modal.PrimaryButtonCssClass,
						winOpportunityActionLink.Modal.CloseButtonCssClass);
			}

			if (loseOpportunityActionLink.Enabled && (loseOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.LoseOpportunityModal(loseOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						loseOpportunityActionLink.Modal.CssClass, loseOpportunityActionLink.Modal.Title,
						loseOpportunityActionLink.Confirmation, loseOpportunityActionLink.Modal.DismissButtonSrText,
						loseOpportunityActionLink.Modal.PrimaryButtonText, loseOpportunityActionLink.Modal.CloseButtonText,
						loseOpportunityActionLink.Modal.TitleCssClass, loseOpportunityActionLink.Modal.PrimaryButtonCssClass,
						loseOpportunityActionLink.Modal.CloseButtonCssClass);
			}

			if (generateQuoteFromOpportunityActionLink.Enabled &&
				(generateQuoteFromOpportunityActionLink.ShowModal == ShowModal.Yes))
			{
				container.InnerHtml +=
					html.GenerateQuoteFromOpportunityModal(
						generateQuoteFromOpportunityActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						generateQuoteFromOpportunityActionLink.Modal.CssClass, generateQuoteFromOpportunityActionLink.Modal.Title,
						generateQuoteFromOpportunityActionLink.Confirmation,
						generateQuoteFromOpportunityActionLink.Modal.DismissButtonSrText,
						generateQuoteFromOpportunityActionLink.Modal.PrimaryButtonText,
						generateQuoteFromOpportunityActionLink.Modal.CloseButtonText,
						generateQuoteFromOpportunityActionLink.Modal.TitleCssClass,
						generateQuoteFromOpportunityActionLink.Modal.PrimaryButtonCssClass,
						generateQuoteFromOpportunityActionLink.Modal.CloseButtonCssClass);
			}

			if (updatePipelinePhaseAction.Enabled)
			{
				container.InnerHtml += html.UpdatePipelinePhaseModal(GetPipelinePhases(),
					updatePipelinePhaseAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
					updatePipelinePhaseAction.Modal.CssClass, updatePipelinePhaseAction.Modal.Title,
					updatePipelinePhaseAction.Confirmation,
					updatePipelinePhaseAction.PipelinePhaseLabel, updatePipelinePhaseAction.DescriptionLabel,
					updatePipelinePhaseAction.Modal.DismissButtonSrText, updatePipelinePhaseAction.Modal.PrimaryButtonText,
					updatePipelinePhaseAction.Modal.CloseButtonText, updatePipelinePhaseAction.Modal.TitleCssClass,
					updatePipelinePhaseAction.Modal.PrimaryButtonCssClass, updatePipelinePhaseAction.Modal.CloseButtonCssClass);
			}

			if (firstWorkflow != null && firstWorkflow.Enabled)
			{
				container.InnerHtml += html.WorkflowModal(firstWorkflow.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
					firstWorkflow.Modal.CssClass, firstWorkflow.Modal.Title, firstWorkflow.Confirmation,
					firstWorkflow.Modal.DismissButtonSrText, firstWorkflow.Modal.PrimaryButtonText, firstWorkflow.Modal.CloseButtonText,
					firstWorkflow.Modal.TitleCssClass, firstWorkflow.Modal.PrimaryButtonCssClass, firstWorkflow.Modal.CloseButtonCssClass);
			}

			if (disassociateAction != null && disassociateAction.Enabled)
			{
				container.InnerHtml +=
					html.DissassociateModal(disassociateAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						disassociateAction.Modal.CssClass, disassociateAction.Modal.Title, disassociateAction.Confirmation,
						disassociateAction.Modal.DismissButtonSrText, disassociateAction.Modal.PrimaryButtonText,
						disassociateAction.Modal.CloseButtonText, disassociateAction.Modal.TitleCssClass,
						disassociateAction.Modal.PrimaryButtonCssClass, disassociateAction.Modal.CloseButtonCssClass);
			}
		}

		private static OptionMetadataCollection GetPipelinePhases()
		{
			var serviceContext = CrmConfigurationManager.CreateContext();

			var response = (RetrieveAttributeResponse)serviceContext.Execute(new RetrieveAttributeRequest
			{
				EntityLogicalName = "opportunity",
				LogicalName = "salesstage"
			});

			var picklist = response.AttributeMetadata as PicklistAttributeMetadata;

			return picklist == null ? null : picklist.OptionSet.Options;
		}
	}
}
