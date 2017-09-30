/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Adxstudio.Xrm.Data;
	using Adxstudio.Xrm.Globalization;

	/// <summary>
	/// Generates Fields for databound controls based on a Saved Query from CRM.
	/// </summary>
	public class SavedQueryColumnsGenerator : IAutoFieldGenerator
	{
		/// <summary>
		/// Language Code to generate the fields with localized labels. 
		/// Must use the CRM Lcid rather than the potentially custom language Lcid since Saved Queries are CRM concept.
		/// </summary>
		public int LanguageCode;

		/// <summary>
		/// Class intialization that specifies the Saved Query entity object and Language Code to generate the fields and localized labels.
		/// </summary>
		/// <param name="serviceContext">Context</param>
		/// <param name="savedQuery">Saved Query</param>
		/// <param name="languageCode">Language Code to be used to retrieve the localized labels</param>
		public SavedQueryColumnsGenerator(OrganizationServiceContext serviceContext, Entity savedQuery, int languageCode)
			: this(serviceContext, savedQuery)
		{
			LanguageCode = languageCode;
		}

		/// <summary>
		/// Class intialization that specifies the Saved Query entity object to generate the fields from.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="savedQuery"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SavedQueryColumnsGenerator(OrganizationServiceContext serviceContext, Entity savedQuery)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (savedQuery == null)
			{
				throw new ArgumentNullException("savedQuery");
			}

			ServiceContext = serviceContext;
			SavedQuery = savedQuery;
		}

		/// <summary>
		/// Class initialization that will retrieve a saved query for the entity given the logical name to generate the fields from.
		/// </summary>
		/// <param name="serviceContext">Context</param>
		/// <param name="entityLogicalName">Logical name of the entity</param>
		/// <param name="savedQueryName">Saved Query View Name for the entity</param>
		public SavedQueryColumnsGenerator(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName)
			: this(serviceContext, GetSavedQuery(serviceContext, entityLogicalName, savedQueryName)) { }

		/// <summary>
		/// Class initialization that will retrieve a saved query for the entity given the logical name to generate the fields from and language code to provide localized labels.
		/// </summary>
		/// <param name="serviceContext">Context</param>
		/// <param name="entityLogicalName">Logical name of the entity</param>
		/// <param name="savedQueryName">Save Query View Name for the entity</param>
		/// <param name="languageCode">Language Code to be used to retrieve the localized labels</param>
		public SavedQueryColumnsGenerator(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName, int languageCode)
			: this(serviceContext, GetSavedQuery(serviceContext, entityLogicalName, savedQueryName))
		{
			LanguageCode = languageCode;
		}

		protected Entity SavedQuery { get; private set; }

		protected OrganizationServiceContext ServiceContext { get; private set; }

		public ICollection GenerateFields(Control control)
		{
			var layoutXml = XElement.Parse(SavedQuery.GetAttributeValue<string>("layoutxml"));
			var cellNames = layoutXml.Element("row").Elements("cell").Select(cell => cell.Attribute("name")).Where(name => name != null);
			var disabledSortCellNames = layoutXml.Element("row").Elements("cell")
						.Where(cell => cell.Attribute("disableSorting") != null && cell.Attribute("disableSorting").Value == "1")
						.Where(cell => cell.Attribute("name") != null)
						.Select(cell => cell.Attribute("name").Value);
			var fetchXml = XElement.Parse(SavedQuery.GetAttributeValue<string>("fetchxml"));
			var entityName = fetchXml.Element("entity").Attribute("name").Value;

			var response = (RetrieveEntityResponse)ServiceContext.Execute(new RetrieveEntityRequest
			{
				LogicalName = entityName,
				EntityFilters = EntityFilters.Attributes
			});

			if (response == null || response.EntityMetadata == null)
			{
				return new DataControlFieldCollection();
			}
			
			if (LanguageCode == 0)
			{
				LanguageCode = HttpContext.Current?.GetCrmLcid() ?? CultureInfo.CurrentCulture.LCID;
			}
			
			var fields =
				from name in cellNames
				let label = GetLabel(ServiceContext, response.EntityMetadata, name.Value, fetchXml, LanguageCode)
				where label != null
				select new BoundField
				{
					DataField = name.Value,
					SortExpression = disabledSortCellNames.Contains(name.Value) ? string.Empty : name.Value,
					HeaderText = label
				};

			return fields.ToArray();
		}

		/// <summary>
		/// Transforms a collection of entities to a DataTable.
		/// </summary>
		public DataTable ToDataTable(IEnumerable<Entity> entities, string dateTimeFormat = null, IFormatProvider dateTimeFormatProvider = null)
		{
			return entities.ToDataTable(ServiceContext, SavedQuery, dateTimeFormat: dateTimeFormat, dateTimeFormatProvider: dateTimeFormatProvider);
		}

		private static Entity GetSavedQuery(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			var savedQuery = serviceContext.CreateQuery("savedquery")
				.FirstOrDefault(e => e.GetAttributeValue<string>("name") == savedQueryName
					&& e.GetAttributeValue<string>("returnedtypecode") == entityLogicalName);

			if (savedQuery == null)
			{
				throw new ArgumentException("A saved query for entity {0} with the name {1} couldn't be found.".FormatWith(entityLogicalName, savedQueryName), "savedQueryName");
			}

			return savedQuery;
		}

		private static string GetLabel(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string attributeName, XElement fetchXml, int languageCode)
		{
			string label;

			return TryGetLabelFromAttributeMetadata(entityMetadata, attributeName, languageCode, out label)
				|| TryGetLabelFromLinkEntityAlias(serviceContext, entityMetadata, attributeName, fetchXml, languageCode, out label)
				? label
				: null;
		}

		private static bool TryGetLabelFromAttributeMetadata(EntityMetadata entityMetadata, string attributeName, int languageCode, out string label)
		{
			label = null;

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);

			if (attributeMetadata == null)
			{
				return false;
			}

			var localizedLabel = attributeMetadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode);

			label = localizedLabel == null
				? attributeMetadata.DisplayName.GetLocalizedLabelString()
				: localizedLabel.Label;

			return true;
		}

		private static bool TryGetLabelFromLinkEntityAlias(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string aliasAttributeName, XElement fetchXml, int languageCode, out string label)
		{
			label = null;

			var aliasAttributeNameMatch = Regex.Match(aliasAttributeName, @"^(?<alias>\w+)\.(?<attributeName>\w+)$", RegexOptions.ExplicitCapture);

			if (!aliasAttributeNameMatch.Success)
			{
				return false;
			}

			var alias = aliasAttributeNameMatch.Groups["alias"].Value;
			var attributeName = aliasAttributeNameMatch.Groups["attributeName"].Value;

			var linkEntityElement = fetchXml.XPathSelectElement("//link-entity[@alias='{0}']".FormatWith(alias));

			if (linkEntityElement == null)
			{
				return false;
			}

			var linkEntityNameAttribute = linkEntityElement.Attribute("name");

			if (linkEntityNameAttribute == null)
			{
				return false;
			}

			var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
			{
				LogicalName = linkEntityNameAttribute.Value,
				EntityFilters = EntityFilters.Attributes
			});

			if (response == null || response.EntityMetadata == null)
			{
				return false;
			}

			string linkAttributeLabel;

			if (!TryGetLabelFromAttributeMetadata(response.EntityMetadata, attributeName, languageCode, out linkAttributeLabel))
			{
				return false;
			}

			var linkEntityToAttribute = linkEntityElement.Attribute("to");

			if (linkEntityToAttribute == null)
			{
				label = linkAttributeLabel;

				return true;
			}

			string linkRelationshipLabel;

			if (!TryGetLabelFromAttributeMetadata(entityMetadata, linkEntityToAttribute.Value, languageCode, out linkRelationshipLabel))
			{
				label = linkAttributeLabel;

				return true;
			}

			label = "{0} ({1})".FormatWith(linkAttributeLabel, linkRelationshipLabel);

			return true;
		}
	}
}
