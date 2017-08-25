/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Careers.Pages
{
	public partial class Careers : PortalPage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			var query = from jp in XrmContext.CreateQuery("adx_jobposting")
				where jp.GetAttributeValue<EntityReference>("adx_websiteid") == Website.ToEntityReference()
						where jp.GetAttributeValue<OptionSetValue>("statecode") != null && jp.GetAttributeValue<OptionSetValue>("statecode").Value == 0
				orderby jp.GetAttributeValue<string>("adx_name")
				select new
				{
					Id = jp.GetAttributeValue<Guid>("adx_jobpostingid"),
					Name = jp.GetAttributeValue<string>("adx_name"),
					Description = jp.GetAttributeValue<string>("adx_description"),
					ClosingOn = jp.GetAttributeValue<DateTime?>("adx_closingon")
				};

			var postings = query.ToArray().Where(e => IsOpen(e.ClosingOn));

			JobPostings.DataSource = postings;
			JobPostings.DataBind();
		}

		private static bool IsOpen(DateTime? closingOn)
		{
			return closingOn == null || closingOn.Value.AddDays(1).Date >= DateTime.UtcNow;
		}
	}
}
