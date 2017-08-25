/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents full, extended info about a product.
	/// </summary>
	public class Product : IProduct
	{
		/// <summary>
		/// Product initialization
		/// </summary>
		/// <param name="product">Product entity record</param>
		/// <param name="productMetadata">Product entity metadata</param>
		/// <param name="pricingInfo">Product Pricing Info</param>
		/// <param name="primaryImageAttachment">Primary Image Sales Attachment</param>
		/// <param name="thumbnailImageAttachment">Thumbnail Image Sales Attachment</param>
		public Product(Entity product, EntityMetadata productMetadata, IProductPricingInfo pricingInfo, ISalesAttachment primaryImageAttachment, ISalesAttachment thumbnailImageAttachment)
		{
			if (product == null) throw new ArgumentNullException("product");
			if (productMetadata == null) throw new ArgumentNullException("productMetadata");
			if (product.LogicalName != "product") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), product.LogicalName), "product");

			Brand = product.GetAttributeValue<EntityReference>("adx_brand");
			Currency = product.GetAttributeValue<EntityReference>("transactioncurrencyid");
			DefaultPriceList = product.GetAttributeValue<EntityReference>("priceleveid");
			DefaultUnit = product.GetAttributeValue<EntityReference>("defaultuomid");
			Entity = product;
			EntityReference = product.ToEntityReference();
			ImageURL = primaryImageAttachment == null ? string.Empty : primaryImageAttachment.URL;
			ImageThumbnailURL = thumbnailImageAttachment == null ? string.Empty : thumbnailImageAttachment.URL;
			PricingInfo = pricingInfo;
			Subject = product.GetAttributeValue<EntityReference>("subjectid");
			UnitGroup = product.GetAttributeValue<EntityReference>("defaultuomscheduleid");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Product, HttpContext.Current, "create_note", 1, product.ToEntityReference(), "create");
			}
		}

		public EntityReference Brand { get; private set; }
		public string BrandName
		{
			get { return Brand.Name; }
		}
		public EntityReference Currency { get; private set; }
		public decimal CurrentPrice
		{
			get { return PricingInfo.Price; }
		}
		public bool CurrentUserCanWriteReview { get { return true; } }
		public EntityReference DefaultPriceList { get; private set; }
		public string DefaultPriceListName
		{
			get { return DefaultPriceList == null ? string.Empty : DefaultPriceList.Name; }
		}
		public EntityReference DefaultUnit { get; private set; }
		public string Description
		{
			get { return Entity.GetAttributeValue<string>("description"); }
		}
		public Entity Entity { get; private set; }
		public EntityReference EntityReference { get; private set; }
		public string ImageURL { get; private set; }
		public string ImageThumbnailURL { get; private set; }
		public bool IsInStock
		{
			get { return QuantityOnHand > 0; }
		}
		public decimal ListPrice
		{
			get
			{
				var listPrice = Entity.GetAttributeValue<Money>("price");
				return listPrice == null ? 0 : listPrice.Value;
			}
		}
		public string ModelNumber
		{
			get { return Entity.GetAttributeValue<string>("adx_modelnumber"); }
		}

		public string Name
		{
			get { return Entity.GetAttributeValue<string>("name"); }
		}
		public string PartialURL
		{
			get { return Entity.GetAttributeValue<string>("adx_partialurl"); }
		}
		public IProductPricingInfo PricingInfo { get; private set; }
		public decimal QuantityOnHand
		{
			get { return Entity.GetAttributeValue<decimal>("quantityonhand"); }
		}
		public IProductRatingInfo RatingInfo
		{
			get
			{
				return new ProductRatingInfo(Entity.GetAttributeValue<double?>("adx_ratingaverage").GetValueOrDefault(0),
					Entity.GetAttributeValue<double?>("adx_ratingaveragerationalvalue").GetValueOrDefault(0),
					Entity.GetAttributeValue<int?>("adx_ratingcount").GetValueOrDefault(0),
					Entity.GetAttributeValue<int?>("adx_ratingmaximumvalue").GetValueOrDefault(0),
					Entity.GetAttributeValue<double?>("adx_ratingsum").GetValueOrDefault(0));
			}
		}
		public DateTime ReleaseDate { get; private set; }
		public bool RequiresSpecialInstructions
		{
			get { return Entity.GetAttributeValue<bool?>("adx_requiresspecialinstructions").GetValueOrDefault(false); }
		}
		public string SKU
		{
			get { return Entity.GetAttributeValue<string>("productnumber"); }
		}
		public string SpecialInstructions
		{
			get { return Entity.GetAttributeValue<string>("adx_specialinstructions"); }
		}
		public string Specifications
		{
			get { return Entity.GetAttributeValue<string>("adx_specifications"); }
		}
		public decimal StockVolume
		{
			get { return Entity.GetAttributeValue<decimal>("stockvolume"); }
		}
		public decimal StockWeight
		{
			get { return Entity.GetAttributeValue<decimal>("stockweight"); }
		}
		public EntityReference Subject { get; private set; }
		public EntityReference UnitGroup { get; private set; }
	}
}
