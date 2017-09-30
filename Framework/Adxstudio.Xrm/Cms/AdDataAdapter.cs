/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Web;
	using System.Linq;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;

	public class AdDataAdapter : IAdDataAdapter
	{
		public const string AdRoute = "Ad";
		public const string PlacementRoute = "AdPlacement";
		public const string RandomAdRoute = "RandomAd";

		public AdDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public IAd SelectAd(Guid adId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", adId));

			var ad = SelectAd(e => e.GetAttributeValue<Guid>("adx_adid") == adId);

			if (ad == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", adId));

			return ad;
		}

		public IAd SelectAd(string adName)
		{
			if (string.IsNullOrEmpty(adName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var ad = SelectAd(e => e.GetAttributeValue<string>("adx_name") == adName);

			if (ad == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return ad;
		}

		protected virtual IAd SelectAd(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var publishingStateAccessProvider = new PublishingStateAccessProvider(Dependencies.GetRequestContext().HttpContext);

			// Bulk-load all ad entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_ad")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.ToArray();

			var entity = allEntities.FirstOrDefault(e =>
				match(e)
				&& IsActive(e)
				&& publishingStateAccessProvider.TryAssert(serviceContext, e));

			if (entity == null)
			{
				return null;
			}

			var ad = CreateAd(entity, serviceContext);

			return ad;
		}

		public IAdPlacement SelectAdPlacement(Guid adPlacementId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}",
				adPlacementId));

			var adPlacement = SelectAdPlacement(e => e.GetAttributeValue<Guid>("adx_adplacementid") == adPlacementId);

			if (adPlacement == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}",
				adPlacementId));

			return adPlacement;
		}

		public IAdPlacement SelectAdPlacement(string adPlacementName)
		{
			if (string.IsNullOrEmpty(adPlacementName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var adPlacement = SelectAdPlacement(e => e.GetAttributeValue<string>("adx_name") == adPlacementName);

			if (adPlacement == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

            return adPlacement;
		}

		protected virtual IAdPlacement SelectAdPlacement(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			// Bulk-load all ad placement entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_adplacement")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.ToArray();

			var entity = allEntities.FirstOrDefault(e => match(e) && IsActive(e));

			if (entity == null)
			{
				return null;
			}

			var publishingStateAccessProvider = new PublishingStateAccessProvider(Dependencies.GetRequestContext().HttpContext);

			var ads = entity.GetRelatedEntities(serviceContext, new Relationship("adx_adplacement_ad"))
				.Where(e => publishingStateAccessProvider.TryAssert(serviceContext, e))
				.Where(IsActive)
				.Select(e => CreateAd(e, serviceContext));

			return new AdPlacement(entity, ads);
		}

		private static IAd CreateAd(Entity entity, OrganizationServiceContext serviceContext)
		{
			var note = entity.GetRelatedEntities(serviceContext, "adx_ad_Annotations").FirstOrDefault();
			return new Ad(entity, note);
		}

		public IAd SelectRandomAd(Guid adPlacementId)
		{
			var placement = SelectAdPlacement(adPlacementId);
			return placement == null ? null : SelectRandomAd(placement);
		}

		public IAd SelectRandomAd(string adPlacementName)
		{
			var placement = SelectAdPlacement(adPlacementName);
			return placement == null ? null : SelectRandomAd(placement);
		}

		protected IAd SelectRandomAd(IAdPlacement placement)
		{
			if (placement == null)
			{
				return null;
			}

			var array = placement.Ads.ToArray();

			if (array.Length == 0) return null;

			var random = new Random(DateTime.Now.Millisecond);
			return array[random.Next(0, array.Length)];
		}

		private static bool IsActive(Entity entity)
		{
			if (entity == null)
			{
				return false;
			}

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			var expirationDate = entity.GetAttributeValue<DateTime?>("adx_expirationdate");

			return statecode != null && statecode.Value == 0 && (expirationDate == null || expirationDate > DateTime.UtcNow);
		}
	}
}
