/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Provides data operations for a single product.
	/// </summary>
	public interface IProductDataAdapter
	{
		/// <summary>
		/// Selects the attributes of the product.
		/// </summary>
		IProduct Select();

		/// <summary>
		/// Selects the <see cref="ISalesLiterature">sales literature</see> with a given name associated with the product. 
		/// </summary>
		/// <param name="name">Name of a sales literature record</param>
		IEnumerable<ISalesLiterature> SelectSalesLiterature(string name);
		
		/// <summary>
		/// Selects the <see cref="ISalesAttachment">sales attachments</see> for a specified sales literature associated with the product.
		/// </summary>
		/// <param name="salesLiterature">Sales Literature Entity Reference</param>
		IEnumerable<ISalesAttachment> SelectSalesAttachments(EntityReference salesLiterature);

		/// <summary>
		/// Selects the <see cref="ISalesAttachment">sales attachments</see> for a sales literature record with specified Literature Type Code the associated with the product.
		/// </summary>
		/// <param name="literatureTypeCode">Literature Type Code</param>
		IEnumerable<ISalesAttachment> SelectSalesAttachments(SalesLiteratureTypeCode literatureTypeCode);

		/// <summary>
		/// Selects the <see cref="IProductImageGalleryNode">product image gallery nodes</see> associated with the product via sales attachments related to a product's sales literature record with a literature type code equal to 'Image Gallery (756150000)'.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IProductImageGalleryNode> SelectImageGalleryNodes();

		/// <summary>
		/// Create a review
		/// </summary>
		/// <param name="title">Title of the review</param>
		/// <param name="content">The review content body</param>
		/// <param name="rating">Rating</param>
		/// <param name="maximumRatingValue">Rational value of the rating</param>
		/// <param name="reviewerName">Name (Nickname) of the reviewer</param>
		/// <param name="reviewerLocation">Location of the reviewer</param>
		/// <param name="reviewerEmail">Email of the reviewer</param>
		/// <param name="recommend">Whether or not the reviewer would recommend the item being reviewed</param>
		/// <returns></returns>
		void CreateReview(string title, string content, double rating, int maximumRatingValue, string reviewerName, string reviewerLocation, string reviewerEmail, bool recommend);
	}
}
