/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Settings for displaying a map.
	/// </summary>
	public class MapConfiguration
	{
		private const string DefaultRestUrl = "https://dev.virtualearth.net/REST/v1/Locations";
		private string _restUrl;

		/// <summary>
		/// Unit of measure for the distance search
		/// </summary>
		public enum DistanceUnits
		{
			/// <summary>
			/// miles
			/// </summary>
			Miles = 756150001,
			/// <summary>
			/// km
			/// </summary>
			Km = 756150000
		}
		/// <summary>
		/// Indicates whether the map control is enabled or not.
		/// </summary>
		public bool Enabled { get; set; }
		/// <summary>
		/// Key used to authorize map service transactions.
		/// </summary>
		public string Credentials { get; set; }
		/// <summary>
		/// Gets or sets the REST URL for the map service. Default: https://dev.virtualearth.net/REST/v1/Locations 
		/// </summary>
		public string RestUrl
		{
			get
			{
				return string.IsNullOrWhiteSpace(_restUrl) ? DefaultRestUrl : _restUrl;
			}
			set
			{
				_restUrl = value;
			}
		}
		/// <summary>
		/// Latitude of the map's default center position.
		/// </summary>
		public double DefaultCenterLatitude { get; set; }
		/// <summary>
		/// Longitude of the map's default center position.
		/// </summary>
		public double DefaultCenterLongitude { get; set; }
		/// <summary>
		/// Default zoom level of the map. An integer value from 1 - 19 indicating the default zoom level of the map. Default: 12 
		/// </summary>
		public int DefaultZoom { get; set; }
		/// <summary>
		/// An integer value of the horizontal offset of the map pushpin infobox. Default: 25
		/// </summary>
		public int InfoboxOffsetX { get; set; }
		/// <summary>
		/// An integer value of the vertical offset of the map pushpin infobox. Default: 46 
		/// </summary>
		public int InfoboxOffsetY { get; set; }
		/// <summary>
		/// An integer value of the height of the map pushpin image. Default: 39 
		/// </summary>
		public int PinImageHeight { get; set; }
		/// <summary>
		/// An integer value of the width of the map pushpin image. Default: 32 
		/// </summary>
		public int PinImageWidth { get; set; }
		/// <summary>
		/// A URL to an image file to be used as the pushpin on the map. If none is specified, a default pin image will be used.
		/// </summary>
		public string PinImageUrl { get; set; }
		/// <summary>
		/// The unit of measure for distance values. One of the following, miles or Km. Default: miles 
		/// </summary>
		public DistanceUnits DistanceUnit { get; set; }

		/// <summary>
		/// List of integer values to be populated in the dropdown used in the web portal for selecting the distance to search for location on the map to. Note: The first value in the list is the default search distance used when initially rendering the map. Default: 5,10,25,50,100 
		/// </summary>
		public List<int> DistanceValues { get; set; }

		/// <summary>
		/// Attribute logical name of the latitude field on the target entity.
		/// </summary>
		/// <remarks>Recommended attribute schema settings. Type: Floating Point Number, Precision: 5, Min. Value: -90, Max. Value: 90, IME Mode: disabled</remarks>
		public string LatitudeFieldName { get; set; }
		/// <summary>
		/// Attribute logical name of the longitude field on the target entity.
		/// </summary>
		/// <remarks>Recommended attribute schema settings. Type: Floating Point Number, Precision: 5, Min. Value: -180, Max. Value: 180, IME Mode: disabled</remarks>
		public string LongitudeFieldName { get; set; }
		/// <summary>
		/// Attribute logical name on the target entity that represents the title displayed in the popup infobox for the map pushpin.
		/// </summary>
		public string InfoboxTitleFieldName { get; set; }
		/// <summary>
		/// Attribute logical name on the target entity that represents the description displayed in the popup infobox for the map pushpin.
		/// </summary>
		public string InfoboxDescriptionFieldName { get; set; }
		/// <summary>
		/// URL to handle the search request.
		/// </summary>
		public string SearchUrl { get; set; }

		/// <summary>
		/// Contstructor
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="restUrl"></param>
		/// <param name="searchUrl"></param>
		/// <param name="enabled"></param>
		/// <param name="defaultCenterLatitude"></param>
		/// <param name="defaultCenterLongitude"></param>
		/// <param name="defaultZoom"></param>
		/// <param name="infoboxOffsetX"></param>
		/// <param name="infoboxOffsetY"></param>
		/// <param name="pinImageHeight"></param>
		/// <param name="pinImageWidth"></param>
		/// <param name="pinImageUrl"></param>
		/// <param name="distanceUnit"></param>
		/// <param name="distanceValues"></param>
		public MapConfiguration(string credentials = null, string restUrl = null, string searchUrl = null, bool enabled = false,
			double defaultCenterLatitude = 0, double defaultCenterLongitude = 0, int defaultZoom = 12, int infoboxOffsetX = 25,
			int infoboxOffsetY = 46, int pinImageHeight = 39, int pinImageWidth = 32, string pinImageUrl = null,
			DistanceUnits distanceUnit = DistanceUnits.Miles, List<int> distanceValues = null)
		{
			Credentials = credentials;
			RestUrl = restUrl;
			SearchUrl = searchUrl;
			Enabled = enabled;
			DefaultCenterLatitude = defaultCenterLatitude;
			DefaultCenterLongitude = defaultCenterLongitude;
			DefaultZoom = defaultZoom;
			InfoboxOffsetX = infoboxOffsetX;
			InfoboxOffsetY = infoboxOffsetY;
			PinImageHeight = pinImageHeight;
			PinImageWidth = pinImageWidth;
			PinImageUrl = pinImageUrl;
			DistanceUnit = distanceUnit;
			DistanceValues = distanceValues;
		}

		public MapConfiguration(IPortalContext portalContext, bool mapEnabled, OptionSetValue mapDistanceUnit,
					  string mapDistanceValues, int? mapInfoboxOffsetY, int? mapInfoboxOffsetX, int? mapPushpinWidth,
					  string mapPushpinUrl, int? mapZoom, double? mapLongitude, double? mapLatitude, string mapRestUrl,
					  string mapCredentials, int? mapPushpinHeight)
		{
			Enabled = mapEnabled;

			if (!string.IsNullOrWhiteSpace(mapCredentials)) { Credentials = mapCredentials; } 

			RestUrl = mapRestUrl;
			DefaultCenterLatitude = mapLatitude ?? 0;
			DefaultCenterLongitude = mapLongitude ?? 0;
			DefaultZoom = mapZoom ?? 12;
			PinImageUrl = mapPushpinUrl;
			PinImageWidth = mapPushpinWidth ?? 32;
			PinImageHeight = mapPushpinHeight ?? 39;
			InfoboxOffsetX = mapInfoboxOffsetX ?? 25;
			InfoboxOffsetY = mapInfoboxOffsetY ?? 46;

			if (!string.IsNullOrWhiteSpace(mapDistanceValues))
			{
				var distanceItems = mapDistanceValues.Split(',');
				var distanceList = new List<int>();

				if (distanceItems.Any())
				{
					foreach (var distance in distanceItems)
					{
						int distanceValue;
						if (int.TryParse(distance, out distanceValue))
						{
							distanceList.Add(distanceValue);
						}
					}
				}
				else
				{
					distanceList = new List<int> { 5, 10, 25, 100 };
				}

				DistanceValues = distanceList;
			}

			if (mapDistanceUnit != null)
			{
				if (Enum.IsDefined(typeof(DistanceUnits), mapDistanceUnit.Value))
				{
					DistanceUnit = (DistanceUnits)mapDistanceUnit.Value;
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Distance Unit value '{0}' is not a valid value defined by MapConfiguration.DistanceUnits class.",
						mapDistanceUnit.Value));
				}
			}

			SearchUrl = WebsitePathUtility.ToAbsolute(portalContext.Website, "/EntityList/Map/Search/");
		}
	}
}
