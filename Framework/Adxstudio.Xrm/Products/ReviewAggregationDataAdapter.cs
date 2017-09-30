/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	public class ReviewAggregationDataAdapter : IReviewAggregationDataAdapter
	{
		private enum SortDirection
		{
			Ascending,
			Descending
		}

		private const string DefaultSortExpression = "SubmittedOn DESC";

		private static readonly IDictionary<string, Expression<Func<Entity, object>>> SortExpressions = new Dictionary<string, Expression<Func<Entity, object>>>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "SubmittedOn", review => review.GetAttributeValue<object>("adx_submittedon") },
			{ "Rating", review => review.GetAttributeValue<object>("adx_rating") },
		};

		public ReviewAggregationDataAdapter(
			IDataAdapterDependencies dependencies,
			Func<OrganizationServiceContext, int> selectCount,
			Func<OrganizationServiceContext, IQueryable<Entity>> selectEntities)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (selectCount == null) throw new ArgumentNullException("selectCount");
			if (selectEntities == null) throw new ArgumentNullException("selectEntities");

			Dependencies = dependencies;
			SelectCount = selectCount;
			SelectEntities = selectEntities;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected Func<OrganizationServiceContext, int> SelectCount { get; private set; }

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectEntities { get; private set; }

		public IEnumerable<IReview> SelectReviews()
		{
			return SelectReviews(0, -1);
		}

		public IEnumerable<IReview> SelectReviews(string sortExpression)
		{
			return SelectReviews(0, -1, sortExpression);
		}

		public IEnumerable<IReview> SelectReviews(int startRowIndex, int maximumRows)
		{
			return SelectReviews(startRowIndex, maximumRows, DefaultSortExpression);
		}

		public IEnumerable<IReview> SelectReviews(int startRowIndex, int maximumRows, string sortExpression)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("startRowIndex={0}, maximumRows={1}, sortExpression={2}: Start", startRowIndex, maximumRows, sortExpression));

			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return Enumerable.Empty<IReview>();
			}

			var serviceContext = Dependencies.GetServiceContext();

			var query = SelectEntities(serviceContext);

			var sorts = ParseSortExpression(string.IsNullOrEmpty(sortExpression) ? DefaultSortExpression : sortExpression);

			query = sorts.Aggregate(query, (current, sort) => sort.Item2 == SortDirection.Ascending
				? current.OrderBy(sort.Item1)
				: current.OrderByDescending(sort.Item1));

			if (startRowIndex > 0)
			{
				query = query.Skip(startRowIndex);
			}

			if (maximumRows > 0)
			{
				query = query.Take(maximumRows);
			}

			var user = Dependencies.GetPortalUser();
			var website = Dependencies.GetWebsite();

			var reviews = new ReviewFactory(serviceContext, user, website).Create(query);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.ProductReview, HttpContext.Current, "read_product_review", string.Empty, reviews.Count(), string.Empty, "read");
			}

			return reviews;
		}

		public int SelectReviewCount()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var serviceContext = Dependencies.GetServiceContext();

			var count = SelectCount(serviceContext);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return count;
		}

		private static readonly Regex _sortExpressionPattern = new Regex(@"(?<name>\w+)\s*(?<direction>(asc|ascending|desc|descending))?\s*(,)?", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

		private static IEnumerable<Tuple<Expression<Func<Entity, object>>, SortDirection>> ParseSortExpression(string sortExpression)
		{
			if (string.IsNullOrEmpty(sortExpression))
			{
				return Enumerable.Empty<Tuple<Expression<Func<Entity, object>>, SortDirection>>();
			}

			return _sortExpressionPattern.Matches(sortExpression).Cast<Match>().Select(match =>
			{
				var sortNameCapture = match.Groups["name"].Value;

				Expression<Func<Entity, object>> sort;

				if (!SortExpressions.TryGetValue(sortNameCapture, out sort))
				{
					return null;
				}

				var sortDirectionCapture = match.Groups["direction"].Value;

				var sortDirection = string.IsNullOrEmpty(sortDirectionCapture) || sortDirectionCapture.StartsWith("a", StringComparison.InvariantCultureIgnoreCase)
					? SortDirection.Ascending
					: SortDirection.Descending;

				return new Tuple<Expression<Func<Entity, object>>, SortDirection>(sort, sortDirection);

			}).Where(sort => sort != null).ToArray();
		}
	}
}
