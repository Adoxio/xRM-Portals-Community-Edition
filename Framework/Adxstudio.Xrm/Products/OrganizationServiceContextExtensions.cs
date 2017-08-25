/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// OrganizationServiceContext extension methods
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		/// <summary>
		/// Get product by ID.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="productId">Unique ID of the product record</param>
		/// <returns>Product entity record</returns>
		public static Entity GetProduct(this OrganizationServiceContext context, Guid productId)
		{
			if (context == null) throw new ArgumentNullException("context");

			var product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == productId);

			return product;
		}

		/// <summary>
		/// Retrieve the product count for a specified subject
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="subjectId">Subject ID</param>
		/// <returns>Product count</returns>
		public static int FetchSubjectProductCount(this OrganizationServiceContext serviceContext, Guid subjectId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""subjectid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""subjectid"" operator=""eq"" />
						</filter>
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						</filter>
					</entity>
				</fetch>");

			var subjectIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='subjectid']");

			if (subjectIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the subjectid filter element.");
			}

			subjectIdAttribute.SetAttributeValue("value", subjectId.ToString());

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return (int)response.EntityCollection.Entities.First().GetAttributeValue<AliasedValue>("count").Value;
		}

		/// <summary>
		/// Retrieve the product count for a specified campaign
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="campaignId">Campaign ID</param>
		/// <returns>Product count</returns>
		public static int FetchCampaignProductCount(this OrganizationServiceContext serviceContext, Guid campaignId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""productid"" aggregate=""count"" alias=""count"" />
						<link-entity name=""campaignitem"" from=""entityid"" to=""productid"" visible=""false"" link-type=""outer"">
							<filter type=""and"">
								<condition attribute=""campaignid"" operator=""eq"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");

			var campaignIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='campaignid']");

			if (campaignIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the campaignid filter element.");
			}

			campaignIdAttribute.SetAttributeValue("value", campaignId.ToString());

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return (int)response.EntityCollection.Entities.First().GetAttributeValue<AliasedValue>("count").Value;
		}

		/// <summary>
		/// Retrieve the product count for a specified brand.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="brandId">The Product (product) brand (adx_brandid).</param>
		/// <returns></returns>
		public static int FetchBrandProductCount(this OrganizationServiceContext serviceContext, Guid brandId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""productid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_brand"" operator=""eq"" />
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						</filter>
					</entity>
				</fetch>");

			var vendorNameAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_brand']");

			if (vendorNameAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_brand filter"));
			}

			vendorNameAttribute.SetAttributeValue("value", brandId.ToString());

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.First().GetAttributeAliasedValue<int>("count");
		}

		/// <summary>
		/// Retrieve the product count for a specified subject and brand.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="subjectId">The subject ID (subjectid).</param>
		/// <param name="brandId">The Product (product) brand (adx_brandid).</param>
		/// <returns></returns>
		public static int FetchSubjectBrandProductCount(this OrganizationServiceContext serviceContext, Guid subjectId, Guid brandId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""productid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""subjectid"" operator=""eq"" />
							<condition attribute=""adx_brand"" operator=""eq"" />
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						</filter>
					</entity>
				</fetch>");

			var subjectIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='subjectid']");

			if (subjectIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the subjectid filter element.");
			}

			subjectIdAttribute.SetAttributeValue("value", subjectId.ToString());

			var brandIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_brand']");

			if (brandIdAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_ratingaverage gte filter"));
			}

			brandIdAttribute.SetAttributeValue("value", brandId);

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.First().GetAttributeAliasedValue<int>("count");
		}

		/// <summary>
		/// Retrieve the product count for a specified subject and brand.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="subjectId">The subject ID (subjectid).</param>
		/// <param name="brandId">The Product (product) brand (adx_brandid).</param>
		/// <param name="minRating">The minimum average rating (adx_ratingaverage, inclusive) for products to be counted.</param>
		/// <param name="maxRating">The maximum average rating (adx_ratingaverage, exclusive) for products to be counted.</param>
		/// <returns></returns>
		public static int FetchSubjectBrandRatingProductCount(this OrganizationServiceContext serviceContext, Guid subjectId, Guid brandId, double? minRating, double? maxRating)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""productid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""subjectid"" operator=""eq"" />
							<condition attribute=""adx_brand"" operator=""eq"" />
							<condition attribute=""adx_ratingaverage"" operator=""ge"" />
							<condition attribute=""adx_ratingaverage"" operator=""lt"" />
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						</filter>
					</entity>
				</fetch>");

			var subjectIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='subjectid']");

			if (subjectIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the subjectid filter element.");
			}

			subjectIdAttribute.SetAttributeValue("value", subjectId.ToString());

			var brandIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_brand']");

			if (brandIdAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_brand filter"));
			}

			brandIdAttribute.SetAttributeValue("value", brandId);

			var minRatingAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_ratingaverage' and @operator='ge']");

			if (minRatingAttribute == null)
			{
				throw new InvalidOperationException(ResourceManager.GetString("ADX_Ratingaveragegte_Filter_Element_Select_Exception"));
			}

			if (minRating.HasValue)
			{
				minRatingAttribute.SetAttributeValue("value", minRating.ToString());
			}
			else
			{
				minRatingAttribute.Remove();
			}

			var maxRatingAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_ratingaverage' and @operator='lt']");

			if (maxRatingAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_ratingaverage lt"));
			}
			
			if (maxRating.HasValue)
			{
				maxRatingAttribute.SetAttributeValue("value", maxRating.ToString());
			}
			else
			{
				maxRatingAttribute.Remove();
			}
			
			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.First().GetAttributeAliasedValue<int>("count");
		}

		/// <summary>
		/// Retrieve the product count for a specified subject and brand.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="subjectId">The subject ID (subjectid).</param>
		/// <param name="minRating">The minimum average rating (adx_ratingaverage, inclusive) for products to be counted.</param>
		/// <param name="maxRating">The maximum average rating (adx_ratingaverage, exclusive) for products to be counted.</param>
		/// <returns></returns>
		public static int FetchSubjectRatingProductCount(this OrganizationServiceContext serviceContext, Guid subjectId, double? minRating, double? maxRating)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""product"">
						<attribute name=""productid"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""subjectid"" operator=""eq"" />
							<condition attribute=""adx_ratingaverage"" operator=""ge"" />
							<condition attribute=""adx_ratingaverage"" operator=""lt"" />
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						</filter>
					</entity>
				</fetch>");

			var subjectIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='subjectid']");

			if (subjectIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the subjectid filter element.");
			}

			subjectIdAttribute.SetAttributeValue("value", subjectId.ToString());

			var minRatingAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_ratingaverage' and @operator='ge']");

			if (minRatingAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_ratingaverage gte filter"));
			}

			if (minRating.HasValue)
			{
				minRatingAttribute.SetAttributeValue("value", minRating.ToString());
			}
			else
			{
				minRatingAttribute.Remove();
			}

			var maxRatingAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_ratingaverage' and @operator='lt']");

			if (maxRatingAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Unable to select {0} element.", "adx_ratingaverage lt"));
			}
			
			if (maxRating.HasValue)
			{
				maxRatingAttribute.SetAttributeValue("value", maxRating.ToString());
			}
			else
			{
				maxRatingAttribute.Remove();
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return response.EntityCollection.Entities.First().GetAttributeAliasedValue<int>("count");
		}

		/// <summary>
		/// Retrieve the review count for a specified product
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="productId">Product ID</param>
		/// <returns>Review count</returns>
		public static int FetchProductReviewCount(this OrganizationServiceContext serviceContext, Guid productId)
		{
			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""adx_review"">
						<attribute name=""adx_product"" aggregate=""count"" alias=""count"" />
						<filter type=""and"">
							<condition attribute=""adx_product"" operator=""eq"" />
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<condition attribute=""adx_publishtoweb"" operator=""eq"" value=""1"" />
						</filter>
					</entity>
				</fetch>");

			var productIdAttribute = fetchXml.XPathSelectElement("//condition[@attribute='adx_product']");

			if (productIdAttribute == null)
			{
				throw new InvalidOperationException("Unable to select the adx_product filter element.");
			}

			productIdAttribute.SetAttributeValue("value", productId.ToString());

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			return (int)response.EntityCollection.Entities.First().GetAttributeValue<AliasedValue>("count").Value;
		}
	}
}
