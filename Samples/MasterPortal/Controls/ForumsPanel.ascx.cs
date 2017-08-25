/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Web.Mvc.Html;

namespace Site.Controls
{
	public partial class ForumsPanel : PortalUserControl
	{
		protected void CreateForumDataAdapter(object sender, ObjectDataSourceEventArgs args)
		{
			args.ObjectInstance = new WebsiteForumDataAdapter(new PortalContextDataAdapterDependencies(
				Portal,
				new PaginatedLatestPostUrlProvider("page", Html.IntegerSetting("Forums/PostsPerPage").GetValueOrDefault(20))));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}
	}
}
