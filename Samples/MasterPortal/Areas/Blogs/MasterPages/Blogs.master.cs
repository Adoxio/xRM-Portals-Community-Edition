/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.MasterPages;
using OrganizationServiceContextExtensions = Adxstudio.Xrm.Cms.OrganizationServiceContextExtensions;
using PortalContextDataAdapterDependencies = Adxstudio.Xrm.Blogs.PortalContextDataAdapterDependencies;
using System.Web;
using Adxstudio.Xrm.Web;

namespace Site.Areas.Blogs.MasterPages
{
	public partial class Blogs : PortalMasterPage
	{
		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);

		private const string _blogQueryStringField = "blog";
		private const string _searchQueryStringField = "q";

		/// <summary>
		/// The search parameter to filter by entity name
		/// </summary>
		private const string SearchEntityNameStringField = "logicalNames";

		/// <summary>
		/// The block entities for filtering search
		/// </summary>
		private const string SearchBlogEntitiesName = "adx_blog,adx_blogpost";

		protected bool SearchIsVisible
		{
			get
			{
				var website = Context.GetWebsite();
				var enabled = website.Settings.Get<bool>("Search/Enabled");
				return enabled && website.Settings.Get<bool>("blogs/displaySearch");
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			Guid blogId;

			BlogSearchFilters.Visible = TryGetBlogScope(out blogId);

			BlogSearch.Visible = SearchIsVisible;

			var q = HttpUtility.HtmlEncode(Request.QueryString[_searchQueryStringField]);

			if (!string.IsNullOrEmpty(q))
			{
				QueryText.Text = HttpUtility.HtmlDecode(q);
			}

			QueryText.Attributes["onkeydown"] = OnEnterKeyDownThenClick(SearchButton);
		}

		protected void SearchButton_Click(object sender, EventArgs e)
		{
			var serviceContext = _portal.Value.ServiceContext;

			var searchPage = OrganizationServiceContextExtensions.GetPageBySiteMarkerName(serviceContext, _portal.Value.Website, "Blog Search");

			if (searchPage == null)
			{
				return;
			}

			var url = OrganizationServiceContextExtensions.GetUrl(serviceContext, searchPage);

			if (url == null)
			{
				return;
			}

			var urlBuilder = new UrlBuilder(url);
		

			urlBuilder.QueryString.Set(_searchQueryStringField, QueryText.Text);
			urlBuilder.QueryString.Set(SearchEntityNameStringField, SearchBlogEntitiesName);

			Guid blogId;

			if (BlogSearchFilters.Visible && BlogSearchFilterOptions.SelectedValue == "blog" && TryGetBlogScope(out blogId))
			{
				urlBuilder.QueryString.Set("filter", String.Format("adx_blogid:@{0}", blogId));
			}

			Response.Redirect(urlBuilder.PathWithQueryString);
		}

		private static string OnEnterKeyDownThenClick(Control button)
		{
			if (button == null) return string.Empty;

			return string.Format(@"
                if(!event.ctrlKey && !event.shiftKey && event.keyCode == 13) {{
                    document.getElementById('{0}').click();
                    return false;
				}}
				return true; ",
				button.ClientID);
		}

		protected void CreateBlogDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			Guid blogId;

			e.ObjectInstance = TryGetBlogIdFromQueryString(out blogId)
				? new BlogDataAdapter(new EntityReference("adx_blog", blogId), new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext))
				: new SiteMapNodeBlogDataAdapter(System.Web.SiteMap.CurrentNode, new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext)) as IBlogDataAdapter;
		}

		private bool TryGetBlogIdFromQueryString(out Guid blogId)
		{
			blogId = default(Guid);

			var blogQuery = Request.QueryString[_blogQueryStringField];

			if (string.IsNullOrEmpty(blogQuery))
			{
				return false;
			}

			return Guid.TryParse(blogQuery, out blogId);
		}

		private bool TryGetBlogScope(out Guid blogId)
		{
			if (TryGetBlogIdFromQueryString(out blogId))
			{
				return true;
			}
			
			var entity = _portal.Value.Entity;

			if (entity == null)
			{
				return false;
			}

			if (entity.LogicalName == "adx_blog")
			{
				blogId = entity.Id;

				return true;
			}

			if (entity.LogicalName == "adx_blogpost")
			{
				var blogReference = _portal.Value.Entity.GetAttributeValue<EntityReference>("adx_blogid");

				if (blogReference != null)
				{
					blogId = blogReference.Id;

					return true;
				}
			}

			return false;
		}
	}
}
