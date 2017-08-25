/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adxstudio.Xrm.Partner
{
	public static class Enums
	{

		/// <summary>
		/// Enumeration for the option set msa_allocatedtopartner.
		/// </summary>
		public enum AllocatedToPartner
		{
			Yes = 1,
			No = 2
		}

		/// <summary>
		/// Enumeration for the Status of an Opportunity.
		/// </summary>
		public enum OpportunityState
		{
			Open = 0,
			Won = 1,
			Lost = 2
		}

		public enum OpportunityStatusReason
		{
			InProgress = 1,
			OnHold = 2,
			Canceled = 4,
			OpenForBidding = 200000,
			Delivered = 100000001,
			Purchased = 100000004,
			Accepted = 100000003,
			Returned = 100000005,
			Declined = 100000006,
			Expired = 100000007
		}

		public enum EntityFormStatusCode
		{
			Active = 1
		}

		public enum OpportunityAccessScope
		{
			Self = 100000000,
			Account = 100000001
		}
	}
}
