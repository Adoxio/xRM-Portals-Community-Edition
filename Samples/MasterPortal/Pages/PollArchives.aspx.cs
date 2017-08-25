/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Pages
{
	public partial class PollArchives : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_poll")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions =
								new[]
								{
									new Condition("adx_websiteid", ConditionOperator.Equal, Website.Id),
									new Condition("adx_expirationdate", ConditionOperator.LessEqual, DateTime.UtcNow)
								}
						}
					}
				}
			};

			var polls = PortalOrganizationService.RetrieveMultiple(fetch).Entities;

			PollsArchiveListView.DataSource = polls;
			PollsArchiveListView.DataBind();
		}

		protected void PollsArchiveListView_ItemDataBound(object sender, ListViewItemEventArgs e)
		{
			var listItem = e.Item as ListViewDataItem;

			if (listItem == null || listItem.DataItem == null)
			{
				return;
			}

			var poll = listItem.DataItem as Entity;

			var listView = (ListView)e.Item.FindControl("PollResponsesListView");
			var totalLabel = (System.Web.UI.WebControls.Label)e.Item.FindControl("Total");

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_polloption")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions =
								new[]
								{
									new Condition("adx_pollid", ConditionOperator.Equal, poll.Id)
								}
						}
					}
				}
			};

			var pollResponses = PortalOrganizationService.RetrieveMultiple(fetch).Entities.ToList();

			var totalVotes = pollResponses.Sum(p => p.GetAttributeValue<int?>("adx_votes").GetValueOrDefault(0));

			totalLabel.Text = totalVotes.ToString(CultureInfo.InvariantCulture);

			if (totalVotes <= 0)
			{
				return;
			}

			var results = from response in pollResponses
				select new
				{
					Response = response.GetAttributeValue<string>("adx_answer"),
					Count = response.GetAttributeValue<int?>("adx_votes").GetValueOrDefault(0),
					Percentage = Convert.ToInt32((response.GetAttributeValue<int?>("adx_votes").GetValueOrDefault(0)) / ((float)totalVotes) * (100))
				};

			listView.DataSource = results;

			listView.DataBind();
		}
	}
}
