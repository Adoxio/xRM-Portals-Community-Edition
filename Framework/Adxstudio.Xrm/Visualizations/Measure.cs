/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System.Xml.Linq;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Measure parsed from Measure XML defined in datadescription of a CRM chart visualization.
	/// </summary>
	public class Measure
	{
		/// <summary>
		/// The specific alias for the measure that will correspond with an alias of an attribute in the FetchXml query.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Parse Measure XML into a <see cref="Measure"/>.
		/// </summary>
		/// <param name="element">The measure XML element to be parsed.</param>
		/// <returns>The <see cref="Measure"/> object result of the parse.</returns>
		public static Measure Parse(XElement element)
		{
			return new Measure
			{
				Alias = element.GetAttribute("alias")
			};
		}
	}
}
