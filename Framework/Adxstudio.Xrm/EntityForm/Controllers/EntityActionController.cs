/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.EntityForm.Controllers
{
	public class EntityActionController : Controller
	{
		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult CloseCase(EntityReference entityReference, string resolutionSubject, string resolutionDescription)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = context.RetrieveSingle(entityReference.LogicalName,
				FetchAttribute.None,
				new Condition("incidentid", ConditionOperator.Equal, entityReference.Id));
			var test = entityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, context);

			adapter.CloseIncident(entityReference, resolutionSubject, resolutionDescription);

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Case, this.HttpContext, "close_incident", 1, entity.ToEntityReference(), "edit");
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ResolveCase(EntityReference entityReference, string resolutionSubject, string resolutionDescription)
		{
			if (string.IsNullOrWhiteSpace(resolutionSubject) && string.IsNullOrWhiteSpace(resolutionDescription))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, (ResourceManager.GetString("ResolutionSubNDesc_Field_Required_Exception")));
			}
			if (string.IsNullOrWhiteSpace(resolutionSubject))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, (ResourceManager.GetString("ResolutionSub_Field_Required_Exception")));
			}
			if (string.IsNullOrWhiteSpace(resolutionDescription)) 
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, (ResourceManager.GetString("ResolutionDesc_Field_Required_Exception")));
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portalContext.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = serviceContext.RetrieveSingle(entityReference.LogicalName,
				FetchAttribute.None,
				new Condition("incidentid", ConditionOperator.Equal, entityReference.Id));
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portalContext, serviceContext);

			adapter.CloseIncident(entityReference, resolutionSubject, resolutionDescription);

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Case, this.HttpContext, "resolve_incident", 1, entity.ToEntityReference(), "edit");
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ReopenCase(EntityReference entityReference)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portalContext.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = serviceContext.RetrieveSingle(entityReference.LogicalName,
				FetchAttribute.None,
				new Condition("incidentid", ConditionOperator.Equal, entityReference.Id));
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CaseDataAdapter(entityReference, new PortalContextDataAdapterDependencies(portalContext, null, Request.RequestContext));

			adapter.Reopen();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Case, this.HttpContext, "reopen_incident", 1, entity.ToEntityReference(), "edit");
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult CancelCase(EntityReference entityReference)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portalContext.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = serviceContext.RetrieveSingle(entityReference.LogicalName,
				FetchAttribute.None,
				new Condition("incidentid", ConditionOperator.Equal, entityReference.Id));
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CaseDataAdapter(entityReference,
				new PortalContextDataAdapterDependencies(portalContext, null, Request.RequestContext));

			adapter.Cancel();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Case, this.HttpContext, "cancel_incident", 1, entity.ToEntityReference(), "edit");
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult QualifyLead(EntityReference entityReference, bool createAccount, bool createContact, bool createOpportunity)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			if (!crmEntityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var lead = context.RetrieveSingle(entityReference.LogicalName,
				FetchAttribute.None,
				new Condition("leadid", ConditionOperator.Equal, entityReference.Id));

			if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, lead)
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "contact")
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "account")
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "opportunity"))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, context);

			adapter.QualifyLead(entityReference, createAccount, createContact, createOpportunity, null, null);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult CalculateActualValueOfOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.CalculateActualValueOfOpportunity(entityReference);

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			serviceContext.TryRemoveFromCache(entity);

			serviceContext.UpdateObject(entity);

			serviceContext.SaveChanges();

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult	ConvertQuoteToOrder(EntityReference	entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			if (!crmEntityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var quote = context.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("quoteid") == entityReference.Id);

			if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, quote)
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "salesorder"))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var	adapter	= new CoreDataAdapter(portal, context);

			var status = quote.GetAttributeValue<OptionSetValue>("statecode").Value;

			if (status != (int)QuoteState.Active)
			{
				adapter.SetState(entityReference, (int)QuoteState.Active, (int)QuoteStatusCode.InProgressActive);
			}

			if (status != (int)QuoteState.Won)
			{
				adapter.WinQuote(entityReference);

			}

			adapter.CovertQuoteToSalesOrder(entityReference);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ConvertOrderToInvoice(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			if (!crmEntityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var salesorder = context.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("salesorderid") == entityReference.Id);

			if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, salesorder)
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "invoice"))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, context);

			adapter.ConvertSalesOrderToInvoice(entityReference);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Deactivate(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(serviceContext, entityReference.LogicalName);

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>(entityPrimaryKey) == entityReference.Id);

			try
			{
				if (!entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity))
				{
					Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return Json(new { Message = ResourceManager.GetString("DoNot_Have_Appropriate_Permissions") });
				}
			}
			catch (InvalidOperationException)
			{
				Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return Json(new { Message = ResourceManager.GetString("DoNot_Have_Appropriate_Permissions") });
			}

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.SetState(entityReference, (int)StandardState.Inactive, (int)StandardStatusCode.Inactive);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Activate(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(serviceContext, entityReference.LogicalName);

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>(entityPrimaryKey) == entityReference.Id);

			if (!entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.SetState(entityReference, (int)StandardState.Active, (int)StandardStatusCode.Active);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ActivateQuote(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.SetState(entityReference, (int)QuoteState.Active, (int)QuoteStatusCode.InProgressActive);

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("quoteid") == entityReference.Id);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult SetOpportunityOnHold(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.SetState(entityReference, (int)OpportunityState.Open, (int)OpportunityStatusCode.OnHold);

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult WinOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.WinOpportunity(entityReference);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult LoseOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entity);

			if (!test)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, serviceContext);

			adapter.LoseOpportunity(entityReference);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GenerateQuoteFromOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			var opportunity = context.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			if (!crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Write, opportunity)
				|| !crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Create, "quote"))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("DoNot_Have_Appropriate_Permissions"));
			}

			var adapter = new CoreDataAdapter(portal, context);

			adapter.GenerateQuoteFromOpportunity(entityReference);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult UpdatePipelinePhase(EntityReference entityReference, string stepName, int salesStage, string description)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var opportunity = serviceContext.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			Debug.Assert(opportunity != null, "opportunity != null");

			opportunity.Attributes["stepname"] = stepName;
			opportunity.Attributes["salesstage"] = new OptionSetValue(salesStage);

			serviceContext.UpdateObject(opportunity);

			serviceContext.SaveChanges();

			var objNotes = new Entity();
			objNotes.LogicalName = "annotation";

			objNotes.Attributes["subject"] = "Updated Pipeline Phase: " + stepName;
			objNotes.Attributes["notetext"] = description;

			objNotes.Attributes["objectid"] = entityReference;

			//objNotes.Attributes["objecttypecode"] = 1084;

			serviceContext.AddObject(objNotes);

			serviceContext.SaveChanges();

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			entity.Attributes["stepname"] = stepName;

			serviceContext.UpdateObject(entity);

			serviceContext.SaveChanges();



			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ReopenOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, serviceContext);

			var opportunityEntity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			var isPartnerCreated = opportunityEntity.GetAttributeValue<bool>("adx_partnercreated");

			if (isPartnerCreated)
			{
				adapter.SetState(entityReference, (int)OpportunityState.Open, (int)OpportunityStatusCode.InProgress);
			}
			else
			{
				adapter.SetState(entityReference, (int)OpportunityState.Open, (int)OpportunityStatusCode.Accepted);
			}

			var entity = serviceContext.CreateQuery(entityReference.LogicalName).First(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult SetState(EntityReference entityReference, int stateCode, int statusReason)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, context);

			adapter.SetState(entityReference, stateCode, statusReason);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}
	}
}
