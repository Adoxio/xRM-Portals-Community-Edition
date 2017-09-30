/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.UI.WebControls;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Products;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Products.Pages
{
	public partial class ProductCollection : PortalPage
	{
		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);

		protected EntityReference Brand;

		protected Dictionary<int, string> RatingFilterOptions = new Dictionary<int, string>
		{
			{ 3, "+" },
			{ 4, "+" },
			{ 5, string.Empty },
		};

		protected IDictionary<string, string> SortOptions = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "Name ASC", "A&ndash;Z" },
			{ "Name DESC", "Z&ndash;A" },
			{ "Price ASC, Name ASC", "$ Low&ndash;High" },
			{ "Price DESC, Name ASC", "$ High&ndash;Low" },
			{ "Rating DESC, Name ASC", "Rating &ndash; High&ndash;Low" },
			{ "Rating ASC, Name ASC", "Rating &ndash; Low&ndash;High" },
		};

		protected EntityReference Subject;

		protected string CurrentSortOptionLabel
		{
			get
			{
				string sortOption;

				return SortOptions.TryGetValue(Request.QueryString["orderby"] ?? string.Empty, out sortOption)
					? sortOption
					: SortOptions.First().Value;
			}
		}

		protected string AllBrandsUrl
		{
			get
			{
				var urlBuilder = new UrlBuilder(Request.Url.PathAndQuery);

				urlBuilder.QueryString.Remove("brand");
				urlBuilder.QueryString.Remove("page");

				return urlBuilder.PathWithQueryString;
			}
		}

		protected bool NoBrandFilter
		{
			get { return Brand == null; }
		}

		protected bool NoRatingFilter
		{
			get
			{
				int activeRatingFilter;

				return !(
					int.TryParse(Request.QueryString["rating"], out activeRatingFilter)
						&& RatingFilterOptions.Any(option => option.Key == activeRatingFilter));
			}
		}

		protected string AnyRatingUrl
		{
			get
			{
				var urlBuilder = new UrlBuilder(Request.Url.PathAndQuery);

				urlBuilder.QueryString.Remove("rating");
				urlBuilder.QueryString.Remove("page");

				return urlBuilder.PathWithQueryString;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			if (Entity.LogicalName != "adx_webpage")
			{
				throw new ArgumentException(string.Format("Invalid target entity. This page template is designed for use with webpage (adx_webpage) records. The current entity type is {0}.", Entity.LogicalName));
			}

			Subject = Entity.GetAttributeValue<EntityReference>("adx_subjectid");

			if (Subject == null)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Current Web Page with ID equal to '{0}' does not have a required subject. Please ensure subjects have been created and that a subject has been assigned to the Web Page and any products that should be displayed for this page.", Entity.Id));

                Subject = new EntityReference("subject", Guid.NewGuid());
			}

			Guid brandId;

			Brand = Guid.TryParse(Request.QueryString["brand"], out brandId)
				? new EntityReference("adx_brand", brandId)
				: null;
		}

		protected void CreateSubjectProductsDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = new SubjectProductsDataAdapter(Subject, new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext));
		}

		protected void CreateBrandDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = new SubjectBrandDataAdapter(Subject, new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext));
		}

		protected string GetBrandFilterUrl(object id)
		{
			if (id == null)
			{
				return Request.Url.PathAndQuery;
			}

			var urlBuilder = new UrlBuilder(Request.Url.PathAndQuery);

			urlBuilder.QueryString.Set("brand", id.ToString());
			urlBuilder.QueryString.Remove("page");

			return urlBuilder.PathWithQueryString;
		}

		protected string GetSortUrl(string sortExpresion)
		{
			if (sortExpresion == null)
			{
				return Request.Url.PathAndQuery;
			}

			var urlBuilder = new UrlBuilder(Request.Url.PathAndQuery);

			urlBuilder.QueryString.Set("orderby", sortExpresion);
			urlBuilder.QueryString.Remove("page");

			return urlBuilder.PathWithQueryString;
		}

		protected bool IsActiveBrandFilter(object id)
		{
			return id is Guid
				&& Brand != null
				&& (Guid)id == Brand.Id;
		}

		protected bool IsActiveRatingFilter(int rating)
		{
			int activeRatingFilter;

			return int.TryParse(Request.QueryString["rating"], out activeRatingFilter)
				&& activeRatingFilter == rating;
		}

		protected string GetRatingFilterUrl(int rating)
		{
			var urlBuilder = new UrlBuilder(Request.Url.PathAndQuery);

			urlBuilder.QueryString.Set("rating", rating.ToString(CultureInfo.InvariantCulture));
			urlBuilder.QueryString.Remove("page");

			return urlBuilder.PathWithQueryString;
		}
	}
}
