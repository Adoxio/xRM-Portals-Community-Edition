/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Portal.Configuration;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Mapping
{
	public class MappingFieldMetadataCollection
	{
		public string PortalName { get; set; }

		public string LatitudeFieldName { get; set; }
		public string LongitudeFieldName { get; set; }
		public string AddressLineFieldName { get; set; }
		public string NeightbourhoodFieldName { get; set; }
		public string CityFieldName { get; set; }
		public string CountyFieldName { get; set; }
		public string StateProvinceFieldName { get; set; }
		public string CountryFieldName { get; set; }
		public string PostalCodeFieldName { get; set; }
		public string FormattedLocationFieldName { get; set; }

		public bool Enabled { get; set; }
		public bool DisplayMap { get; set; }

		public string BingMapsURL { 
			get
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

				var website = portal.Website;

				var context = portal.ServiceContext;

				var url = context.GetSiteSettingValueByName(website, "Bingmaps/restURL") ?? "https://dev.virtualearth.net/REST/v1/Locations/";

				return url;
			}
		}

		public string BingMapsCredentials
		{
			get
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

				var website = portal.Website;

				var context = portal.ServiceContext;

				var url = context.GetSiteSettingValueByName(website, "Bingmaps/credentials") ?? "AsdIaH5DkTDK6WatxqiPbYONvXCR6X_6kdbiV00XV3h7D3c9NhaeBBlyHOngsjji";

				return url;
			}
		}

	}
}
