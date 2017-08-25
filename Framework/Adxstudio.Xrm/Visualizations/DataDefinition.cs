/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Defines the data description of a CRM chart visualization (savedqueryvisualization).
	/// </summary>
	public class DataDefinition
	{
		/// <summary>
		/// Collection of <see cref="Fetch"/>
		/// </summary>
		public ICollection<Fetch> FetchCollection { get; set; }

		/// <summary>
		/// Collection of <see cref="Category"/>
		/// </summary>
		public ICollection<Category> CategoryCollection { get; set; }

		/// <summary>
		/// Parse data description XML string into a DataDefinition object.
		/// </summary>
		/// <param name="text">The data description XML string to be parsed.</param>
		/// <returns>The <see cref="DataDefinition"/> result from parsing the XML string.</returns>
		public static DataDefinition Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		/// <summary>
		/// Parse data description XML into a DataDefinition object.
		/// </summary>
		/// <param name="element">The data description XML element to be parsed.</param>
		/// <returns>The <see cref="DataDefinition"/> result from parsing the XML.</returns>
		public static DataDefinition Parse(XElement element)
		{
			if (element == null)
			{
				return null;
			}

			var fetchCollectionElement = element.Element("fetchcollection");

			if (fetchCollectionElement == null)
			{
				return null;
			}

			var fetchElements = fetchCollectionElement.Elements("fetch");

			var categoryCollectionElement = element.Element("categorycollection");

			if (categoryCollectionElement == null)
			{
				return null;
			}

			var categoryElements = categoryCollectionElement.Elements("category");

			var categoryCollection = categoryElements.Select(Category.Parse).ToList();

			var fetchCollection = fetchElements.Select(Fetch.Parse).ToList();

			foreach (var fetch in fetchCollection)
			{
				// DateGrouping require us to also retrieve a datetime value so we can format series labels correctly. 
				// Grouping by day for example with a date like 9/23/2016 will result in the value 23 to be stored in the data.
				// We must manually add this aggregate attribute into the fetch so we get the actual date value 9/23/2016 displayed in the chart series.
				var dateGroupingAttributes = fetch.Entity.Attributes.Where(a => a.DateGrouping != null && a.DateGrouping.Value == DateGroupingType.Day).ToArray();
				foreach (var attribute in dateGroupingAttributes)
				{
					fetch.Entity.Attributes.Add(new FetchAttribute(attribute.Name,
						string.Format("{0}_dategroup_value", attribute.Alias), AggregateType.Max));
				}
			}

			return new DataDefinition
			{
				CategoryCollection = categoryCollection,
				FetchCollection = fetchCollection
			};
		}
	}
}
