/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;

	/// <summary>
	/// Collection of <see cref="Measure"/> parsed from MeasureCollection XML defined in datadescription of a CRM chart visualization.
	/// </summary>
	public class MeasureCollection
	{
		/// <summary>
		/// Collection of <see cref="Measure"/> parsed from MeasureCollection XML.
		/// </summary>
		public ICollection<Measure> Measures { get; set; }

		/// <summary>
		/// Parse MeasureCollection XML into a <see cref="MeasureCollection"/>.
		/// </summary>
		/// <param name="element">The MeasureCollection XML element to be parsed.</param>
		/// <returns>The <see cref="MeasureCollection"/> result of the parse.</returns>
		public static MeasureCollection Parse(XElement element)
		{
			return new MeasureCollection
			{
				Measures = element.Elements("measure").Select(Measure.Parse).ToList()
			};
		}
	}
}
