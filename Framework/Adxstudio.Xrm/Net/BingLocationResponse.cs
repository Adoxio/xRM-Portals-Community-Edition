/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Net
{
	/// <summary>
	/// Class is used to deserialize the JSON response from the Bing Maps REST API.
	/// http://msdn.microsoft.com/en-us/library/ff701707.aspx
	/// </summary>
	[DataContract]
	public class BingLocationResponse
	{
		/// <summary>
		/// A status code that offers additional information about authentication success or failure.
		/// </summary>
		[DataMember]
		public string authenticationResultCode { get; set; }

		/// <summary>
		/// A URL that references a brand image to support contractual branding requirements.
		/// </summary>
		[DataMember]
		public string brandLogoUri { get; set; }
		
		/// <summary>
		/// A copyright notice.
		/// </summary>
		[DataMember]
		public string copyright { get; set; }

		/// <summary>
		/// A collection of ResourceSet objects. A ResourceSet is a container of Resources returned by the request. For more information, see the ResourceSet section below.
		/// </summary>
		[DataMember]
		public ResourceSet[] resourceSets { get; set; }

		/// <summary>
		/// The HTTP Status code for the request.
		/// </summary>
		[DataMember]
		public string statusCode { get; set; }
		
		/// <summary>
		/// A description of the HTTP status code.
		/// </summary>
		[DataMember]
		public string statusDescription { get; set; }
		
		/// <summary>
		/// A unique identifier for the request.
		/// </summary>
		[DataMember]
		public string traceId { get; set; }

		/// <summary>
		/// A ResourceSet is a container of Resources returned by the request.
		/// </summary>
		[DataContract]
		public class ResourceSet
		{
			/// <summary>
			/// An estimate of the total number of resources in the ResourceSet.
			/// </summary>
			[DataMember]
			public int estimatedTotal { get; set; }

			/// <summary>
			/// A collection of one or more resources. The resources that are returned depend on the request.
			/// http://msdn.microsoft.com/en-us/library/ff701725.aspx
			/// </summary>
			[DataMember]
			public Resource[] resources { get; set; }

			/// <summary>
			/// A Location resource containing location information that corresponds to the values provided in the request. 
			/// </summary>
			[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1", Name = "Location")]
			public class Resource
			{
				/// <summary>
				/// The type of request.
				/// </summary>
				[DataMember]
				public string __type { get; set; }

				/// <summary>
				/// A geographic area that contains the location. A bounding box contains SouthLatitude, WestLongitude, NorthLatitude, and EastLongitude values in units of degrees.
				/// </summary>
				[DataMember]
				public double[] bbox { get; set; }

				/// <summary>
				/// The name of the resource.
				/// </summary>
				[DataMember]
				public string name { get; set; }

				/// <summary>
				/// The latitude and longitude coordinates of the location.
				/// </summary>
				[DataMember]
				public Point point { get; set; }

				/// <summary>
				/// The latitude and longitude coordinates of the location.
				/// </summary>
				[DataContract]
				public class Point
				{
					/// <summary>
					/// The type of point.
					/// </summary>
					[DataMember]
					public string type { get; set; }

					/// <summary>
					/// The latitude and longitude coordinates of the location. The coordinates are double values that are separated by commas and are specified in the following order; Latitude, Longitude
					/// </summary>
					[DataMember]
					public string[] coordinates { get; set; }
				}

				/// <summary>
				/// The postal address for the location. An address can contain AddressLine, Neighborhood, Locality, AdminDistrict, AdminDistrict2, CountryRegion, FormattedAddress, PostalCode, and Landmark fields.
				/// </summary>
				[DataMember]
				public Address address { get; set; }

				/// <summary>
				/// The postal address for the location.
				/// </summary>
				[DataContract]
				public class Address
				{
					/// <summary>
					/// The official street line of an address relative to the area, as specified by the Locality, or PostalCode, properties.
					/// </summary>
					[DataMember]
					public string addressLine { get; set; }

					/// <summary>
					/// A string specifying the subdivision name in the country or region for an address. This element is typically treated as the first order administrative subdivision, but in some cases it is the second, third, or fourth order subdivision in a country, dependency, or region.
					/// </summary>
					[DataMember]
					public string adminDistrict { get; set; }

					/// <summary>
					/// A string specifying the subdivision name in the country or region for an address. This element is used when there is another level of subdivision information for a location, such as the county.
					/// </summary>
					[DataMember]
					public string adminDistrict2 { get; set; }

					/// <summary>
					/// A string specifying the country or region name of an address.
					/// </summary>
					[DataMember]
					public string countryRegion { get; set; }

					/// <summary>
					/// A string specifying the complete address. This address may not include the country or region.
					/// </summary>
					[DataMember]
					public string formattedAddress { get; set; }

					/// <summary>
					/// A string specifying the populated place for the address. This typically refers to a city, but may refer to a suburb or a neighborhood in certain countries or regions.
					/// </summary>
					[DataMember]
					public string locality { get; set; }

					/// <summary>
					/// A string specifying the post code, postal code, or ZIP Code of an address.
					/// </summary>
					[DataMember]
					public string postalCode { get; set; }

					/// <summary>
					/// A string specifying the neighborhood for an address.
					/// </summary>
					[DataMember]
					public string neighborhood { get; set; }

					/// <summary>
					/// A string specifying the name of the landmark when there is a landmark associated with an address.
					/// </summary>
					[DataMember]
					public string landmark { get; set; }
				}

				/// <summary>
				/// The level of confidence that the geocoded location result is a match. One of the following values: High, Medium, Low.
				/// </summary>
				[DataMember]
				public string confidence { get; set; }

				/// <summary>
				/// The classification of the geographic entity returned, such as Address. For a list of entity types, see http://msdn.microsoft.com/en-us/library/ff728811.aspx
				/// </summary>
				[DataMember]
				public string entityType { get; set; }
			}
		}
	}
}
