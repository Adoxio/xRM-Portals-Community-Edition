/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using System.Web;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class ManageCustomerAccounts : PortalPage
	{
		private DataTable _accounts;

		protected string SortDirection
		{
			get { return ViewState["SortDirection"] as string ?? "ASC"; }
			set { ViewState["SortDirection"] = value; }
		}
		protected string SortExpression
		{
			get { return ViewState["SortExpression"] as string ?? "Accepted"; }
			set { ViewState["SortExpression"] = value; }
		}
		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelCreateAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));
			var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));
			var channelReadAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));
			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var validAcccountClassificationCode = parentCustomerAccount != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)Enums.AccountClassificationCode.Partner;

			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				AccountsList.Visible = false;

				return;
			}

			if (!channelReadAccess && !channelWriteAccess)
			{
				ChannelPermissionsError.Visible = true;
			}
			else
			{
				if (parentCustomerAccount == null)
				{
					NoParentAccountError.Visible = true;
				}
				else
				{
					ParentAccountClassificationCodeError.Visible = !validAcccountClassificationCode;
				}
			}

			if ((!channelReadAccess && !channelWriteAccess) || parentCustomerAccount == null || !validAcccountClassificationCode)
			{
				AccountsList.Visible = false;

				return;
			}

			CreateButton.Visible = channelCreateAccess;

			var accounts = ServiceContext.CreateQuery("account")
				.Where(a => a.GetAttributeValue<EntityReference>("msa_managingpartnerid") == parentCustomerAccount.ToEntityReference())
				.Where(a => a.GetAttributeValue<int?>("statecode") == 0)
				.OrderBy(a => a.GetAttributeValue<string>("name"))
				.Select(a => a);

			_accounts = EnumerableExtensions.CopyToDataTable(accounts.Select(a => new
			{
				accountid = a.Id,
				ID = a.GetAttributeValue<string>("name"),
				City = a.GetAttributeValue<string>("address1_city"),
				State = a.GetAttributeValue<string>("address1_stateorprovince"),
				Phone = a.GetAttributeValue<string>("address1_telephone1"),
				Email = a.GetAttributeValue<string>("emailaddress1"),
			}));

			_accounts.Columns["City"].ColumnName = "City";
			_accounts.Columns["State"].ColumnName = "State";
			_accounts.Columns["Phone"].ColumnName = "Phone";
			_accounts.Columns["Email"].ColumnName = "E-mail Address";

			CustomerAccountsList.DataKeyNames = new[] { "accountid" };
			CustomerAccountsList.DataSource = _accounts;
			CustomerAccountsList.DataBind();
		}
		protected void CustomerAccountsList_Sorting(object sender, GridViewSortEventArgs e)
		{
			SortDirection = e.SortExpression == SortExpression
				? (SortDirection == "ASC" ? "DESC" : "ASC")
				: "ASC";

			SortExpression = e.SortExpression;

			_accounts.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			CustomerAccountsList.DataSource = _accounts;
			CustomerAccountsList.DataBind();
		}

		protected void CustomerAccountsList_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.Header ||
				e.Row.RowType == DataControlRowType.DataRow)
			{
				e.Row.Cells[0].Visible = false;
			}

			if (e.Row.RowType != DataControlRowType.DataRow)
			{
				return;
			}

			var dataKey = CustomerAccountsList.DataKeys[e.Row.RowIndex].Value;

			e.Row.Cells[1].Text = string.Format(@"<a href=""{0}"">{1}</a>",
					HttpUtility.HtmlEncode(EditAccountUrl(dataKey)),
					HttpUtility.HtmlEncode(e.Row.Cells[1].Text));

			e.Row.Cells[1].Attributes.Add("style", "white-space: nowrap;");
		}

		protected string EditAccountUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Edit Customer Account");

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("AccountID", id.ToString());

			return url.PathWithQueryString;
		}
	}
}
