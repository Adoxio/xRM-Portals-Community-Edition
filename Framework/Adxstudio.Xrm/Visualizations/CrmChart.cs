/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;

	/// <summary>
	/// Definition of a CRM chart visualization (savedqueryvisualization).
	/// </summary>
	public class CrmChart
	{
		/// <summary>
		/// Regex pattern for chart definition XML option set placeholder values.
		/// </summary>
		private static readonly Regex OptionSetPatternRegex = new Regex(@"^o:(?<optionSetName>\w+),(?<optionSetValue>\d+)$", RegexOptions.CultureInvariant);

		/// <summary>
		/// Current langauge code (LCID) for this chart definition.
		/// </summary>
		private readonly int languageCode;

		/// <summary>
		/// Information that indicates what data to retrieve and how it is categorized for the series.
		/// </summary>
		public DataDefinition DataDescription { get; set; }

		/// <summary>
		/// Raw XML of the chart data description that is parsed into <see cref="DataDescription"/>
		/// </summary>
		public string DataDescriptionXml { get; set; }

		/// <summary>
		/// Additional information that describes the chart.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// FetchXml query to be executed to retrieve the data for the chart.
		/// </summary>
		public Fetch Fetch { get; set; }

		/// <summary>
		/// Unique identifier of the chart.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Name of the chart.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Raw XML of the chart presentation description that is a serialization of the .Net Chart Control.
		/// </summary>
		public string PresentationDescriptionXml { get; set; }

		/// <summary>
		/// Gets the <see cref="EntityMetadata"/> for the primary entity of the chart.
		/// </summary>
		public EntityMetadata PrimaryEntityMetadata { get; private set; }

		/// <summary>
		/// Logical name of the entity being charted.
		/// </summary>
		public string PrimaryEntityTypeCode { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CrmChart" /> class.
		/// </summary>
		/// <param name="chart">An <see cref="Entity"/> record that must be of type savedqueryvisualization.</param>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> to use to execute retrieval of localized labels.</param>
		/// <param name="languageCode">The locale used to retrieve the localized labels.</param>
		public CrmChart(Entity chart, OrganizationServiceContext serviceContext, int languageCode = 0)
		{
			if (chart == null || chart.LogicalName != "savedqueryvisualization")
			{
				return;
			}

			this.languageCode = languageCode;

			this.DataDescriptionXml = chart.GetAttributeValue<string>("datadescription");

			this.DataDescription = DataDefinition.Parse(this.DataDescriptionXml);

			var localizedDescription = serviceContext.RetrieveLocalizedLabel(chart.ToEntityReference(), "description", languageCode);

			this.Description = string.IsNullOrWhiteSpace(localizedDescription) ? chart.GetAttributeValue<string>("description") : localizedDescription;

			if (this.DataDescription != null && this.DataDescription.FetchCollection != null)
			{
				this.Fetch = this.DataDescription.FetchCollection.FirstOrDefault();
			}
			
			this.Id = chart.Id;

			var localizedName = serviceContext.RetrieveLocalizedLabel(chart.ToEntityReference(), "name", languageCode);

			this.Name = string.IsNullOrWhiteSpace(localizedName) ? chart.GetAttributeValue<string>("name") : localizedName;

			this.PrimaryEntityTypeCode = chart.GetAttributeValue<string>("primaryentitytypecode");

			this.PrimaryEntityMetadata = MetadataHelper.GetEntityMetadata(serviceContext, this.PrimaryEntityTypeCode);

			this.PresentationDescriptionXml = this.ReplacePresentationDescriptionMetadataPlaceholders(chart.GetAttributeValue<string>("presentationdescription"), serviceContext);
		}

		/// <summary>
		/// Replaces placeholder values in chart presentation XML with localized labels.
		/// </summary>
		/// <param name="presentationDescription">Chart presentation description XML.</param>
		/// <param name="serviceContext">CRM service context.</param>
		/// <returns>Chart presentation description XML with placeholder values replaced with proper labels.</returns>
		private string ReplacePresentationDescriptionMetadataPlaceholders(string presentationDescription, OrganizationServiceContext serviceContext)
		{
			if (string.IsNullOrEmpty(presentationDescription))
			{
				return presentationDescription;
			}

			try
			{
				var xml = XElement.Parse(presentationDescription);

				foreach (var namedElement in xml.XPathSelectElements("//*[@Name]"))
				{
					this.ReplacePresentationDescriptionOptionSetPlaceholders(namedElement, "Name", serviceContext);
				}

				return xml.ToString(SaveOptions.DisableFormatting);
			}
			catch (XmlException)
			{
				return presentationDescription;
			}
		}

		/// <summary>
		/// Replaces placeholder values in chart presentation XML element attribute values with localized labels, in place.
		/// </summary>
		/// <param name="element">The XML element with a given attribute value to be parsed and potentialy replaced.</param>
		/// <param name="attributeName">The name of the XML attribute to be parsed for replacements.</param>
		/// <param name="serviceContext">CRM service context.</param>
		private void ReplacePresentationDescriptionOptionSetPlaceholders(XElement element, string attributeName, OrganizationServiceContext serviceContext)
		{
			var attribute = element.Attribute(attributeName);

			if (attribute == null)
			{
				return;
			}

			var placeholderMatch = OptionSetPatternRegex.Match(attribute.Value);

			if (placeholderMatch.Success)
			{
				attribute.SetValue(this.ReplacePresentationDescriptionOptionSetPlaceholderMatch(placeholderMatch, serviceContext));
			}
		}

		/// <summary>
		/// Replaces placeholder values in chart presentation XML option set value matches.
		/// </summary>
		/// <param name="match">The regex match for which to find a localized replacement value.</param>
		/// <param name="serviceContext">CRM service context.</param>
		/// <returns>Localized option set value label.</returns>
		private string ReplacePresentationDescriptionOptionSetPlaceholderMatch(Match match, OrganizationServiceContext serviceContext)
		{
			var optionSetName = match.Groups["optionSetName"].Value;
			var optionSetValue = match.Groups["optionSetValue"].Value;

			var optionSetAttributeMetadata = this.PrimaryEntityMetadata.Attributes
				.OfType<EnumAttributeMetadata>()
				.FirstOrDefault(e => e.OptionSet != null && string.Equals(optionSetName, e.OptionSet.Name, StringComparison.OrdinalIgnoreCase));

			if (optionSetAttributeMetadata != null)
			{
				return this.GetOptionSetValueLabel(optionSetAttributeMetadata.OptionSet, optionSetValue);
			}

			OptionSetMetadata optionSet;

			try
			{
				var retrieveOptionSetResponse = (RetrieveOptionSetResponse)serviceContext.Execute(new RetrieveOptionSetRequest
				{
					Name = optionSetName
				});

				optionSet = retrieveOptionSetResponse.OptionSetMetadata as OptionSetMetadata;
			}
			catch (FaultException<OrganizationServiceFault>)
			{
				return optionSetValue;
			}

			if (optionSet == null)
			{
				return optionSetValue;
			}

			return this.GetOptionSetValueLabel(optionSet, optionSetValue);
		}

		/// <summary>
		/// Gets the localized label for a given option set value.
		/// </summary>
		/// <param name="optionSet">Metadata for a given option set.</param>
		/// <param name="optionSetValue">An option set value, as a string.</param>
		/// <returns>Localized label for a given option set value, otherwise the option set value itself if not found.</returns>
		private string GetOptionSetValueLabel(OptionSetMetadata optionSet, string optionSetValue)
		{
			int value;

			if (!int.TryParse(optionSetValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
			{
				return optionSetValue;
			}

			var option = optionSet.Options.FirstOrDefault(e => e.Value == value);

			if (option == null)
			{
				return optionSetValue;
			}

			return this.GetLocalizedLabel(option.Label);
		}
		
		/// <summary>
		/// Get the localized string from a <see cref="Label"/>.
		/// </summary>
		/// <param name="label">The <see cref="Label"/> to get the localized string from.</param>
		/// <returns>A localized string.</returns>
		private string GetLocalizedLabel(Label label)
		{
			var localizedLabel = label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == this.languageCode);

			if (localizedLabel != null)
			{
				return localizedLabel.Label;
			}

			if (label.UserLocalizedLabel != null)
			{
				return label.UserLocalizedLabel.Label;
			}

			return null;
		}
	}
}
