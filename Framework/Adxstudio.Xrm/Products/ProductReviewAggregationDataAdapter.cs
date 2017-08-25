/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Products
{
	public class ProductReviewAggregationDataAdapter : IReviewAggregationDataAdapter
	{
		private readonly ReviewAggregationDataAdapter _helperDataAdapter;

		public ProductReviewAggregationDataAdapter(EntityReference product, IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			_helperDataAdapter = new ReviewAggregationDataAdapter(
				dependencies,
				serviceContext => serviceContext.FetchProductReviewCount(product.Id),
				serviceContext => CreateReviewEntityQuery(serviceContext, product));
		}

		public IEnumerable<IReview> SelectReviews()
		{
			return _helperDataAdapter.SelectReviews();
		}

		public IEnumerable<IReview> SelectReviews(string sortExpression)
		{
			return _helperDataAdapter.SelectReviews(sortExpression);
		}

		public IEnumerable<IReview> SelectReviews(int startRowIndex, int maximumRows)
		{
			return _helperDataAdapter.SelectReviews(startRowIndex, maximumRows);
		}

		public IEnumerable<IReview> SelectReviews(int startRowIndex, int maximumRows, string sortExpression)
		{
			return _helperDataAdapter.SelectReviews(startRowIndex, maximumRows, sortExpression);
		}

		public int SelectReviewCount()
		{
			return _helperDataAdapter.SelectReviewCount();
		}

		private static IQueryable<Entity> CreateReviewEntityQuery(OrganizationServiceContext serviceContext, EntityReference product)
		{
			return serviceContext.CreateQuery("adx_review")
				.Where(review => review.GetAttributeValue<EntityReference>("adx_product") == product)
				.Where(review => review.GetAttributeValue<OptionSetValue>("statecode") != null && review.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ReviewState.Active)
				.Where(review => review.GetAttributeValue<bool?>("adx_publishtoweb").GetValueOrDefault(false));
		}
	}
}
