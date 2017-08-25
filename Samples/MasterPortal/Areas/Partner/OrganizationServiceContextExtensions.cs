/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Partner;

namespace Site.Areas.Partner
{
	public static class OrganizationServiceContextExtensions
	{
		public static Enum GetAlertType(this OrganizationServiceContext context, Guid? id, Entity website)
		{
			if (id == null)
			{
				return Enums.AlertType.None;
			}

			var opportunity = context.CreateQuery("opportunity").First(opp => opp.GetAttributeValue<Guid>("opportunityid") == id);

			if (opportunity.GetAttributeValue<OptionSetValue>("statecode") != null && opportunity.GetAttributeValue<OptionSetValue>("statecode").Value != (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Open)
			{
				return Enums.AlertType.None;
			}

			if (opportunity.GetAttributeValue<OptionSetValue>("statuscode") != null && opportunity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered)
			{
				return Enums.AlertType.New;
			}

			var opportunityLatestStatusModifiedOn = context.GetOpportunityLatestStatusModifiedOn(opportunity);

			return opportunityLatestStatusModifiedOn == null
					   ? Enums.AlertType.None
					   : opportunityLatestStatusModifiedOn <= DateTime.Now.AddDays(-context.GetInactiveDaysUntilPotentiallyStalled(website))
							 ? Enums.AlertType.PotentiallyStalled
							 : (opportunityLatestStatusModifiedOn <= DateTime.Now.AddDays(-context.GetInactiveDaysUntilOverdue(website))
									? Enums.AlertType.Overdue
									: Enums.AlertType.None);
		}
	}

}
