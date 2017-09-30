/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Action = Adxstudio.Xrm.Web.UI.JsonConfiguration.Action;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Services.Query;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	

	public class FormConfiguration : IFormConfiguration
	{
		public string EntityName { get; set; }
		public string PrimaryKeyName { get; set; }
		public Guid Id { get; set; }
		public DeleteActionLink DeleteActionLink { get; set; }
		public CloseIncidentActionLink CloseIncidentActionLink { get; set; }
		public ResolveCaseActionLink ResolveCaseActionLink { get; set; }
		public ReopenCaseActionLink ReopenCaseActionLink { get; set; }
		public CancelCaseActionLink CancelCaseActionLink { get; set; }
		public QualifyLeadActionLink QualifyLeadActionLink { get; set; }
		public ConvertOrderToInvoiceActionLink ConvertOrderToInvoiceActionLink { get; set; }
		public ConvertQuoteToOrderActionLink ConvertQuoteToOrderActionLink { get; set; }
		public CalculateOpportunityActionLink CalculateOpportunityActionLink { get; set; }
		public DeactivateActionLink DeactivateActionLink { get; set; }
		public ActivateActionLink ActivateActionLink { get; set; }
		public ActivateQuoteActionLink ActivateQuoteActionLink { get; set; }
		public SetOpportunityOnHoldActionLink SetOpportunityOnHoldActionLink { get; set; }
		public ReopenOpportunityActionLink ReopenOpportunityActionLink { get; set; }
		public WinOpportunityActionLink WinOpportunityActionLink { get; set; }
		public LoseOpportunityActionLink LoseOpportunityActionLink { get; set; }
		public GenerateQuoteFromOpportunityActionLink GenerateQuoteFromOpportunityActionLink { get; set; }
		public UpdatePipelinePhaseActionLink UpdatePipelinePhaseActionLink { get; set; }
		public SubmitActionLink SubmitActionLink { get; set; }
		public PreviousActionLink PreviousActionLink { get; set; }
		public NextActionLink NextActionLink { get; set; }
		public CreateRelatedRecordActionLink CreateRelatedRecordActionLink { get; set; }
		public List<ViewActionLink> TopFormActionLinks { get; set; }
		public List<ViewActionLink> BottomFormActionLinks { get; set; }
		public bool EnableEntityPermissions { get; set; }
		public int LanguageCode { get; set; }
		public string PortalName { get; set; }
		public bool EnableActions { get; set; }
		public ActionButtonStyle? ActionButtonStyle { get; set; }
		public ActionButtonPlacement? ActionButtonPlacement { get; set; }
		public ActionButtonAlignment? ActionButtonAlignment { get; set; }
		public ShowActionButtonContainer? ShowActionButtonContainer { get; set; }
		public string ActionButtonDropDownLabel { set; get; }
		public string ActionNavbarCssClass { set; get; }
		public string TopContainerCssClass { get; set; }
		public string BottomContainerCssClass { get; set; }
		public bool AutoGenerateSteps { set; get; }

		public FormConfiguration()
		{
			ShowActionButtonContainer = JsonConfiguration.ShowActionButtonContainer.No;

			IntializeSpecialActionLinks();

			SubmitActionLink = new SubmitActionLink();
			PreviousActionLink = new PreviousActionLink();
			NextActionLink = new NextActionLink();
			CreateRelatedRecordActionLink = new CreateRelatedRecordActionLink();

		}

		public FormConfiguration(string entityName)
		{
			ShowActionButtonContainer = JsonConfiguration.ShowActionButtonContainer.No;

			EntityName = entityName;
			IntializeSpecialActionLinks();

			SubmitActionLink = new SubmitActionLink();
			PreviousActionLink = new PreviousActionLink();
			NextActionLink = new NextActionLink();
			CreateRelatedRecordActionLink = new CreateRelatedRecordActionLink();
		}

		/// <summary>
		/// Class constructor used by the EntityForm Control
		/// </summary>
		public FormConfiguration(IPortalContext portalContext, string entityName, FormActionMetadata formActionMetadata, 
			string portalName, int languageCode, bool enableEntityPermissions, bool autoGenerateStepsFromTabs, bool addSubmitButton = false, bool addNextPrevious = false)
		{
			ShowActionButtonContainer = JsonConfiguration.ShowActionButtonContainer.No;

			EntityName = entityName;
			IntializeSpecialActionLinks();

			SubmitActionLink = new SubmitActionLink();
			PreviousActionLink = new PreviousActionLink();
			NextActionLink = new NextActionLink();
			CreateRelatedRecordActionLink = new CreateRelatedRecordActionLink();

			if (formActionMetadata == null) return;

			ActionButtonDropDownLabel = formActionMetadata.ActionButtonDropDownLabel.GetLocalizedString(languageCode);
			ActionNavbarCssClass = formActionMetadata.ActionNavbarCssClass;
			TopContainerCssClass = formActionMetadata.TopContainerCssClass;
			BottomContainerCssClass = formActionMetadata.BottomContainerCssClass;

			ActionButtonStyle = formActionMetadata.ActionButtonStyle;
			ActionButtonPlacement = formActionMetadata.ActionButtonPlacement;
			ActionButtonAlignment = formActionMetadata.ActionButtonAlignment;

			EnableEntityPermissions = enableEntityPermissions;
			PortalName = portalName;
			LanguageCode = languageCode;

			AutoGenerateSteps = autoGenerateStepsFromTabs;

			if (formActionMetadata.Actions != null) SetFormActions(portalContext, formActionMetadata, languageCode, portalName, addSubmitButton, addNextPrevious);
		}

		//add the special action links
		private void IntializeSpecialActionLinks()
		{
			//DeleteActionLink = new DeleteActionLink();
			//ResolveCaseActionLink = new ResolveCaseActionLink();
			//ReopenCaseActionLink = new ReopenCaseActionLink();
			//CancelCaseActionLink = new CancelCaseActionLink();
			//CloseIncidentActionLink = new CloseIncidentActionLink();
			//QualifyLeadActionLink = new QualifyLeadActionLink();
			//ConvertQuoteToOrderActionLink = new ConvertQuoteToOrderActionLink();
			//ConvertOrderToInvoiceActionLink = new ConvertOrderToInvoiceActionLink();
			//CalculateOpportunityActionLink = new CalculateOpportunityActionLink();
			//DeactivateActionLink = new DeactivateActionLink();
			//ActivateActionLink = new ActivateActionLink();
			//ActivateQuoteActionLink = new ActivateQuoteActionLink();
			//SetOpportunityOnHoldActionLink = new SetOpportunityOnHoldActionLink();
			//ReopenOpportunityActionLink = new ReopenOpportunityActionLink();
			//WinOpportunityActionLink = new WinOpportunityActionLink();
			//LoseOpportunityActionLink = new LoseOpportunityActionLink();
			//GenerateQuoteFromOpportunityActionLink = new GenerateQuoteFromOpportunityActionLink();
			//UpdatePipelinePhaseActionLink = new UpdatePipelinePhaseActionLink();
		}

		private void SetFormActions(IPortalContext portalContext, FormActionMetadata formActionMetadata, int languageCode, string portalName = null, 
			bool addSubmitButton = false, bool addNextPrevious = false)
		{
			var actions = formActionMetadata.Actions.OrderBy(a => a.ActionIndex).ToList();

			if (addSubmitButton)
			{
				var submitActions = actions.Where(a => a is SubmitAction);

				if (!(submitActions.Any()))
				{
					var newSubmitAction = new SubmitAction()
										  {
											  ActionButtonAlignment	= JsonConfiguration.ActionButtonAlignment.Left,
											  ActionButtonPlacement	= JsonConfiguration.ActionButtonPlacement.BelowForm,
											  ActionButtonStyle	= JsonConfiguration.ActionButtonStyle.ButtonGroup,
											  ActionIndex =	-1,
											  ButtonCssClass = "btn-primary"
										  };

					actions.Insert(0, newSubmitAction);
				}
			}

			if (addNextPrevious && AutoGenerateSteps)
			{
				var nextActions = actions.Where(a => a is NextAction);

				if (!(nextActions.Any()))
				{
					var newNextAction = new NextAction()
					{
						ActionButtonAlignment = JsonConfiguration.ActionButtonAlignment.Left,
						ActionButtonPlacement = JsonConfiguration.ActionButtonPlacement.BelowForm,
						ActionButtonStyle = JsonConfiguration.ActionButtonStyle.ButtonGroup,
						ActionIndex = -1,
						ButtonCssClass = "btn btn-primary navbar-btn button next next-btn"
					};

					actions.Insert(0, newNextAction);
				}

				var previousActions = actions.Where(a => a is PreviousAction);

				if (!(previousActions.Any()))
				{
					var newPreviousAction = new PreviousAction()
					{
						ActionButtonAlignment = JsonConfiguration.ActionButtonAlignment.Left,
						ActionButtonPlacement = JsonConfiguration.ActionButtonPlacement.BelowForm,
						ActionButtonStyle = JsonConfiguration.ActionButtonStyle.ButtonGroup,
						ActionIndex = -2,
						ButtonCssClass = "btn btn-default navbar-btn button previous previous-btn"
					};

					actions.Insert(0, newPreviousAction);
				}
			}

			var topItemActionLinks = new List<ViewActionLink>();
			var bottomItemActionLinks = new List<ViewActionLink>();

			foreach (var action in actions)
			{
				if (action is WorkflowAction)
				{
					var workflowAction = (WorkflowAction)action;

					if (!workflowAction.IsConfigurationValid()) continue;
					
					var workflowActionLink = new WorkflowActionLink(portalContext, new EntityReference("workflow", workflowAction.WorkflowId), formActionMetadata, languageCode, workflowAction, true, null, portalName);

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, workflowActionLink);
				}
				
				if (action is DeleteAction)
				{
					var deleteAction = (DeleteAction)action;

					if (!deleteAction.IsConfigurationValid()) continue;
					
					var deleteActionLink = new DeleteActionLink(portalContext, formActionMetadata, languageCode, deleteAction, true, null, portalName);
					
					DeleteActionLink = deleteActionLink;

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, deleteActionLink);
				}

				if (action is SubmitAction)
				{
					var submitAction = (SubmitAction)action;

					if (!submitAction.IsConfigurationValid()) continue;
					
					var submitActionLink = new SubmitActionLink(portalContext, languageCode, submitAction, true, null, portalName);

					SubmitActionLink = submitActionLink;

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, submitActionLink);
				}

				if (action is PreviousAction)
				{
					var previousAction = (PreviousAction)action;

					if (!previousAction.IsConfigurationValid()) continue;
					
					var previousActionLink = new PreviousActionLink(portalContext, languageCode, previousAction, true, null, portalName);

					PreviousActionLink = previousActionLink;

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, previousActionLink);
				}

				if (action is NextAction)
				{
					var nextAction = (NextAction)action;

					if (!nextAction.IsConfigurationValid()) continue;
					
					var nextActionLink = new NextActionLink(portalContext, languageCode, nextAction, true, null, portalName);

					NextActionLink = nextActionLink;

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, nextActionLink);
				}

				if (action is CreateRelatedRecordAction)
				{
					var createRelatedRecordAction = (CreateRelatedRecordAction)action;

					if (!createRelatedRecordAction.IsConfigurationValid()) continue;

					var createRelatedRecordActionLink = new CreateRelatedRecordActionLink(portalContext, formActionMetadata, languageCode, createRelatedRecordAction, true, portalName);

					CreateRelatedRecordActionLink = createRelatedRecordActionLink;

					AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, createRelatedRecordActionLink);
				}

				SetSpecialMessageActions(portalContext, formActionMetadata, languageCode, action, topItemActionLinks, bottomItemActionLinks, portalName);
			}

			TopFormActionLinks	= topItemActionLinks;
			BottomFormActionLinks = bottomItemActionLinks;
		}

		private void SetSpecialMessageActions(IPortalContext portalContext, FormActionMetadata formActionMetadata,
			int languageCode, Action action, List<ViewActionLink> topItemActionLinks, List<ViewActionLink> bottomItemActionLinks, string portalName = null)
		{
			if (action is CloseIncidentAction)
			{
				var closeIncidentAction = (CloseIncidentAction)action;

				if (!closeIncidentAction.IsConfigurationValid()) return;

				var closeIncidentActionLink = new CloseIncidentActionLink(portalContext, formActionMetadata, languageCode, closeIncidentAction, true, null, portalName);

				CloseIncidentActionLink = closeIncidentActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, closeIncidentActionLink);
			}

			if (action is ResolveCaseAction)
			{
				var resolveCaseAction = (ResolveCaseAction)action;

				if (!resolveCaseAction.IsConfigurationValid()) return;

				var resolveCaseActionLink = new ResolveCaseActionLink(portalContext, formActionMetadata, languageCode, resolveCaseAction, true, null, portalName);

				ResolveCaseActionLink = resolveCaseActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, resolveCaseActionLink);
			}

			if (action is ReopenCaseAction)
			{
				var reopenCaseAction = (ReopenCaseAction)action;

				if (!reopenCaseAction.IsConfigurationValid()) return;

				var reopenCaseActionLink = new ReopenCaseActionLink(portalContext, formActionMetadata, languageCode, reopenCaseAction, true, null, portalName);

				ReopenCaseActionLink = reopenCaseActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, reopenCaseActionLink);
			}

			if (action is CancelCaseAction)
			{
				var cancelCaseAction = (CancelCaseAction)action;

				if (!cancelCaseAction.IsConfigurationValid()) return;

				var cancelCaseActionLink = new CancelCaseActionLink(portalContext, formActionMetadata, languageCode, cancelCaseAction, true, null, portalName);

				CancelCaseActionLink = cancelCaseActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, cancelCaseActionLink);
			}

			if (action is QualifyLeadAction)
			{
				var qualifyLeadAction = (QualifyLeadAction)action;

				if (!qualifyLeadAction.IsConfigurationValid()) return;

				var qualifyLeadActionLink = new QualifyLeadActionLink(portalContext, formActionMetadata, languageCode, qualifyLeadAction, true, null, portalName);

				QualifyLeadActionLink = qualifyLeadActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, qualifyLeadActionLink);
			}

			if (action is ConvertQuoteToOrderAction)
			{
				var convertQuoteToOrderAction = (ConvertQuoteToOrderAction)action;

				if (!convertQuoteToOrderAction.IsConfigurationValid()) return;

				var convertQuoteToOrderActionLink = new ConvertQuoteToOrderActionLink(portalContext, formActionMetadata, languageCode, (ConvertQuoteToOrderAction)action);

				ConvertQuoteToOrderActionLink = convertQuoteToOrderActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, convertQuoteToOrderActionLink);
			}

			if (action is ConvertOrderToInvoiceAction)
			{
				var convertOrderToInvoiceAction = (ConvertOrderToInvoiceAction)action;

				if (!convertOrderToInvoiceAction.IsConfigurationValid()) return;

				var convertOrderToInvoiceActionLink = new ConvertOrderToInvoiceActionLink(portalContext, formActionMetadata, languageCode, convertOrderToInvoiceAction, true, null, portalName);

				ConvertOrderToInvoiceActionLink = convertOrderToInvoiceActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, convertOrderToInvoiceActionLink);
			}

			if (action is DeactivateAction)
			{
				var deactivateAction = (DeactivateAction)action;

				if (!deactivateAction.IsConfigurationValid()) return;

				var deactivateActionLink = new DeactivateActionLink(portalContext, formActionMetadata, languageCode, deactivateAction, true, null, portalName);

				DeactivateActionLink = deactivateActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, deactivateActionLink);
			}

			if (action is ActivateAction)
			{
				var activateAction = (ActivateAction)action;

				if (!activateAction.IsConfigurationValid()) return;

				var activateActionLink = new ActivateActionLink(portalContext, formActionMetadata, languageCode, activateAction, true, null, portalName);

				ActivateActionLink = activateActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, activateActionLink);
			}

			if (action is ActivateQuoteAction)
			{
				var activateQuoteAction = (ActivateQuoteAction)action;

				if (!activateQuoteAction.IsConfigurationValid()) return;

				var activateQuoteActionLink = new ActivateQuoteActionLink(portalContext, formActionMetadata, languageCode, activateQuoteAction, true, null, portalName);

				ActivateQuoteActionLink = activateQuoteActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, activateQuoteActionLink);
			}

			if (action is SetOpportunityOnHoldAction)
			{
				var setOpportunityOnHoldAction = (SetOpportunityOnHoldAction)action;

				if (!setOpportunityOnHoldAction.IsConfigurationValid()) return;

				var setOpportunityOnHoldActionLink = new SetOpportunityOnHoldActionLink(portalContext, formActionMetadata, languageCode, setOpportunityOnHoldAction, true, null, portalName);

				SetOpportunityOnHoldActionLink = setOpportunityOnHoldActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, setOpportunityOnHoldActionLink);
			}

			if (action is ReopenOpportunityAction)
			{
				var reopenOpportunityAction = (ReopenOpportunityAction)action;

				if (!reopenOpportunityAction.IsConfigurationValid()) return;

				var reopenOpportunityActionLink = new ReopenOpportunityActionLink(portalContext, formActionMetadata, languageCode, reopenOpportunityAction, true, null, portalName);

				ReopenOpportunityActionLink = reopenOpportunityActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, reopenOpportunityActionLink);
			}

			if (action is CalculateOpportunityAction)
			{
				var calculateOpportunityAction = (CalculateOpportunityAction)action;

				if (!calculateOpportunityAction.IsConfigurationValid()) return;

				var calculateOpportunityActionLink = new CalculateOpportunityActionLink(portalContext, formActionMetadata, languageCode, calculateOpportunityAction, true, null, portalName);
				
				CalculateOpportunityActionLink = calculateOpportunityActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, calculateOpportunityActionLink);
			}

			if (action is WinOpportunityAction)
			{
				var winOpportunityAction = (WinOpportunityAction)action;

				if (!winOpportunityAction.IsConfigurationValid()) return;

				var winOpportunityActionLink = new WinOpportunityActionLink(portalContext, formActionMetadata, languageCode, winOpportunityAction, true, null, portalName);

				WinOpportunityActionLink = winOpportunityActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, winOpportunityActionLink);
			}

			if (action is LoseOpportunityAction)
			{
				var loseOpportunityAction = (LoseOpportunityAction)action;

				if (!loseOpportunityAction.IsConfigurationValid()) return;

				var loseOpportunityActionLink = new LoseOpportunityActionLink(portalContext, formActionMetadata, languageCode, loseOpportunityAction, true, null, portalName);

				LoseOpportunityActionLink = loseOpportunityActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, loseOpportunityActionLink);
			}

			if (action is GenerateQuoteFromOpportunityAction)
			{
				var generateQuoteFromOpportunityAction = (GenerateQuoteFromOpportunityAction)action;

				if (!generateQuoteFromOpportunityAction.IsConfigurationValid()) return;

				var generateQuoteFromOpportunityActionLink = new GenerateQuoteFromOpportunityActionLink(portalContext, formActionMetadata, languageCode, generateQuoteFromOpportunityAction, true, null, portalName);

				GenerateQuoteFromOpportunityActionLink = generateQuoteFromOpportunityActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, generateQuoteFromOpportunityActionLink);
			}

			if (action is UpdatePipelinePhaseAction)
			{
				var updatePipelinePhaseAction = (UpdatePipelinePhaseAction)action;

				if (!updatePipelinePhaseAction.IsConfigurationValid()) return;

				var updatePipelinePhaseActionLink = new UpdatePipelinePhaseActionLink(portalContext, formActionMetadata, languageCode, updatePipelinePhaseAction, true, null, portalName);

				UpdatePipelinePhaseActionLink = updatePipelinePhaseActionLink;

				AddActionLinkToSet(topItemActionLinks, bottomItemActionLinks, updatePipelinePhaseActionLink);
			}
		}

		private static void AddActionLinkToSet(List<ViewActionLink> topItemActionLinks, List<ViewActionLink> bottomItemActionLinks, ViewActionLink actionLink)
		{
			if (actionLink.ActionButtonPlacement == JsonConfiguration.ActionButtonPlacement.AboveForm)
				topItemActionLinks.Add(actionLink);
			else
				bottomItemActionLinks.Add(actionLink);
		}

		public void DisableActionsBasedOnPermissions(OrganizationServiceContext context, string entityName, Guid entityId)
		{
			var entityMetadata = MetadataHelper.GetEntityMetadata(context, entityName);
			var primaryKeyName = entityMetadata.PrimaryIdAttribute;
			
			EnableActions = false;

			if (!EnableEntityPermissions) return;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			var entity = context.CreateQuery(entityName).FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryKeyName) == entityId);

			if (entity == null)
			{
				EnableActions = false;
				return;
			}

			DisableLinks(context, entityName, TopFormActionLinks, crmEntityPermissionProvider, entity);
			DisableLinks(context, entityName, BottomFormActionLinks, crmEntityPermissionProvider, entity);
		}

		private void DisableLinks(OrganizationServiceContext context, string entityName, List<ViewActionLink> links,
			CrmEntityPermissionProvider crmEntityPermissionProvider, Entity entity)
		{
			foreach (var link in links)
			{
				if (link is WorkflowActionLink)
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is SubmitActionLink || link is NextActionLink || link is PreviousActionLink)
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is DeleteActionLink)
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Delete, entity))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is ActivateActionLink || link is DeactivateActionLink)
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (entityName == "incident")
				{
					if (link is CloseIncidentActionLink || link is ResolveCaseActionLink || link is ReopenCaseActionLink ||
					    link is CancelCaseActionLink)
					{
						if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
						{
							link.Enabled = false;
						}
						else EnableActions = true;
					}
				}

				if (link is QualifyLeadActionLink && entityName == "lead")
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity)
					    || !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "contact")
					    || !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "account")
					    || !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "opportunity"))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is ConvertQuoteToOrderActionLink && entityName == "quote")
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity)
					    || !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "salesorder"))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is ActivateQuoteActionLink && entityName == "quote")
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (link is ConvertOrderToInvoiceActionLink && entityName == "salesorder")
				{
					if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity)
					    || !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "invoice"))
					{
						link.Enabled = false;
					}
					else EnableActions = true;
				}

				if (entityName == "opportunity")
				{
					if (link is SetOpportunityOnHoldActionLink || link is CalculateOpportunityActionLink || link is WinOpportunityActionLink
					|| link is LoseOpportunityActionLink || link is GenerateQuoteFromOpportunityActionLink || link is UpdatePipelinePhaseActionLink
					|| link is ReopenOpportunityActionLink)
					{
						if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity))
						{
							link.Enabled = false;
						}
						else EnableActions = true;
					}
				}
			}
		}

		public void DisableActionsBasedOnFilterCriteria(OrganizationServiceContext context, string entityName, Guid entityId)
		{
			if (context == null || string.IsNullOrEmpty(entityName) || entityId == Guid.Empty || TopFormActionLinks == null || BottomFormActionLinks == null)
			{
				return;
			}

			// Disable TopFormActionLinks
			foreach (var link in TopFormActionLinks)
			{
				DisableActionLinkBasedOnFilterCriteria(context, link, entityName, entityId);
			}

			// Disable BottonFormActionLinks
			foreach (var link in BottomFormActionLinks)
			{
				DisableActionLinkBasedOnFilterCriteria(context, link, entityName, entityId);
			}
		}

		private void DisableActionLinkBasedOnFilterCriteria(OrganizationServiceContext context, ViewActionLink link, string entityName, Guid entityId)
		{
			if (link == null || string.IsNullOrEmpty(link.FilterCriteria))
			{
				return;
			}

			// The condition for the filter on primary key
			var primaryAttributeCondition = new Condition
			{
				Attribute = entityName + "id",
				Operator = ConditionOperator.Equal,
				Value = entityId
			};

			// Primary key filter
			var primaryAttributeFilter = new Filter
			{
				Conditions = new[] { primaryAttributeCondition },
				Type = LogicalOperator.And
			};

			Fetch fetch = null;
			try
			{
				fetch = Fetch.Parse(link.FilterCriteria);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.Message);
				return;
			}

			if (fetch.Entity == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Fetch XML query is not valid. Entity can't be Null.");
				return;
			}
			// Set number of fields to fetch to 0.
			fetch.Entity.Attributes = FetchAttribute.None;

			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>();
			}
			// Add primary key filter
			fetch.Entity.Filters.Add(primaryAttributeFilter);

			RetrieveMultipleResponse response;
			try
			{
				response = (RetrieveMultipleResponse)context.Execute(new RetrieveMultipleRequest { Query = fetch.ToFetchExpression() });
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.Message);
				return;
			}

			link.Enabled = response.EntityCollection.Entities.Count > 0;
		}
	}
}
