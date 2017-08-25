/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Provides data operations on a set of products for a given subject.
	/// </summary>
	public class SubjectProductsDataAdapter : IFilterableProductAggregationDataAdapter
	{
		private readonly ProductAggregationDataAdapter _helperDataAdapter;

		public SubjectProductsDataAdapter(EntityReference subject, IDataAdapterDependencies dependencies)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (subject.LogicalName != "subject") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), subject.LogicalName), "subject");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			_helperDataAdapter = new ProductAggregationDataAdapter(
				dependencies,
				(serviceContext, brand, rating) => GetProductCount(subject, serviceContext, brand, rating),
				serviceContext => CreateProductEntityQuery(subject, serviceContext));
		}

		public IEnumerable<IProduct> SelectProducts()
		{
			return _helperDataAdapter.SelectProducts();
		}

		public IEnumerable<IProduct> SelectProducts(string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows)
		{
			return _helperDataAdapter.SelectProducts(startRowIndex, maximumRows);
		}

		public IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows, string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(startRowIndex, maximumRows, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(brand, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating, string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(brand, rating, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating)
		{
			return _helperDataAdapter.SelectProducts(brand, rating);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int startRowIndex, int maximumRows, string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(brand, startRowIndex, maximumRows, sortExpression);
		}

		public IEnumerable<IProduct> SelectProducts(string brand, int? rating, int startRowIndex, int maximumRows, string sortExpression)
		{
			return _helperDataAdapter.SelectProducts(brand, rating, startRowIndex, maximumRows, sortExpression);
		}

		public int SelectProductCount()
		{
			return _helperDataAdapter.SelectProductCount();
		}

		public int SelectProductCount(string sortExpression)
		{
			return _helperDataAdapter.SelectProductCount(sortExpression);
		}
		
		public int SelectProductCount(string brand, string sortExpression)
		{
			return _helperDataAdapter.SelectProductCount(brand, sortExpression);
		}

		public int SelectProductCount(string brand, int? rating, string sortExpression)
		{
			return _helperDataAdapter.SelectProductCount(brand, rating, sortExpression);
		}

		public int SelectProductCount(string brand, int? rating)
		{
			return _helperDataAdapter.SelectProductCount(brand, rating);
		}

		private static IQueryable<Entity> CreateProductEntityQuery(EntityReference subject, OrganizationServiceContext serviceContext)
		{
			return serviceContext.CreateQuery("product")
				.Where(product => product.GetAttributeValue<EntityReference>("subjectid") == subject)
				.Where(product => product.GetAttributeValue<OptionSetValue>("statecode") != null && product.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ProductState.Active);
		}

		private static int GetProductCount(EntityReference subject, OrganizationServiceContext serviceContext, string brand, int? rating)
		{
			Guid brandId;

			if (Guid.TryParse(brand, out brandId))
			{
				return rating.HasValue
					? serviceContext.FetchSubjectBrandRatingProductCount(subject.Id, brandId, Convert.ToDouble(rating.Value), null)
					: serviceContext.FetchSubjectBrandProductCount(subject.Id, brandId);
			}

			return rating.HasValue
				? serviceContext.FetchSubjectRatingProductCount(subject.Id, Convert.ToDouble(rating.Value), null)
				: serviceContext.FetchSubjectProductCount(subject.Id);
		}
	}
}
