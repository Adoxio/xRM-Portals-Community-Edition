/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Retail.Pages
{
	public partial class MyAccount : PortalPage
	{
		private const string _accountFetchXmlFormat = @"
			<fetch mapping=""logical"">
				<entity name=""account"">
					<all-attributes />
					<filter type=""and"">
						<condition attribute=""accountid"" operator=""eq"" value=""{0}""/>
					</filter>
				</entity>
			</fetch>";

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == Contact.Id);

			if (contact == null)
			{
				return;
			}

			var household = contact.GetAttributeValue<EntityReference>("parentcustomerid");

			if (household == null)
			{
				Household.Visible = false;

				return;
			}

			Household.Visible = true;

			HouseholdDataSource.FetchXml = string.Format(_accountFetchXmlFormat, household.Id);
		}
	}
}
