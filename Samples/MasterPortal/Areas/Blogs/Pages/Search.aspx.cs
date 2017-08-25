/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web;
using Site.Pages;

namespace Site.Areas.Blogs.Pages
{
	public partial class Search : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Guid blogId;

			if (Guid.TryParse(Request.QueryString["blog"], out blogId))
			{
				SearchData.SelectParameters.Add("Blog", blogId.ToString());
				SearchData.Query = "+(@Query) +adx_blogid:@Blog";
			}
		}

		protected string GetDisplayUrl(object urlData)
		{
			if (urlData == null)
			{
				return string.Empty;
			}

			try
			{
				return new UrlBuilder(urlData.ToString()).ToString();
			}
			catch (FormatException)
			{
				return string.Empty;
			}
		}

		protected void SearchData_OnSelected(object sender, SearchDataSourceStatusEventArgs args)
		{
			// If the SearchDataSource reports that the index was not found, try to build the index.
			if (args.Exception is IndexNotFoundException)
			{
				using (var builder = args.Provider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}

				args.ExceptionHandled = true;

				// Redirect/refresh to let the user continue their search.
				Response.Redirect(Request.Url.PathAndQuery);
			}
		}
	}
}
