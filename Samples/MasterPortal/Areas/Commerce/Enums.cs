/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Site.Areas.Commerce
{
	public class Enums
	{
		/// <summary>
		/// Enumeration for the Status of an Sales Order.
		/// </summary>
		public enum SalesOrderState
		{
			Active = 0,
			Submitted = 1,
			Canceled = 2,
			Fulfilled = 3,
			Invoiced = 4
		}

		/// <summary>
		/// Enumeration for the Status reason of an Sales Order.
		/// </summary>
		public enum SalesOrderStatusCode
		{
			New = 1,
			Pending = 2,
			Processesing = 3,
			Canceled = 4,
			Shipped = 100001,
			Partial = 100002,
			Invoiced = 100003
		}

		/// <summary>
		/// Enumeration for the Status of an Quote.
		/// </summary>
		public enum QuoteState
		{
			Draft = 0,
			Active = 1,
			Won = 2,
			Closed = 3
		}

		/// <summary>
		/// Enumeration for the Status reason of an Quote.
		/// </summary>
		public enum QuoteStatusCode
		{
			Draft = 1,
			New = 2,
			Open = 3,
			Won = 4,
			Lost = 5,
			Canceled = 6,
			Revised = 7
		}
	}
}
