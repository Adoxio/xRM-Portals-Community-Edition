/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Describes details needed to be able to build a chart from CRM chart visualizations.
	/// </summary>
	public interface IChartBuilder
	{
		/// <summary>
		/// A definition of a CRM chart visualization (savedqueryvisualization).
		/// </summary>
		CrmChart ChartDefinition { get; set; }

		/// <summary>
		/// An optional definition of a CRM view (savedquery) that if present is used to merge the FetchXML into the chart's FetchXML.
		/// </summary>
		CrmView ViewDefinition { get; set; }

		/// <summary>
		/// The FetchXML query to be executed to retrieve the data to be plotted in the chart. If <see cref="ViewDefinition"/> has been specified, this FetchXML will be the result of a merge of the chart's <see cref="Fetch"/> and view's <see cref="Fetch"/>.
		/// </summary>
		Fetch Query { get; set; }
	}
}
