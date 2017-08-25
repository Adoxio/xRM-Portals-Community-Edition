/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Enumeration of the state of the product record.
	/// </summary>
	public enum ProductState
	{
		/// <summary>
		/// Record is active.
		/// </summary>
		Active = 0,
		/// <summary>
		/// Record is not active.
		/// </summary>
		InActive = 1
	}

	/// <summary>
	/// Provides data operations on a given set of products.
	/// </summary>
	public interface IProductAggregationDataAdapter
	{
		/// <summary>
		/// Select products.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IProduct> SelectProducts();

		/// <summary>
		/// Select products.
		/// </summary>
		/// <param name="startRowIndex">Starting row index to begin selecting products.</param>
		/// <param name="maximumRows">Maximum number rows of records to return</param>
		/// <returns></returns>
		IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows);

		/// <summary>
		/// Selects the total number of products.
		/// </summary>
		/// <returns></returns>
		int SelectProductCount();
	}

	public interface ISortableProductAggregationDataAdapter : IProductAggregationDataAdapter
	{
		IEnumerable<IProduct> SelectProducts(string sortExpression);

		IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows, string sortExpression);

		int SelectProductCount(string sortExpression);
	}

	public interface IFilterableProductAggregationDataAdapter : ISortableProductAggregationDataAdapter
	{
		IEnumerable<IProduct> SelectProducts(string brand, string sortExpression);

		IEnumerable<IProduct> SelectProducts(string brand, int? rating, string sortExpression);

		IEnumerable<IProduct> SelectProducts(string brand, int? rating);

		IEnumerable<IProduct> SelectProducts(string brand, int startRowIndex, int maximumRows, string sortExpression);

		IEnumerable<IProduct> SelectProducts(string brand, int? rating, int startRowIndex, int maximumRows, string sortExpression);

		int SelectProductCount(string brand, string sortExpression);

		int SelectProductCount(string brand, int? rating, string sortExpression);

		int SelectProductCount(string brand, int? rating);
	}
}
