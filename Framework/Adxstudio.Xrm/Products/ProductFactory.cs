/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Products
{
	class ProductFactory
	{
		private readonly EntityReference _portalUser;
		private readonly EntityReference _website;
		private readonly OrganizationServiceContext _serviceContext;

		public ProductFactory(OrganizationServiceContext serviceContext, EntityReference portalUser, EntityReference website)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			_serviceContext = serviceContext;
			_portalUser = portalUser;
			_website = website;
		}

		public IEnumerable<IProduct> Create(IEnumerable<Entity> productEntities)
		{
			var products = productEntities.ToArray();

			return
				products.Select(
					e =>
					new Product(e, _serviceContext.GetEntityMetadata(e.LogicalName, EntityFilters.Attributes), GetProductPricingInfo(e), GetProductImageSalesAttachment(e),
					            GetProductImageThumbnailSalesAttachment(e))).ToArray();
		}

		private IEnumerable<ISalesAttachment> SelectSalesAttachments(Entity product, SalesLiteratureTypeCode type)
		{
			var salesAttachments = Enumerable.Empty<ISalesAttachment>();
			
			var salesLiterature = product.GetRelatedEntities(_serviceContext, "productsalesliterature_association")
				.Where(s =>
					s.GetAttributeValue<bool?>("iscustomerviewable").GetValueOrDefault() &&
					s.GetAttributeValue<OptionSetValue>("literaturetypecode") != null &&
					s.GetAttributeValue<OptionSetValue>("literaturetypecode").Value == (int)type)
				.ToArray();

			if (!salesLiterature.Any())
			{
				return salesAttachments;
			}

			var salesliteratureitems = salesLiterature
				.Select(literature => SelectSalesAttachments(literature.ToEntityReference()))
				.Aggregate(salesAttachments, (current, attachments) => current.Union(attachments))
				.OrderBy(e => e.FileName);

			return salesliteratureitems;
		}

		private IEnumerable<ISalesAttachment> SelectSalesAttachments(EntityReference salesLiterature)
		{
			return _serviceContext.CreateQuery("salesliteratureitem")
				.Where(e => e.GetAttributeValue<EntityReference>("salesliteratureid") != null && e.GetAttributeValue<EntityReference>("salesliteratureid").Id == salesLiterature.Id)
				.ToArray()
				.Select(e => new SalesAttachment(e, _website))
				.OrderBy(e => e.FileName)
				.ToArray();
		}

		private ISalesAttachment GetProductImageSalesAttachment(Entity product)
		{
			return GetSalesAttachment(product, SalesLiteratureTypeCode.ImageGallery, "primary fullsize");
		}

		private ISalesAttachment GetProductImageThumbnailSalesAttachment(Entity product)
		{
			return GetSalesAttachment(product, SalesLiteratureTypeCode.ImageGallery, "primary thumbnail");
		}

		private ISalesAttachment GetSalesAttachment(Entity product, SalesLiteratureTypeCode type, string keywords)
		{
			var attachment = SelectSalesAttachments(product, type).FirstOrDefault(e => e.Keywords != null && e.Keywords.Contains(keywords));

			return attachment;
		}

		private IProductPricingInfo GetProductPricingInfo(Entity product)
		{
			var pricingInfo = new ProductPricingInfo(0, string.Empty, false, 0, 0);

			// Determine the name of the price list to be used to get the product price

			var priceListName = _serviceContext.GetDefaultPriceListName(_website != null ? _website.Id : Guid.Empty);

			pricingInfo.PriceListName = priceListName;

			if (_website == null)
			{
				return pricingInfo;
			}

			var website = _serviceContext.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == _website.Id);
			
			if (website == null)
			{
				return pricingInfo;
			}

			var price = _serviceContext.GetProductPriceByPriceListName(product, website, priceListName) ?? new Money(0);

			pricingInfo.Price = pricingInfo.RegularPrice = price.Value;

			return pricingInfo;
		}
	}
}
