/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;

namespace Site.Controls
{
	public partial class NewsPanel : PortalUserControl
	{
		private Entity _newsBlog;

		protected void Page_Load(object sender, EventArgs e)
		{
			LoadNewsBlog();

			Visible = (_newsBlog != null);
		}

		protected void CreateNewsDataAdapter(object sender, ObjectDataSourceEventArgs args)
		{
			args.ObjectInstance = new BlogDataAdapter(_newsBlog, new PortalContextDataAdapterDependencies(Portal, requestContext: Request.RequestContext));
		}

		private void LoadNewsBlog()
		{
			var newsBlogName = Portal.ServiceContext.GetSiteSettingValueByName(Website, "News Blog Name");

			if (string.IsNullOrWhiteSpace(newsBlogName))
			{
				return;
			}

			_newsBlog = Portal.ServiceContext.CreateQuery("adx_blog")
				.FirstOrDefault(b =>
					b.GetAttributeValue<Guid>("adx_websiteid") == Website.Id
					&&
					b.GetAttributeValue<string>("adx_name") == newsBlogName);
		}
	}
}
