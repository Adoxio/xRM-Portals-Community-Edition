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
	/// Category parsed from Category XML defined in datadescription of a CRM chart visualization.
	/// </summary>
	public class Category
	{
		/// <summary>
		/// Optionally specified alias for the category.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Collection of measures associated with the category.
		/// </summary>
		public ICollection<MeasureCollection> MeasureCollections { get; set; }

		/// <summary>
		/// Parse Category XML into a <see cref="Category"/>.
		/// </summary>
		/// <param name="element">The Category XML element to be parsed.</param>
		/// <returns>The <see cref="Category"/> result of the parse.</returns>
		public static Category Parse(XElement element)
		{
			return new Category
			{
				Alias = element.GetAttribute("alias"),
				MeasureCollections = element.Elements("measurecollection").Select(MeasureCollection.Parse).ToList()
			};
		}
	}
}
