/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Provides data operations for a single product, as represented by a Product entity.
	/// </summary>
	public class ProductDataAdapter : IProductDataAdapter
	{
		internal enum StateCode
		{
			Active = 0,
			Inactive = 1
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ProductDataAdapter(EntityReference product, IDataAdapterDependencies dependencies)
		{
			if (product == null) throw new ArgumentNullException("product");
			if (product.LogicalName != "product") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), product.LogicalName), "product");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Product = product;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ProductDataAdapter(Entity product, IDataAdapterDependencies dependencies) : this(product.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ProductDataAdapter(IProduct product, IDataAdapterDependencies dependencies) : this(product.Entity, dependencies) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ProductDataAdapter(EntityReference product, string portalName = null) : this(product, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ProductDataAdapter(Entity product, string portalName = null) : this(product, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="product">Product Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ProductDataAdapter(IProduct product, string portalName = null) : this(product, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected EntityReference Product { get; set; }

		public virtual IProduct Select()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var product = serviceContext.GetProduct(Product.Id);
			
			return product == null ? null : new ProductFactory(serviceContext, Dependencies.GetPortalUser(), Dependencies.GetWebsite()).Create(new[] { product }).FirstOrDefault();
		}

		public IEnumerable<ISalesLiterature> SelectSalesLiterature(string name)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var product = serviceContext.GetProduct(Product.Id);

			return product.GetRelatedEntities(serviceContext, "productsalesliterature_association")
				.Where(e =>
					e.GetAttributeValue<bool?>("iscustomerviewable").GetValueOrDefault() &&
					e.GetAttributeValue<string>("name") == name)
				.ToArray()
				.Select(e => new SalesLiterature(e, serviceContext.GetEntityMetadata(e.LogicalName, EntityFilters.Attributes)))
				.OrderBy(e => e.Name)
				.ToArray();
		}

		public virtual IEnumerable<ISalesAttachment> SelectSalesAttachments(SalesLiteratureTypeCode type)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var salesAttachments = Enumerable.Empty<ISalesAttachment>();
			var product = serviceContext.GetProduct(Product.Id);

			var salesLiterature = product.GetRelatedEntities(serviceContext, "productsalesliterature_association")
				.Where(e =>
					e.GetAttributeValue<bool?>("iscustomerviewable").GetValueOrDefault() &&
					e.GetAttributeValue<OptionSetValue>("literaturetypecode") != null &&
					e.GetAttributeValue<OptionSetValue>("literaturetypecode").Value == (int)type)
				.ToList();

			if (!salesLiterature.Any())
			{
				return salesAttachments;
			}

			return salesLiterature.Select(literature => SelectSalesAttachments(literature.ToEntityReference()))
						.Aggregate(salesAttachments, (current, attachments) => current.Union(attachments))
						.OrderBy(e => e.FileName);
		}
		
		public virtual IEnumerable<ISalesAttachment> SelectSalesAttachments(EntityReference salesLiterature)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			return serviceContext.CreateQuery("salesliteratureitem")
				.Where(e => e.GetAttributeValue<EntityReference>("salesliteratureid") == salesLiterature)
				.ToArray()
				.Select(e => new SalesAttachment(e, website))
				.OrderBy(e => e.FileName)
				.ToArray();
		}

		public virtual IEnumerable<IProductImageGalleryNode> SelectImageGalleryNodes()
		{
			var salesAttachments = SelectSalesAttachments(SalesLiteratureTypeCode.ImageGallery).Where(o => o.HasFile);
			var nodes =
				from attachment in salesAttachments 
				group attachment by attachment.Title
				into g
				let thumbnail = g.FirstOrDefault(o => o.Keywords != null && o.Keywords.Contains("thumbnail"))
				let image = g.FirstOrDefault(o => o.Keywords == null || !o.Keywords.Contains("thumbnail"))
				orderby image.Title
				select new ProductImageGalleryNode(image, thumbnail, image == null ? string.Empty : image.URL, thumbnail == null ? string.Empty : thumbnail.URL);

			return nodes;
		}

		public virtual void CreateReview(string title, string content, double rating, int maximumRatingValue, string reviewerName,
									string reviewerLocation, string reviewerEmail, bool recommend)
		{
			var context = Dependencies.GetServiceContext();
			var httpContext = Dependencies.GetRequestContext().HttpContext;
			var reviewerContact = Dependencies.GetPortalUser();

			title.ThrowOnNullOrWhitespace("title", ResourceManager.GetString("Value_Null_In_Create_Review_Exception"));
			reviewerName.ThrowOnNullOrWhitespace("reviewerName", ResourceManager.GetString("Value_Null_In_Create_Review_Exception"));
			if (!httpContext.Request.IsAuthenticated)
			{
				reviewerEmail.ThrowOnNullOrWhitespace("reviewerEmail", ResourceManager.GetString("Value_Null_In_Anonymous_Review_Exception"));
			}
			if (rating < 1)
			{
				throw new ArgumentNullException("rating", ResourceManager.GetString("Must_Be_Greater_Than_Zero_Exception"));
			}

			var review = new Entity("adx_review");
			review["adx_title"] = title;
			review["adx_name"] = title;
			if (!string.IsNullOrWhiteSpace(content))
			{
				review["adx_content"] = content;
			}
			review["adx_rating"] = rating;
			if (maximumRatingValue < 1)
			{
				maximumRatingValue = 5;
			}
			review["adx_ratingrationalvalue"] = rating / maximumRatingValue;
			review["adx_maximumvalue"] = maximumRatingValue;
			review["adx_recommend"] = recommend;
			review["adx_submittedon"] = DateTime.UtcNow;
			review["adx_createdbyusername"] = httpContext.Request.IsAuthenticated ? httpContext.User.Identity.Name : httpContext.Request.AnonymousID;
			// review["adx_createdbyipaddress"] = httpContext.Request.UserHostAddress;
			if (reviewerContact != null)
			{
				review["adx_reviewercontact"] = reviewerContact;
				//var contact = new Entity("contact") {Id = reviewerContact.Id};
				//contact["nickname"] = reviewerName;
				//if (!context.IsAttached(contact))
				//{
				//	context.Attach(contact);
				//}
				//context.UpdateObject(contact);
			}
			review["adx_reviewername"] = reviewerName;
			if (!httpContext.Request.IsAuthenticated)
			{
				review["adx_revieweremail"] = reviewerEmail;
			}
			if (!string.IsNullOrWhiteSpace(reviewerLocation))
			{
				review["adx_reviewerlocation"] = reviewerLocation;
			}
			if (Product != null)
			{
				review["adx_product"] = Product;
			}
			context.AddObject(review);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.ProductReview, HttpContext.Current, "create_product_review", 1, review.ToEntityReference(), "create");
			}

		}

		/// <summary>
		/// Indicates if a user has reviewed the product.
		/// </summary>
		/// <param name="user"></param>
		public bool HasReview(EntityReference user)
		{
			if (user == null) throw new ArgumentNullException("user");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}:{1}", user.LogicalName, user.Id));

			var serviceContext = Dependencies.GetServiceContext();
			var existingReview = SelectReview(serviceContext, user);

			var hasReview = existingReview != null;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}:{1}, {2}", user.LogicalName, user.Id, hasReview));

			return hasReview;
		}

		/// <summary>
		/// Indicates if an anonymous user has reviewed the product.
		/// </summary>
		/// <param name="username"></param>
		public bool HasReview(string username)
		{

			var serviceContext = Dependencies.GetServiceContext();
			var existingReview = SelectReview(serviceContext, username);

			var hasReview = existingReview != null;
            

			return hasReview;
		}

		protected Entity SelectReview(OrganizationServiceContext serviceContext, EntityReference user)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (user == null) throw new ArgumentNullException("user");

			return serviceContext.CreateQuery("adx_review")
				.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_reviewercontact") == user
					&& e.GetAttributeValue<EntityReference>("adx_product") == Product);
		}

		protected Entity SelectReview(OrganizationServiceContext serviceContext, string username)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			
			return serviceContext.CreateQuery("adx_review")
				.FirstOrDefault(e => e.GetAttributeValue<string>("adx_createdbyusername") == username
					&& e.GetAttributeValue<EntityReference>("adx_product") == Product);
		}
	}
}
