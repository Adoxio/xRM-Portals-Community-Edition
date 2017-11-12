/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Mapping
{
	/// <summary>
	/// Helper methods on the <see cref="OrganizationServiceContext"/> class.
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		/// <summary>
		/// Returns Entities according to a distance calculation - works on a prexisting query.
		/// </summary>
		/// <param name="query">The Query to perform the filter on.</param>
		/// <param name="originLatitude">Origin Latitude to calculate distance against.</param>
		/// <param name="originLongitude">Origin Longitude to calculate distance against.</param>
		/// <param name="entityLongitudeAttributeName">Attribute name on target entity.</param>
		/// <param name="entityLatitudeAttributeName">Attribute name on target entity.</param>
		/// <param name="maxDistance">Maximum distance from point of origin to location.</param>
		/// <param name="units">Unit of measure (Kilometers or Miles)</param>
		public static IEnumerable<Entity> DistanceQuery(this IQueryable<Entity> query, double originLatitude, double originLongitude, 
														string entityLongitudeAttributeName, string entityLatitudeAttributeName, int maxDistance, GeoHelpers.Units units)
		{
			//http://en.wikipedia.org/wiki/Spherical_law_of_cosines
			// Distance = acos(SIN(lat1)*SIN(lat2)+COS(lat1)*COS(lat2)*COS(lon2-lon1))*6371
			//convert degrees to radians - multiply degrees by p/180

			var earthRadius = units == GeoHelpers.Units.Kilometers
								? GeoHelpers.EarthRadiusInKilometers
								: GeoHelpers.EarthRadiusInMiles;

			var returnEnumerable = from c in query.ToList()
								   let latitude = c.GetAttributeValue<double>(entityLatitudeAttributeName)
								   let longitude = c.GetAttributeValue<double>(entityLongitudeAttributeName)
								   let distance =
									   earthRadius * 2 *
									   Math.Asin(
										   Math.Sqrt(Math.Pow(Math.Sin((Math.Abs(originLatitude) - Math.Abs(latitude)) * Math.PI / 180 / 2), 2) +
													 Math.Cos(Math.Abs(originLatitude) * Math.PI / 180) * Math.Cos(Math.Abs(latitude) * Math.PI / 180) *
													 Math.Pow(Math.Sin((Math.Abs(originLongitude) - Math.Abs(longitude)) * Math.PI / 180 / 2), 2)))
								   where distance < maxDistance
								   orderby distance
								   select c;

			return returnEnumerable;
		}

		/// <summary>
		/// Returns Entities according to a distance calculation and includes the computed distance in the result.
		/// </summary>
		/// <param name="query">The Query to perform the filter on.</param>
		/// <param name="originLatitude">Origin Latitude to calculate distance against.</param>
		/// <param name="originLongitude">Origin Longitude to calculate distance against.</param>
		/// <param name="entityLongitudeAttributeName">Attribute name on target entity.</param>
		/// <param name="entityLatitudeAttributeName">Attribute name on target entity.</param>
		/// <param name="maxDistance">Maximum distance from point of origin to location.</param>
		/// <param name="units">Unit of measure (Kilometers or Miles)</param>
		public static IEnumerable<Tuple<Entity, double>> DistanceQueryWithResult(this IEnumerable<Entity> query, double originLatitude, double originLongitude,
														string entityLongitudeAttributeName, string entityLatitudeAttributeName, int maxDistance, GeoHelpers.Units units)
		{
			//http://en.wikipedia.org/wiki/Spherical_law_of_cosines
			// Distance = acos(SIN(lat1)*SIN(lat2)+COS(lat1)*COS(lat2)*COS(lon2-lon1))*6371
			//convert degrees to radians - multiply degrees by p/180

			var earthRadius = units == GeoHelpers.Units.Kilometers
								? GeoHelpers.EarthRadiusInKilometers
								: GeoHelpers.EarthRadiusInMiles;

			var returnEnumerable = from c in query
								   let latitude = c.GetAttributeValue<double>(entityLatitudeAttributeName)
								   let longitude = c.GetAttributeValue<double>(entityLongitudeAttributeName)
								   let distance =
									   earthRadius * 2 *
									   Math.Asin(
										   Math.Sqrt(Math.Pow(Math.Sin((Math.Abs(originLatitude) - Math.Abs(latitude)) * Math.PI / 180 / 2), 2) +
													 Math.Cos(Math.Abs(originLatitude) * Math.PI / 180) * Math.Cos(Math.Abs(latitude) * Math.PI / 180) *
													 Math.Pow(Math.Sin((Math.Abs(originLongitude) - Math.Abs(longitude)) * Math.PI / 180 / 2), 2)))
								   where distance < maxDistance
								   orderby distance
								   select new Tuple<Entity, double>(c, distance);

			return returnEnumerable;
		}

		/// <summary>
		/// Filter by comparison of origin latitude and longitude coordinates against the bounding box coordinates.
		/// </summary>
		/// <param name="fetch">Entity fetch.</param>
		/// <param name="originLatitude">Origin Latitude to calculate distance against.</param>
		/// <param name="originLongitude">Origin Longitude to calculate distance against.</param>
		/// <param name="entityLongitudeAttributeName">Longitude attribute name on target entity.</param>
		/// <param name="entityLatitudeAttributeName">Latitude attribute name on target entity.</param>
		/// <param name="entityLongLatAttributeType">Attribute type either 'double' or 'decimal'.</param>
		/// <param name="distance">Maximum distance from point of origin to location.</param>
		/// <param name="units">Unit of measure (Kilometers or Miles).</param>
		/// <returns></returns>
		public static Fetch FilterByBoxDistance(this Fetch fetch, double originLatitude, double originLongitude,
			string entityLongitudeAttributeName, string entityLatitudeAttributeName,
			string entityLongLatAttributeType, int distance, GeoHelpers.Units units)
		{
			//get the bounding box
			var earthRadius = units == GeoHelpers.Units.Kilometers ? GeoHelpers.EarthRadiusInKilometers : GeoHelpers.EarthRadiusInMiles;
			var angularDistance = distance / (double)earthRadius; // angular distance on a great circle
			var originLatitudeRadians = GeoHelpers.DegreesToRadians(originLatitude);
			var originLongitudeRadians = GeoHelpers.DegreesToRadians(originLongitude);
			var minLatitudeRadians = originLatitudeRadians - angularDistance;
			var maxLatitudeRadians = originLatitudeRadians + angularDistance;
			var deltaLongitude = Math.Asin(Math.Sin(angularDistance) / Math.Cos(originLatitudeRadians));
			var minLongitudeRadians = originLongitudeRadians - deltaLongitude;
			var maxLongitudeRadians = originLongitudeRadians + deltaLongitude;
			var minLatitude = GeoHelpers.RadiansToDegrees(minLatitudeRadians);
			var maxLatitude = GeoHelpers.RadiansToDegrees(maxLatitudeRadians);
			var minLongitude = GeoHelpers.RadiansToDegrees(minLongitudeRadians);
			var maxLongitude = GeoHelpers.RadiansToDegrees(maxLongitudeRadians);

			if (entityLongLatAttributeType.ToLower() == "double")
			{
				fetch.Entity.Filters.Add(new Filter
				{
					Conditions = new[]
					{
						new Condition(entityLatitudeAttributeName, ConditionOperator.GreaterThan, minLatitude),
						new Condition(entityLatitudeAttributeName, ConditionOperator.LessThan, maxLatitude),
						new Condition(entityLongitudeAttributeName, ConditionOperator.LessThan, maxLongitude),
						new Condition(entityLongitudeAttributeName, ConditionOperator.GreaterThan, minLongitude)  
					},
					Type = LogicalOperator.And
				});
			}
			else if (entityLongLatAttributeType.ToLower() == "decimal")
			{
				fetch.Entity.Filters.Add(new Filter
				{
					Conditions = new[]
					{
						new Condition(entityLatitudeAttributeName, ConditionOperator.GreaterThan, (decimal)minLatitude),
						new Condition(entityLatitudeAttributeName, ConditionOperator.LessThan, (decimal)maxLatitude),
						new Condition(entityLongitudeAttributeName, ConditionOperator.LessThan, (decimal)maxLongitude),
						new Condition(entityLongitudeAttributeName, ConditionOperator.GreaterThan, (decimal)minLongitude)
					},
					Type = LogicalOperator.And
				});
			}

			return fetch;
		}
	}
}
