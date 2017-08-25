/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Net
{
	/// <summary>
	/// Class used to interact with the Bing Maps REST API.
	/// </summary>
	public class BingMapLookup
	{
		private string BingMapRestUrl { get; set; }
		private string BingMapKey { get; set; }
		private string UserLocation { get; set; }
		private int? IncludeNeighborhood { get; set; }

		/// <summary>
		/// Parameterless initialization for calling the Bing Maps REST API that uses Settings records from within CRM when building the request URL.
		/// </summary>
		/// <remarks>Requires the following Settings from within CRM.
		/// BingMap/Key - (Required) Bing Maps Key used to authenticate with the Bing Maps REST API.
		/// BingMap/RestUrl - (Required) URL to the Bing Maps REST API.
		/// BingMap/UserLocation/Latitude - (Recommended) Latitude coordinate
		/// BingMap/UserLocation/Longitude - (Recommended) Longitude coordinate
		/// BingMap/IncludeNeighborhood - (Optional) (default = 0) | 0 - do not include the neighborhood with the address information in the response when it is available; 1 - include the neighborhood with the address information in the response when it is available.
		/// </remarks>
		public BingMapLookup()
		{
			const string bingMapKeySettingName = "BingMap/Key";
			const string bingMapUrlSettingName = "BingMap/RestUrl";
			const string bingMapUserLocationLatitudeSettingName = "BingMap/UserLocation/Latitude";
			const string bingMapUserLocationLongitudeSettingName = "BingMap/UserLocation/Longitude";
			const string bingMapIncludeNeighborhoodSettingName = "BingMap/IncludeNeighborhood";

			var context = new CrmOrganizationServiceContext();

			var setting = context.CreateQuery("adx_setting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name").EndsWith(bingMapKeySettingName));
			
			var key = string.Empty;

			if (setting != null)
			{
				key = setting.GetAttributeValue<string>("adx_value");
			}

			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ApplicationException(string.Format("No Bing Maps Key was specified in the setting {0}.", bingMapKeySettingName));
			}

			setting = context.CreateQuery("adx_setting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name").EndsWith(bingMapUrlSettingName));

			var restUrl = string.Empty;

			if (setting != null)
			{
				restUrl = setting.GetAttributeValue<string>("adx_value");
			}

			if (string.IsNullOrWhiteSpace(restUrl))
			{
				throw new ApplicationException(string.Format("No Bing Maps URL was specified in the setting {0}.", bingMapUrlSettingName));
			}

			var userLocation = string.Empty;
			var latitude = string.Empty;
			var longitude = string.Empty;

			setting = context.CreateQuery("adx_setting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name").EndsWith(bingMapUserLocationLatitudeSettingName));

			if (setting != null)
			{
				latitude = setting.GetAttributeValue<string>("adx_value");
			}

			setting = context.CreateQuery("adx_setting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name").EndsWith(bingMapUserLocationLongitudeSettingName));

			if (setting != null)
			{
				longitude = setting.GetAttributeValue<string>("adx_value");
			}

			if (!string.IsNullOrWhiteSpace(latitude) && !string.IsNullOrWhiteSpace(longitude))
			{
				userLocation = string.Format("{0},{1}", latitude, longitude);
			}

			setting = context.CreateQuery("adx_setting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name").EndsWith(bingMapIncludeNeighborhoodSettingName));

			var includeNeighborhood = 0;

			if (setting != null)
			{
				var value = setting.GetAttributeValue<string>("adx_value");

				int.TryParse(value, out includeNeighborhood);
			}

			BingMapRestUrl = restUrl;

			BingMapKey = key;

			UserLocation = userLocation;

			IncludeNeighborhood = includeNeighborhood;
		}

		/// <summary>
		/// Initialization for calling the Bing Maps REST API.
		/// </summary>
		/// <param name="bingMapRestUrl">The URL for making calls to the Bing Maps REST Services</param>
		/// <param name="bingMapKey">The Bing Maps Key to use to authenticate the request.</param>
		public BingMapLookup(string bingMapRestUrl, string bingMapKey)
		{
			if (string.IsNullOrWhiteSpace(bingMapRestUrl))
			{
				throw new ArgumentNullException("bingMapRestUrl");
			}

			if (string.IsNullOrWhiteSpace(bingMapKey))
			{
				throw new ArgumentNullException("bingMapKey");
			}

			BingMapKey = bingMapKey;

			BingMapRestUrl = bingMapRestUrl;
		}

		/// <summary>
		/// Initialization for calling the Bing Maps REST API.
		/// </summary>
		/// <param name="bingMapRestUrl">The URL for making calls to the Bing Maps REST Services</param>
		/// <param name="bingMapKey">The Bing Maps Key to use to authenticate the request.</param>
		/// <param name="userLocation">(Optional) The user’s current position. A point on the earth specified as a latitude and longitude. When you specify this parameter, the user’s location is taken into account and the results returned may be more relevant to the user. Example: userLocation=51.504360719046616,-0.12600176611298197</param>
		/// <param name="includeNeighborhood">(Optional) Specifies to include the neighborhood with the address information the response when it is available. One of the following values: 1=Include neighborhood information when available, 0=[default]Do not include neighborhood information.</param>
		public BingMapLookup(string bingMapRestUrl, string bingMapKey, string userLocation, int? includeNeighborhood)
		{
			if (string.IsNullOrWhiteSpace(bingMapRestUrl))
			{
				throw new ArgumentNullException("bingMapRestUrl");
			}

			if (string.IsNullOrWhiteSpace(bingMapKey))
			{
				throw new ArgumentNullException("bingMapKey");
			}

			BingMapRestUrl = bingMapRestUrl;

			BingMapKey = bingMapKey;

			UserLocation = userLocation;

			IncludeNeighborhood = includeNeighborhood ?? 0;
		}
		
		/// <summary>
		/// Method that occurs when validation is performed on the server.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void LocationServerValidate(object sender, ServerValidateEventArgs args)
		{
			var location = args.Value;

			if (string.IsNullOrWhiteSpace(location))
			{
				args.IsValid = false;

				return;
			}

			var context = new CrmOrganizationServiceContext();

			try
			{
				args.IsValid = Validate(context, location);
			}
			catch (Exception e)
			{
				args.IsValid = false;

				var validator = sender as CustomValidator;

				if (validator != null) validator.ErrorMessage = string.Format(ResourceManager.GetString("Geolocation_Query_Could_Not_Be_Completed"), BingMapRestUrl, e.Message);
			}
		}

		protected bool Validate(OrganizationServiceContext context, string location)
		{
			return IsLocationValid(location);
		}

		/// <summary>
		/// Create a web request to query the Bing Maps REST API and return the Bing Location Response.
		/// </summary>
		/// <param name="query">A string that contains information about a location, such as an address or landmark name.</param>
		/// <returns>BingLocationResponse</returns>
		public BingLocationResponse GetGeocodeLocationByQuery(string query)
		{
			BingLocationResponse result = null;

			var request = WebRequest.Create(string.Format("{0}?Key={1}&query={2}&userLocation={3}&inclnb={4}", BingMapRestUrl, BingMapKey, query, UserLocation, IncludeNeighborhood ?? 0)) as HttpWebRequest;

			
				if (request != null)
				{
					using (var response = (HttpWebResponse)request.GetResponse())
					{
						result = GetResult(response);
					}
				}
			

			return result;
		}

		private static BingLocationResponse GetResult(HttpWebResponse response)
		{
			BingLocationResponse result = null;

			if (response != null && response.StatusCode == HttpStatusCode.OK)
			{
				using (var stream = response.GetResponseStream())
				{
					var serialiser = new DataContractJsonSerializer(typeof(BingLocationResponse));

					if (stream != null)
					{
						result = serialiser.ReadObject(stream) as BingLocationResponse;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Create a web request to query the Bing Maps REST API and return if the location is valid or not.
		/// </summary>
		/// <param name="query">A string that contains information about a location, such as an address or landmark name.</param>
		/// <returns>True if location response indicates the location's coordinates could be resolved. Otherwise false.</returns>
		public virtual bool IsLocationValid(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return false;
			}

			var geocodeResult = GetGeocodeLocationByQuery(query);

			if (geocodeResult == null
				|| geocodeResult.resourceSets == null
				|| !geocodeResult.resourceSets.Any()
				|| geocodeResult.resourceSets.First().resources == null
				|| !geocodeResult.resourceSets.First().resources.Any()
				|| geocodeResult.resourceSets.First().resources.First().address == null
				|| geocodeResult.resourceSets.First().resources.First().point == null
				|| geocodeResult.resourceSets.First().resources.First().point.coordinates == null
				|| !geocodeResult.resourceSets.First().resources.First().point.coordinates.Any())
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Query the Bing Maps REST API and set the location coordinates and properties on the Entity provided.
		/// </summary>
		/// <param name="context">The runtime context of the data service that is used to track Microsoft Dynamics CRM entities and that sends and receives entities from the server. </param>
		/// <param name="entity">An instance of an entity (a record).</param>
		/// <param name="query">A string that contains information about a location, such as an address or landmark name.</param>
		/// <remarks>
		/// Entity must have the following string attributes; adx_latitude, adx_longitude, adx_city, adx_stateprovince, adx_country, adx_formattedaddress, adx_addressline, adx_county, adx_postalcode.
		/// </remarks>
		public virtual void SetGeoLocationCoordinates(OrganizationServiceContext context, Entity entity, string query)
		{
			var geocodeResult = GetGeocodeLocationByQuery(query);

			if (geocodeResult == null
				|| geocodeResult.resourceSets == null
				|| !geocodeResult.resourceSets.Any()
				|| geocodeResult.resourceSets.First().resources == null
				|| !geocodeResult.resourceSets.First().resources.Any()
				|| geocodeResult.resourceSets.First().resources.First().address == null
				|| geocodeResult.resourceSets.First().resources.First().point == null
				|| geocodeResult.resourceSets.First().resources.First().point.coordinates == null
				|| !geocodeResult.resourceSets.First().resources.First().point.coordinates.Any())
			{
				return;
			}

			var street = geocodeResult.resourceSets.First().resources.First().address.addressLine;
			var city = geocodeResult.resourceSets.First().resources.First().address.locality;
			var state = geocodeResult.resourceSets.First().resources.First().address.adminDistrict;
			var country = geocodeResult.resourceSets.First().resources.First().address.countryRegion;
			var formattedaddress = geocodeResult.resourceSets.First().resources.First().address.formattedAddress;
			var county = geocodeResult.resourceSets.First().resources.First().address.adminDistrict2;
			var postalcode = geocodeResult.resourceSets.First().resources.First().address.postalCode;

			decimal latitude;

			if (!decimal.TryParse(geocodeResult.resourceSets.First().resources.First().point.coordinates[0], out latitude))
			{
				return;
			}

			decimal longitude;

			if (!decimal.TryParse(geocodeResult.resourceSets.First().resources.First().point.coordinates[1], out longitude))
			{
				return;
			}

			try
			{
				entity.Attributes["adx_latitude"] = latitude;
				entity.Attributes["adx_longitude"] = longitude;
				entity.Attributes["adx_city"] = city;
				entity.Attributes["adx_stateprovince"] = state;
				entity.Attributes["adx_country"] = country;
				entity.Attributes["adx_formattedaddress"] = formattedaddress;
				entity.Attributes["adx_addressline"] = street;
				entity.Attributes["adx_county"] = county;
				entity.Attributes["adx_postalcode"] = postalcode;
				context.UpdateObject(entity);
				context.SaveChanges();
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("SetGeoLocationCoordinates", "{0}", ex.ToString()));
            }
		}
	}
}
