/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Feedback;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Cms
{
	public class RatingDataAdapter : IRatingDataAdapter
	{
		public RatingDataAdapter(EntityReference rateableReference, IDataAdapterDependencies dependencies)
		{
			if (rateableReference == null)
			{
				throw new ArgumentNullException("rateableReference");
			}

			LogicalName = rateableReference.LogicalName;

			RateableReference = rateableReference;

			Dependencies = dependencies;
		}

		public RatingDataAdapter(Entity rateable, IDataAdapterDependencies dependencies) 
			: this(rateable.ToEntityReference(), dependencies) { }

		public RatingDataAdapter(EntityReference rateableReference, string portalName = null)
			: this(rateableReference, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public RatingDataAdapter(Entity rateable, string portalName = null) 
			: this(rateable.ToEntityReference(), new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference RateableReference { get; set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected IRatingInfo RatingInfo { get; set; }

		private string LogicalName { get; set; }

		public virtual bool RatingsEnabled
		{
			get { return RateableReference != null; }
		}

		public virtual IRating SelectUserRating()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var user = Dependencies.GetPortalUser();

			if (user == null) return null;

			var query = serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName)
				.Where(rating => rating.GetAttributeValue<EntityReference>("regardingobjectid").Id == RateableReference.Id);

			query = query.Where(rating => rating.GetAttributeValue<string>("regardingobjecttypecode") == RateableReference.LogicalName);

			//If any extended data needs to be fetched, do so here

			var entities = query.Where(rating => rating.GetAttributeValue<EntityReference>(FeedbackMetadataAttributes.UserIdAttributeName) == user);

			var userRating = entities.FirstOrDefault();

			if (userRating == null) return null;

			return new Rating(userRating);
		}

		public IRating SelectVisitorRating(string visitorID)
		{
			if (string.IsNullOrEmpty(visitorID)) return null;

			var serviceContext = Dependencies.GetServiceContext();

			var query = serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName)
				.Where(rating => rating.GetAttributeValue<EntityReference>("regardingobjectid").Id == RateableReference.Id);

			query = query.Where(rating => rating.GetAttributeValue<string>("regardingobjecttypecode") == RateableReference.LogicalName);

			//If any extended data needs to be fetched, do so here

			var entities = query.Where(rating => rating.GetAttributeValue<string>(FeedbackMetadataAttributes.VisitorAttributeName) == visitorID);

			var visitorRating = entities.FirstOrDefault();

			if (visitorRating == null) return null;

			return new Rating(visitorRating);
		}

		public IEnumerable<IRating> SelectRatings()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var query = serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName)
				.Where(rating => rating.GetAttributeValue<EntityReference>("regardingobjectid").Id == RateableReference.Id && rating.GetAttributeValue<int?>(FeedbackMetadataAttributes.RatingValueAttributeName) != null);

			query = query.Where(rating => rating.GetAttributeValue<string>("regardingobjecttypecode") == RateableReference.LogicalName);

			//If any extended data needs to be fetched, do so here

			var entities = query.ToArray();

			return entities.Select(e => new Rating(e)).ToArray();
		}

		public virtual IRatingInfo GetRatingInfo()
		{
			if (RatingInfo != null) return RatingInfo;

			var ratings = SelectRatings();
			int yesCount = 0, noCount = 0, totalCount = 0, ratingSum = 0;
			double averageRating = 0.0f;
			foreach (var rating in ratings)
			{
				var value = rating.Value;

				if (value == 0) { noCount++; }
				if (value == 1) { yesCount++; }

				totalCount++;

				ratingSum = ratingSum + value;

				averageRating = (double)ratingSum / (double)totalCount;
			}

			RatingInfo = new RatingInfo(yesCount, noCount, averageRating, totalCount, ratingSum);

			return RatingInfo;
			
		}

		public void SaveRating(int rating, int maxRating, int minRating)
		{
			var userRating = SelectUserRating();

			if (userRating == null)
			{
				var newRatingEntity = new Entity(FeedbackMetadataAttributes.RatingEntityName);

				var user = Dependencies.GetPortalUser();

				newRatingEntity.Attributes[FeedbackMetadataAttributes.UserIdAttributeName] = user;

				AddRating(newRatingEntity, rating, maxRating, minRating);
			}
			else //update existing rating
			{
				var existingRating = userRating.Entity;

				var serviceContext = Dependencies.GetServiceContext();

				var updatingRating =
					serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName).FirstOrDefault(r => r.GetAttributeValue<Guid?>(FeedbackMetadataAttributes.ActivityIdAttributeName) == existingRating.Id);

				UpdateRating(rating, maxRating, minRating, serviceContext, updatingRating);
			}
		}

		public void SaveRating(int rating, int maxRating, int minRating, string visitorID)
		{
			if (Dependencies.GetPortalUser() == null)
			{
				var visitorRating = SelectVisitorRating(visitorID);

				if (visitorRating == null)
				{
					var newRatingEntity = new Entity(FeedbackMetadataAttributes.RatingEntityName);

					if (!string.IsNullOrEmpty(visitorID))
					{
						newRatingEntity.Attributes[FeedbackMetadataAttributes.VisitorAttributeName] = visitorID;
					}

					AddRating(newRatingEntity, rating, maxRating, minRating);
				}
				else
				{
					var existingRating = visitorRating.Entity;

					var serviceContext = Dependencies.GetServiceContext();

					var updatingRating =
						serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName).FirstOrDefault(r => r.GetAttributeValue<Guid?>(FeedbackMetadataAttributes.ActivityIdAttributeName) == existingRating.Id);

					UpdateRating(rating, maxRating, minRating, serviceContext, updatingRating);
				}
			}
			else
			{
				SaveRating(rating, maxRating, minRating);
			}
		}

		public void AddRating(IRating rating)
		{
			AddRating(rating.Entity, rating.Value, rating.MaximumValue, rating.MinimumValue);
		}

		public void AddRating(Entity entity, int rating, int maxRating, int minRating)
		{
			var serviceContext = Dependencies.GetServiceContext();

			entity.Attributes["regardingobjectid"] = RateableReference;
			entity.Attributes[FeedbackMetadataAttributes.RatingValueAttributeName] = rating;
			entity.Attributes[FeedbackMetadataAttributes.MaxRatingAttributeName] = maxRating;
			entity.Attributes[FeedbackMetadataAttributes.MinRatingAttributeName] = minRating;
			entity.Attributes["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			var entityMetadata = GetRelatedEntityMetadata(serviceContext);

			var title = string.Empty;
			var displayName = string.Empty;

			if (entityMetadata != null)
			{
				displayName = entityMetadata.DisplayName.UserLocalizedLabel.Label;

				var relatedEntity = GetRelatedEntity(serviceContext, entityMetadata.PrimaryIdAttribute);
				title = relatedEntity.GetAttributeValue<string>(entityMetadata.PrimaryNameAttribute);
			}

			entity.Attributes["title"] = ResourceManager.GetString("Feedback_Default_Title").FormatWith(displayName, title);

			serviceContext.AddObject(entity);
			serviceContext.SaveChanges();
		}

		public void DeleteUserRating(string visitorID)
		{
			if (Dependencies.GetPortalUser() == null)
			{
				var visitorRating = SelectVisitorRating(visitorID);

				if (visitorRating == null)
				{
					//No rating found.  Is this an error?
				}
				else
				{
					var existingRating = visitorRating.Entity;

					DeleteRating(existingRating);
				}
			}
			else
			{
				var userRating = SelectUserRating();

				if (userRating == null)
				{
					//No rating found.
				}
				else //update existing rating
				{
					var existingRating = userRating.Entity;

					DeleteRating(existingRating);
				}
			}
		}

		public void DeleteRating(Entity entity)
		{
			var serviceContext = Dependencies.GetServiceContext();

			var updatingRating =
				serviceContext.CreateQuery(FeedbackMetadataAttributes.RatingEntityName).FirstOrDefault(
					r => r.GetAttributeValue<Guid?>(FeedbackMetadataAttributes.ActivityIdAttributeName) == entity.Id);

			serviceContext.DeleteObject(updatingRating);
			serviceContext.SaveChanges();
		}

		private void UpdateRating(int rating, int maxRating, int minRating, OrganizationServiceContext serviceContext, Entity existingRating)
		{
			existingRating.Attributes[FeedbackMetadataAttributes.RatingValueAttributeName] = rating;
			existingRating.Attributes[FeedbackMetadataAttributes.MaxRatingAttributeName] = maxRating;
			existingRating.Attributes[FeedbackMetadataAttributes.MinRatingAttributeName] = minRating;

			if (!serviceContext.IsAttached(existingRating))
			{
				serviceContext.Attach(existingRating);
			}

			serviceContext.UpdateObject(existingRating);

			serviceContext.SaveChanges();
		}

		/// <summary>
		/// Retrieve EntityMetadata
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <returns>EntityMetadata</returns>
		private EntityMetadata GetRelatedEntityMetadata(OrganizationServiceContext serviceContext)
		{
			var metadataRequest = new RetrieveEntityRequest()
			{
				EntityFilters = EntityFilters.All,
				LogicalName = LogicalName,
				RetrieveAsIfPublished = false
			};

			return ((RetrieveEntityResponse)serviceContext.Execute(metadataRequest)).EntityMetadata;
		}

		/// <summary>
		/// Retrieve Entity
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="primaryIdAttribute">PrimaryIdAttribute Name</param>
		/// <returns>Entity</returns>
		private Entity GetRelatedEntity(OrganizationServiceContext serviceContext, string primaryIdAttribute)
		{
			var entity =
				serviceContext.CreateQuery(RateableReference.LogicalName)
					.FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryIdAttribute) == RateableReference.Id);

			return entity;
		}
	}
}
