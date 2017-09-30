/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Core
{
	public enum StandardState
	{
		Active = 0,
		Inactive = 1
	}

	public enum StandardStatusCode
	{
		Active = 1,
		Inactive = 2
	}

	public enum LeadState
	{
		Open = 0,
		Qualified = 1,
		Disqualified = 2
	}

	public enum LeadStatusCode
	{
		New = 1,
		Contacted = 2,
		Qualified = 3,
		Lost = 4,
		CannotContact = 5,
		NoLongerInterested = 6,
		Canceled = 7
	}

	public enum IncidentState
	{
		Active = 0,
		Resolved = 1,
		Canceled = 2
	}

	public enum IncidentStatusCode
	{
		InProgress = 1,
		OnHold = 2,
		WaitingForDetails = 3,
		Researching = 4,
		ProblemSolved = 5,
		Canceled = 6
	}

	public enum QuoteState
	{
		Draft = 0,
		Active = 1,
		Won = 2,
		Closed = 3
	}

	public enum QuoteStatusCode
	{
		InProgressDraft = 1,
		InProgressActive = 2,
		Open = 3,
		Won = 4,
		Lost = 5,
		Canceled = 6,
		Revised = 7
	}

	public enum OpportunityState
	{
		Open = 0,
		Won = 1,
		Lost = 2
	}

	public enum OpportunityStatusCode
	{
		InProgress = 1,
		OnHold = 2,
		Won = 3,
		Canceled = 4,
		OutSold = 5,
		OpenForBidding = 200000,
		Delivered = 100000001,
		Accepted = 100000003,
		Purchased = 100000004,
		Declined = 100000006,
		Expired = 100000007,
		Returned = 756150000
	}

	public class CoreDataAdapter
	{
		public IPortalContext Portal { get; private set; }

		public OrganizationServiceContext Context { get; private set; }

		public CoreDataAdapter(IPortalContext portal, OrganizationServiceContext context)
		{
			Portal = portal;
			Context = context;
		}

		public SetStateResponse SetState(EntityReference entityReference, int stateCode, int statusReason)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;
			
			var setStateRequest = new SetStateRequest()
								  {
									  EntityMoniker	= entityReference,
									  State	= new OptionSetValue(stateCode),
									  Status = new OptionSetValue(statusReason)
								  };

			var setStateResponse = (SetStateResponse)context.Execute(setStateRequest);

			return setStateResponse;
		}

		public QualifyLeadResponse QualifyLead(EntityReference entityReference, bool createAccount, bool createContact, bool createOpportunity,
			EntityReference opportunityCurrencyId, EntityReference opportunityCustomerId)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			if (opportunityCurrencyId == null)
			{
				var currency =
					context.CreateQuery("transactioncurrency").FirstOrDefault(
						c => c.GetAttributeValue<string>("currencyname") == "US Dollar") ??
					context.CreateQuery("transactioncurrency").FirstOrDefault();

				if (currency != null) opportunityCurrencyId = currency.ToEntityReference();
			}

			//start with the case of creating contact, account, AND opportunity
			var qualifyLeadReq = new QualifyLeadRequest
			{
				CreateAccount = true,
				CreateContact = true,
				CreateOpportunity = true,
				LeadId = entityReference,
				OpportunityCurrencyId = opportunityCurrencyId,
				Status = new OptionSetValue((int)LeadStatusCode.Qualified)
			};

			var qualifyLeadResponse = (QualifyLeadResponse)context.Execute(qualifyLeadReq);

			return qualifyLeadResponse;
		}

		public CloseIncidentResponse CloseIncident(EntityReference entityReference, string resolutionSubject, string resolutionDescription)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var entity = context.CreateQuery("incident").FirstOrDefault(i => i.GetAttributeValue<Guid>("incidentid") == entityReference.Id);

			var resolution = new Entity("incidentresolution");

			resolution.Attributes["subject"] = resolutionSubject;

			resolution.Attributes["description"] = resolutionDescription;

			resolution.Attributes["incidentid"] = entityReference;

			// Create the request to close the incident, and set its resolution to the 
			// resolution created above
			var closeRequest = new CloseIncidentRequest
				{
					IncidentResolution = resolution,
					Status = new OptionSetValue((int)IncidentStatusCode.ProblemSolved)
				};

			// Execute the close request
			var closeResponse = (CloseIncidentResponse)context.Execute(closeRequest);

			context.TryRemoveFromCache(entity);
			context.TryRemoveFromCache(resolution);

			return closeResponse;

		}

		public ConvertQuoteToSalesOrderResponse	CovertQuoteToSalesOrder(EntityReference	entityReference)
		{
			var	portal = PortalCrmConfigurationManager.CreatePortalContext();
			var	context	= portal.ServiceContext;

			// Define columns to be	retrieved after	creating the order
			var	salesOrderColumns =	new	ColumnSet("salesorderid", "totalamount");

			// Convert the quote to	a sales	order
			var	convertQuoteRequest	=
				new	ConvertQuoteToSalesOrderRequest()
				{
					QuoteId	= entityReference.Id,
					ColumnSet =	salesOrderColumns
				};
			
			var	convertQuoteResponse = (ConvertQuoteToSalesOrderResponse)context.Execute(convertQuoteRequest);

			return convertQuoteResponse;
		}

		public ConvertSalesOrderToInvoiceResponse ConvertSalesOrderToInvoice(EntityReference entityReference)
		{
			var	portal = PortalCrmConfigurationManager.CreatePortalContext();
			var	context	= portal.ServiceContext;

			// Define columns to be	retrieved after	creating the order
			var	salesOrderColumns =	new	ColumnSet("invoiceid", "totalamount");

			// Convert the quote to	a sales	order
			var	convertQuoteRequest	=
				new	ConvertSalesOrderToInvoiceRequest()
				{
					SalesOrderId = entityReference.Id,
					ColumnSet =	salesOrderColumns
				};

			var	convertQuoteResponse = (ConvertSalesOrderToInvoiceResponse)context.Execute(convertQuoteRequest);

			return convertQuoteResponse;
		}

		public WinQuoteResponse WinQuote(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var quoteClose = new Entity("quoteclose");

			quoteClose.Attributes["subject"] = "Quote Closed from Web " + DateTime.Now.ToString(CultureInfo.InvariantCulture);
			quoteClose.Attributes["quoteid"] = entityReference;

			var winQuoteRequest = new WinQuoteRequest()
			{
				QuoteClose = quoteClose,
				Status = new OptionSetValue(-1)
			};

			var winQuoteResponse = (WinQuoteResponse)context.Execute(winQuoteRequest);

			return winQuoteResponse;
		}

		public WinOpportunityResponse WinOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var opportunityClose = new Entity("opportunityclose");

			opportunityClose.Attributes["subject"] = "Opportunity won via Web Portal" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
			opportunityClose.Attributes["opportunityid"] = entityReference;

			var winQuoteRequest = new WinOpportunityRequest()
			{
				OpportunityClose = opportunityClose,
				Status = new OptionSetValue((int)OpportunityStatusCode.Won)
			};

			var winOpportunityResponse = (WinOpportunityResponse)context.Execute(winQuoteRequest);

			return winOpportunityResponse;
		}

		public LoseOpportunityResponse LoseOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var opportunityClose = new Entity("opportunityclose");

			opportunityClose.Attributes["subject"] = "Opportunity closed as lost via Web Portal" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
			opportunityClose.Attributes["opportunityid"] = entityReference;

			var loseOpportunityRequest = new LoseOpportunityRequest()
			{
				OpportunityClose = opportunityClose,
				Status = new OptionSetValue((int)OpportunityStatusCode.Canceled)
			};

			var loseOpportunityResponse = (LoseOpportunityResponse)context.Execute(loseOpportunityRequest);

			return loseOpportunityResponse;
		}

		public CalculateActualValueOpportunityResponse CalculateActualValueOfOpportunity(EntityReference entityReference)
		{
			var	portal = PortalCrmConfigurationManager.CreatePortalContext();
			var	context	= portal.ServiceContext;

			var	calculateActualValueRequest	= 
				new	CalculateActualValueOpportunityRequest()
				{
					OpportunityId = entityReference.Id
				};

			var calculateActualValueResponse =
				(CalculateActualValueOpportunityResponse)context.Execute(calculateActualValueRequest);

			return calculateActualValueResponse;
		}

		public GenerateQuoteFromOpportunityResponse GenerateQuoteFromOpportunity(EntityReference entityReference)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var quoteColumns = new ColumnSet("quoteid", "name");

			// Convert the quote to	a sales	order
			var generateQuoteFromOpportunityRequest =
				new GenerateQuoteFromOpportunityRequest()
				{
					OpportunityId = entityReference.Id,
					ColumnSet = quoteColumns
				};

			var generateQuoteFromOpportunityResponse = (GenerateQuoteFromOpportunityResponse)context.Execute(generateQuoteFromOpportunityRequest);

			return generateQuoteFromOpportunityResponse;
		}



	}
}
