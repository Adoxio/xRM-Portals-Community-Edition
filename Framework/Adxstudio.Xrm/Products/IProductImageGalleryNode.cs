/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Products
{
	public interface IProductImageGalleryNode
	{
		ISalesAttachment ImageSalesAttachment { get; }

		string ImageURL { get; }

		ISalesAttachment ThumbnailImageSalesAttachment { get; }

		string ThumbnailImageURL { get; }

		string Title { get; }
	}
}
