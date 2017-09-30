/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.HelpDesk
{
	/// <summary>
	/// Organization Service context extensions
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		private enum EntityState
		{
			Active = 0
		}

		private enum AllotmentType
		{
			NumberCases = 100000000
		}

		/// <summary>
		/// Get Support Plans for a specified Contact
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contactid"></param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveSupportPlansForContact(this OrganizationServiceContext context, Guid contactid)
		{
			var plans = context.CreateQuery("adx_supportplan").Where(o =>
							o.GetAttributeValue<EntityReference>("adx_customercontact") == new EntityReference("contact", contactid) &&
							(o.GetAttributeValue<OptionSetValue>("adx_allotmenttype") != null && o.GetAttributeValue<OptionSetValue>("adx_allotmenttype").Value == (int)AllotmentType.NumberCases && o.GetAttributeValue<int?>("adx_allotmentsremaining").GetValueOrDefault(0) > 0) &&
							o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)EntityState.Active);

			return plans;
		}

		/// <summary>
		/// Get Support Plans for a specified Contact and Product
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contactid"></param>
		/// <param name="productid"> </param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveSupportPlansForContact(this OrganizationServiceContext context, Guid contactid, Guid productid)
		{
			var plans = context.CreateQuery("adx_supportplan").Where(o =>
							o.GetAttributeValue<EntityReference>("adx_customercontact") == new EntityReference("contact", contactid) &&
							o.GetAttributeValue<EntityReference>("adx_product") == new EntityReference("product", productid) &&
							(o.GetAttributeValue<OptionSetValue>("adx_allotmenttype") != null && o.GetAttributeValue<OptionSetValue>("adx_allotmenttype").Value == (int)AllotmentType.NumberCases && o.GetAttributeValue<int?>("adx_allotmentsremaining").GetValueOrDefault(0) > 0) &&
							o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)EntityState.Active);

			return plans;
		}

		/// <summary>
		/// Get Support Plans for a specified Account
		/// </summary>
		/// <param name="context"></param>
		/// <param name="accountid"></param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveSupportPlansForAccount(this OrganizationServiceContext context, Guid accountid)
		{
			var plans = context.CreateQuery("adx_supportplan").Where(o =>
							o.GetAttributeValue<EntityReference>("adx_customer") == new EntityReference("account", accountid) &&
							(o.GetAttributeValue<OptionSetValue>("adx_allotmenttype") != null && o.GetAttributeValue<OptionSetValue>("adx_allotmenttype").Value == (int)AllotmentType.NumberCases && o.GetAttributeValue<int?>("adx_allotmentsremaining").GetValueOrDefault(0) > 0) &&
							o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)EntityState.Active);

			return plans;
		}

		/// <summary>
		/// Get Support Plans for a specified Account and Product
		/// </summary>
		/// <param name="context"></param>
		/// <param name="accountid"></param>
		/// <param name="productid"> </param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveSupportPlansForAccount(this OrganizationServiceContext context, Guid accountid, Guid productid)
		{
			var plans = context.CreateQuery("adx_supportplan").Where(o =>
							o.GetAttributeValue<EntityReference>("adx_customer") == new EntityReference("account", accountid) &&
							o.GetAttributeValue<EntityReference>("adx_product") == new EntityReference("product", productid) &&
							(o.GetAttributeValue<OptionSetValue>("adx_allotmenttype") != null && o.GetAttributeValue<OptionSetValue>("adx_allotmenttype").Value == (int)AllotmentType.NumberCases && o.GetAttributeValue<int?>("adx_allotmentsremaining").GetValueOrDefault(0) > 0) &&
							o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)EntityState.Active);

			return plans;
		}
	}
}
