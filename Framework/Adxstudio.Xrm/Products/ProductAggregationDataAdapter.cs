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
	internal class ProductAggregationDataAdapter : IFilterableProductAggregationDataAdapter
	{
		private enum SortDirection
		{
			Ascending,
			Descending
		}

		private const string DefaultSortExpression = "Name";

		private static readonly IDictionary<string, Expression<Func<Entity, object>>> SortExpressions = new Dictionary<string, Expression<Func<Entity, object>>>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "Name", product => product.GetAttributeValue<string>("name") },
			{ "Price", product => product.GetAttributeValue<Money>("price") },
			{ "Rating", product => product.GetAttributeValue<object>("adx_ratingaverage") },
		};

		public ProductAggregationDataAdapter(
			IDataAdapterDependencies dependencies,
			Func<OrganizationServiceContext, string, int?, int> selectCount,
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

		protected Func<OrganizationServiceContext, string, int?, int> SelectCount { get; private set; }

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectEntities { get; private set; }

		public IEnumerable<IProduct> SelectProducts()
		{
			return SelectProducts(0, -1);
		}

		public IEnumerable<IProduct> SelectProducts(string sortExpression)
		{
			return SelectProducts(0, -1, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, string sortExpression)
		{
			return SelectProducts(brand, 0, -1, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating, string sortExpression)
		{
			return SelectProducts(brand, rating, 0, -1, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating)
		{
			return SelectProducts(brand, rating, 0, -1, DefaultSortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows)
		{
			return SelectProducts(startRowIndex, maximumRows, DefaultSortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows, string sortExpression)
		{
			return SelectProducts(null, startRowIndex, maximumRows, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int startRowIndex, int maximumRows, string sortExpression)
		{
			return SelectProducts(brand, null, startRowIndex, maximumRows, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating, int startRowIndex, int maximumRows, string sortExpression)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("brand={0}, rating={1}, startRowIndex={2}, maximumRows={3}, sortExpression={4}: Start", brand, rating, startRowIndex, maximumRows, sortExpression));

			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return Enumerable.Empty<IProduct>();
			}

			var serviceContext = Dependencies.GetServiceContext();

			var query = SelectEntities(serviceContext);

			Guid brandId;

			if (Guid.TryParse(brand, out brandId))
			{
				query = query.Where(e => e.GetAttributeValue<EntityReference>("adx_brand") == new EntityReference("adx_brand", brandId));
			}

			if (rating.HasValue)
			{
				var minValue = Convert.ToDouble(rating.Value);

				query = query.Where(e => e.GetAttributeValue<double?>("adx_ratingaverage") >= minValue);
			}

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

			var products = new ProductFactory(serviceContext, user, website).Create(query);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Product, HttpContext.Current, "read_product", string.Empty, products.Count(), string.Empty, "read");
			}

			return products;
		}

		public int SelectProductCount()
		{
			return SelectProductCount(null, (int?)null);
		}

		public int SelectProductCount(string sortExpression)
		{
			return SelectProductCount();
		}

		public int SelectProductCount(string brand, string sortExpression)
		{
			return SelectProductCount(brand, null, sortExpression);
		}

		public int SelectProductCount(string brand, int? rating, string sortExpression)
		{
			return SelectProductCount(brand, rating);
		}

		public int SelectProductCount(string brand, int? rating)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("brand={0}, rating={1}: Start", brand, rating));

			var serviceContext = Dependencies.GetServiceContext();

			var count = SelectCount(serviceContext, brand, rating);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return count;
		}

		private static readonly Regex SortExpressionPattern = new Regex(@"(?<name>\w+)\s*(?<direction>(asc|ascending|desc|descending))?\s*(,)?", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

		private static IEnumerable<Tuple<Expression<Func<Entity, object>>, SortDirection>> ParseSortExpression(string sortExpression)
		{
			if (string.IsNullOrEmpty(sortExpression))
			{
				return Enumerable.Empty<Tuple<Expression<Func<Entity, object>>, SortDirection>>();
			}

			return SortExpressionPattern.Matches(sortExpression).Cast<Match>().Select(match =>
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
