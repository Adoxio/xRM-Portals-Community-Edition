/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Mapping;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Renders a Bing map.
	/// </summary>
	[Description("Renders a Bing map")]
	[ToolboxData(@"<{0}:BingMap runat=""server""></{0}:BingMap>")]
	[DefaultProperty("")]
	public class BingMap : CompositeControl
	{
		/// <summary>
		/// Credentials for Bing maps.
		/// </summary>
		[Description("Bing maps credentials.")]
		[DefaultValue("")]
		public string Credentials
		{
			get
			{
				return ((string)ViewState["Credentials"]) ?? string.Empty;
			}
			set
			{
				ViewState["Credentials"] = value;
			}
		}

		public MappingFieldMetadataCollection MappingFieldCollection { get; set; }
		
		/// <summary>
		/// Script files to be included
		/// </summary>
		protected virtual string[] ScriptIncludes
		{
			get
			{
				var list = new List<string>();
				var scripts = new[]
				{
					"https://www.bing.com/api/maps/mapcontrol",
					"~/xrm-adx/js/bingmap.js"
				};
				list.AddRange(scripts);
				return list.ToArray();
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();

			RegisterClientSideDependencies(this);

			ClientIDMode = ClientIDMode.Static;

			//here we will add hidden input fields containing the values of the MappingFieldsMetadataCollection
			var latitudeFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.LatitudeFieldName ?? "adx_latitude",
											ID = "geolocation_latitudefieldname"
										};

			Controls.Add(latitudeFieldName);

			var longitudeFieldName = new HiddenField()
										 {
											 Value = MappingFieldCollection.LongitudeFieldName ?? "adx_longitude",
											 ID = "geolocation_longitudefieldname"
										 };

			Controls.Add(longitudeFieldName);

			var addressLineFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.AddressLineFieldName ?? "adx_location_addressline",
											ID = "geolocation_addresslinefieldname"
										};

			Controls.Add(addressLineFieldName);

			var neighbourhoodFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.NeightbourhoodFieldName ?? "adx_location_neighorhood",
											ID = "geolocation_neighbourhoodfieldname"
										};

			Controls.Add(neighbourhoodFieldName);

			var cityFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.CityFieldName ?? "adx_location_city",
											ID = "geolocation_cityfieldname"
										};

			Controls.Add(cityFieldName);

			var countyFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.CountyFieldName ?? "adx_location_county",
											ID = "geolocation_countyfieldname"
										};

			Controls.Add(countyFieldName);

			var stateFieldName = new HiddenField()
										 {
											 Value = MappingFieldCollection.StateProvinceFieldName ?? "adx_location_stateorprovince",
											 ID = "geolocation_statefieldname"
										 };

			Controls.Add(stateFieldName);

			var countryFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.CountryFieldName ?? "adx_location_country",
											ID = "geolocation_countryfieldname"
										};

			Controls.Add(countryFieldName);

			var postalCodeFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.PostalCodeFieldName ?? "adx_location_postalcode",
											ID = "geolocation_portalcodefieldname"
										};

			Controls.Add(postalCodeFieldName);

			var formattedLocationFieldName = new HiddenField()
										{
											Value = MappingFieldCollection.FormattedLocationFieldName ?? "adx_location",
											ID = "geolocation_formattedlocationfieldname"
										};

			Controls.Add(formattedLocationFieldName);


			var bingMapsRestUrl = new HiddenField()
			{
				Value = MappingFieldCollection.BingMapsURL,
				ID = "bingmapsresturl"
			};

			Controls.Add(bingMapsRestUrl);

			var bingMapsCredentials = new HiddenField()
			{
				Value = MappingFieldCollection.BingMapsCredentials,
				ID = "bingmapscredentials"
			};

			Controls.Add(bingMapsCredentials);

		}

		/// <summary>
		/// Add the <see cref="ScriptIncludes"/> to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		/// <param name="control"></param>
		protected virtual void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				if (string.IsNullOrWhiteSpace(script))
				{
					continue;
				}

				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null)
				{
					continue;
				}

				var absolutePath = script.StartsWith("http", true, CultureInfo.InvariantCulture) ? script : VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}
	}
}
