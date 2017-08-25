/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	using System.Globalization;
	using System.Web;
	using System.Web.Mvc;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Resources;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// View helpers for entity ratings
	/// </summary>
	public static class RatingExtensions
	{
		/// <summary>
		/// Renders rating for a given entity if the Feedback feature is enabled and the entity is rateable and ratings are enabled for that entity type.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entityReference">The <see cref="EntityReference"/> of the record for which to display rating.</param>
		/// <param name="ratingInfo">The <see cref="IRatingInfo"/> that provides rating value and count if available. If not provided, it will be dynamically retrieved.</param>
		/// <param name="panel">Indicates whether the rating should be wrapped in a bootstrap panel or not.</param>
		/// <param name="panelTitleSnippetName">If panel is true, this is the name of a content snippet to display a title text in the panel heading.</param>
		/// <param name="isReadonly">Indicates whether the rating is readonly (i.e. not editable).</param>
		/// <param name="resetable">Indicates whether the rating can be reset.</param>
		/// <param name="step">Step size value (default: 1)</param>
		/// <param name="min">Minumum value (default: 0)</param>
		/// <param name="max">Maximum value (default: 5)</param>
		/// <param name="urlSave">The URL to the service to create/update a rating.</param>
		/// <param name="roundNearestHalf">Indicates whether or not to round the rating value to the nearest half. Default is true.</param>
		/// <returns>An HTML encoded string representing rating.</returns>
		public static IHtmlString Rating(this HtmlHelper html, EntityReference entityReference, IRatingInfo ratingInfo = null, bool panel = false, string panelTitleSnippetName = "Rating Heading", bool isReadonly = false, bool resetable = false, string step = "1", string min = "0", string max = "5", string urlSave = null, bool roundNearestHalf = true)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new HtmlString(string.Empty);
			}

			if (entityReference == null)
			{
				return new HtmlString(string.Empty);
			}

			if (!isReadonly && string.IsNullOrWhiteSpace(urlSave))
			{
				urlSave = html.GetPortalScopedRouteUrlByName("PortalRateit");
			}

			if (string.IsNullOrWhiteSpace(step))
			{
				step = "1";
			}

			if (string.IsNullOrWhiteSpace(min))
			{
				min = "0";
			}

			if (string.IsNullOrWhiteSpace(max))
			{
				max = "5";
			}

			if (ratingInfo == null)
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext();

				var dataAdapterFactory = new RatingDataAdapterFactory(entityReference);

				var dataAdapter = dataAdapterFactory.GetAdapter(portal, PortalExtensions.GetRequestContext());

				if (!dataAdapter.RatingsEnabled)
				{
					return new HtmlString(string.Empty);
				}

				ratingInfo = dataAdapter.GetRatingInfo() ?? new RatingInfo();
			}

			var ratingValue = roundNearestHalf ? ratingInfo.AverageRatingRounded : ratingInfo.AverageRating;

			var ratingCount = ratingInfo.RatingCount;

			var container = new TagBuilder("div");

			container.AddCssClass("rating");

			var rateit = new TagBuilder("div");

			rateit.AddCssClass("rateit");

			rateit.MergeAttribute("data-rateit-resetable", resetable.ToString().ToLower());

			rateit.MergeAttribute("data-rateit-step", step);

			rateit.MergeAttribute("data-rateit-min", min);

			rateit.MergeAttribute("data-rateit-max", max);

			rateit.MergeAttribute("data-rateit-ispreset", "true");

			rateit.MergeAttribute("data-rateit-readonly", isReadonly.ToString().ToLower());

			rateit.MergeAttribute("data-rateit-value", ratingValue.ToString(CultureInfo.InvariantCulture));

			rateit.MergeAttribute("data-logicalname", entityReference.LogicalName);

			rateit.MergeAttribute("data-id", entityReference.Id.ToString());

			if (!isReadonly)
			{
				rateit.MergeAttribute("data-url-save", urlSave);
			}

			var count = new TagBuilder("span");

			count.AddCssClass("rating-count badge");

			count.InnerHtml = ratingCount.ToString();

			if (panel)
			{
				container.AddCssClass("content-panel panel panel-default");

				var heading = new TagBuilder("div");

				heading.AddCssClass("panel-heading");

				var title = new TagBuilder("h4");

				var icon = new TagBuilder("span");

				icon.AddCssClass("fa fa-star-o");

				icon.MergeAttribute("aria-hidden", "true");

				title.InnerHtml += icon.ToString();

				if (string.IsNullOrWhiteSpace(panelTitleSnippetName))
				{
					panelTitleSnippetName = "Rating Heading";
				}

				var snippet = html.TextSnippet(panelTitleSnippetName, true, "span",
					defaultValue: ResourceManager.GetString("Rate_This_Defaulttext"));

				title.InnerHtml += snippet;

				heading.InnerHtml += title;

				var body = new TagBuilder("div");

				body.AddCssClass("panel-body");

				body.InnerHtml += rateit.ToString();

				body.InnerHtml += count.ToString();

				container.InnerHtml += heading.ToString();

				container.InnerHtml += body.ToString();
			}
			else
			{
				container.InnerHtml += rateit.ToString();

				container.InnerHtml += count.ToString();
			}

			return new HtmlString(container.ToString());
		}
	}
}
