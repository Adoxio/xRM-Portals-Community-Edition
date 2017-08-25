/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;
	using System.Xml.Serialization;
	using System.Xml.XPath;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.Globalization;
	using Adxstudio.Xrm.Metadata;

	/// <summary>
	/// A view defined by a savedquery record in CRM.
	/// </summary>
	public class SavedQueryView
	{
		public const string DefaultAliasColumnNameStringFormat = "{0} ({1})";

		/// <summary>
		/// Details of a column in a view
		/// </summary>
		public class ViewColumn
		{
			/// <summary>
			/// Parameterless constructor
			/// </summary>
			public ViewColumn()
			{
			}
			
			/// <summary>
			/// ViewColumn constructor
			/// </summary>
			/// <param name="logicalName">Logical Name of the column's attribute</param>
			/// <param name="name">Name of the column</param>
			/// <param name="metadata">AttributeMetadata of the column's attribute</param>
			/// <param name="width">Width of the column in pixels</param>
			/// <param name="sortDisabled">True value indicates that sort is disabled, otherwise sort is enabled by default. </param>
			public ViewColumn(string logicalName, string name, AttributeMetadata metadata, int width, bool sortDisabled = false)
			{
				LogicalName = logicalName;
				Name = name;
				Metadata = metadata;
				Width = width;
				SortDisabled = sortDisabled;
			}

			/// <summary>
			/// Logical Name of the column's attribute
			/// </summary>
			public string LogicalName { get; set; }
			/// <summary>
			/// Name of the column
			/// </summary>
			public string Name { get; set; }
			/// <summary>
			/// AttributeMetadata of the column's attribute
			/// </summary>
			public AttributeMetadata Metadata { get; set; }

			/// <summary>
			/// Width of the column in pixels.
			/// </summary>
			public int Width { get; set; }

			/// <summary>
			/// True value indicates that sort is disabled, otherwise sort is enabled by default. 
			/// </summary>
			public bool SortDisabled { get; set; }
		}

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public SavedQueryView()
		{
		}

		/// <summary>
		/// SavedQueryView constructor
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="entityLogicalName">Logical Name of the entity the view is associated to</param>
		/// <param name="savedQueryName">Name of the savedquery record in CRM</param>
		/// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
		/// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
		public SavedQueryView(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: this(serviceContext, GetSavedQuery(serviceContext, entityLogicalName, savedQueryName), languageCode, aliasColumnNameStringFormat) { }

        /// <summary>
        /// SavedQueryView constructor
        /// </summary>
        /// <param name="serviceContext">OrganizationServiceContext</param>
        /// <param name="fetchXmlString">Existing known FetchXML for this view</param>
        /// <param name="layoutXmlString">Existing known LayoutXML for this view</param>
        /// <param name="entityLogicalName">Logical Name of the entity the view is associated to</param>
        /// <param name="savedQueryName">Name of the savedquery record in CRM</param>
        /// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
        /// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
        public SavedQueryView(OrganizationServiceContext serviceContext, string fetchXmlString, string layoutXmlString, string entityLogicalName, string savedQueryName, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
            : this(serviceContext, GetSavedQuery(serviceContext, entityLogicalName, savedQueryName), fetchXmlString, layoutXmlString, languageCode, aliasColumnNameStringFormat) { }

        /// <summary>
        /// SavedQueryView constructor
        /// </summary>
        /// <param name="serviceContext">OrganizationServiceContext</param>
        /// <param name="id">Unique ID of the savedquery record in CRM</param>
        /// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
        /// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
        public SavedQueryView(OrganizationServiceContext serviceContext, Guid id, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
            : this(serviceContext, GetSavedQuery(serviceContext, id), languageCode, aliasColumnNameStringFormat) { }

        /// <summary>
        /// SavedQueryView constructor
        /// </summary>
        /// <param name="serviceContext">OrganizationServiceContext</param>
        /// <param name="fetchXmlString">Existing known FetchXML for this view</param>
        /// <param name="layoutXmlString">Existing known LayoutXML for this view</param>
        /// <param name="id">Unique ID of the savedquery record in CRM</param>
        /// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
        /// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
        public SavedQueryView(OrganizationServiceContext serviceContext, string fetchXmlString, string layoutXmlString, Guid id, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
            : this(serviceContext, GetSavedQuery(serviceContext, id), fetchXmlString, layoutXmlString, languageCode, aliasColumnNameStringFormat) { }

		/// <summary>
		/// SavedQueryView constructor
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="entityLogicalName">Logical Name of the entity the view is associated to</param>
		/// <param name="queryType">View querytype code.</param>
		/// <param name="isDefault">Is the default view.</param>
		/// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
		/// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
		public SavedQueryView(OrganizationServiceContext serviceContext, string entityLogicalName, int queryType, bool isDefault = false, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: this(serviceContext, GetSavedQuery(serviceContext, entityLogicalName, queryType, isDefault), languageCode, aliasColumnNameStringFormat) { }

		/// <summary>
		/// SavedQueryView constructor
		/// </summary>
		/// <param name="serviceContext">OrganizationServiceContext</param>
		/// <param name="savedQuery">savedquery entity</param>
		/// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
		/// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
        public SavedQueryView(OrganizationServiceContext serviceContext, Entity savedQuery, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
            : this(serviceContext, savedQuery, string.Empty, string.Empty, languageCode, aliasColumnNameStringFormat) { }

        /// <summary>
        /// SavedQueryView constructor
        /// </summary>
        /// <param name="serviceContext">OrganizationServiceContext</param>
        /// <param name="savedQuery">savedquery entity</param>
        /// <param name="fetchXmlString">Existing known FetchXML for this view</param>
        /// <param name="layoutXmlString">Existing known LayoutXML for this view</param>
        /// <param name="languageCode">Language code used to retrieve the localized attribute label</param>
        /// <param name="aliasColumnNameStringFormat">A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".</param>
        public SavedQueryView(OrganizationServiceContext serviceContext, Entity savedQuery, string fetchXmlString, string layoutXmlString, int? languageCode = 0, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
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
			LanguageCode = languageCode ?? 0;
			AliasColumnNameStringFormat = aliasColumnNameStringFormat;

            if (string.IsNullOrEmpty(fetchXmlString))
            {
                fetchXmlString = savedQuery.GetAttributeValue<string>("fetchxml");
            }
			if (!string.IsNullOrEmpty(fetchXmlString))
			{
				FetchXml = XElement.Parse(fetchXmlString);
				SortExpression = SetSortExpression(FetchXml);

				var entityElement = FetchXml.Element("entity");
				if (entityElement != null)
				{
					var entityName = entityElement.Attribute("name").Value;
					EntityLogicalName = entityName;
				}

				var response = (RetrieveEntityResponse)ServiceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = EntityLogicalName,
					EntityFilters = EntityFilters.Attributes
				});

				if (response == null || response.EntityMetadata == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to get EntityMetadata for entity of type '{0}'.", EntityNamePrivacy.GetEntityName(EntityLogicalName)));

					return;
				}

				EntityMetadata = response.EntityMetadata;

				PrimaryKeyLogicalName = EntityMetadata.PrimaryIdAttribute;
			}

            if (string.IsNullOrEmpty(layoutXmlString))
            {
                layoutXmlString = savedQuery.GetAttributeValue<string>("layoutxml");
            }
			LayoutXml = XElement.Parse(layoutXmlString);
            
			var rowElement = LayoutXml.Element("row");
			if (rowElement != null)
			{
				var cellNames = rowElement.Elements("cell")
					.Where(cell => cell.Attribute("ishidden") == null || cell.Attribute("ishidden").Value != "1")
					.Select(cell => cell.Attribute("name")).Where(name => name != null);
				CellNames = cellNames;
				var disabledSortCellNames = rowElement.Elements("cell")
					.Where(cell => cell.Attribute("disableSorting") != null && cell.Attribute("disableSorting").Value == "1")
					.Where(cell => cell.Attribute("name") != null)
					.Select(cell => cell.Attribute("name").Value);
				DisabledSortCellNames = disabledSortCellNames;
				var cellWidths = rowElement.Elements("cell")
					.Where(cell => cell.Attribute("ishidden") == null || cell.Attribute("ishidden").Value != "1")
					.Where(cell => cell.Attribute("name") != null)
					.ToDictionary(cell => cell.Attribute("name").Value, cell => Convert.ToInt32(cell.Attribute("width").Value));
				CellWidths = cellWidths;
			}
			
			Name = ServiceContext.RetrieveLocalizedLabel(savedQuery.ToEntityReference(), "name", LanguageCode);
			Id = savedQuery.GetAttributeValue<Guid>("savedqueryid");
		}

		/// <summary>
		/// A format string used to compose an alias column label. Index 0 is the alias attribute display name, index 1 is the aliased entity's display name. Default is "{0} ({1})".
		/// </summary>
		public string AliasColumnNameStringFormat { get; private set; }

		/// <summary>
		/// The name of the view.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Unique identifier of the view.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// Collection of the names of cells in the layout grid.
		/// </summary>
		public IEnumerable<XAttribute> CellNames { get; private set; }

		/// <summary>
		/// Width of the cells in pixels.
		/// </summary>
		public Dictionary<string, int> CellWidths { get; private set; }

		/// <summary>
		/// Collection of the names of cells in the layout grid where sort is disabled.
		/// </summary>
		public IEnumerable<string> DisabledSortCellNames { get; private set; }

		/// <summary>
		/// Logical name of the entity associated with the savedquery.
		/// </summary>
		public string EntityLogicalName { get; private set; }

		/// <summary>
		/// Describes the data to return in the saved query using the FetchXML language.
		/// </summary>
		[XmlElement("FetchXml")]
		public XElement FetchXml { get; private set; }

		/// <summary>
		/// Logical name of the primary id attribute for the entity associated with the savedquery.
		/// </summary>
		public string PrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Language code used to retrieve the localized attribute label.
		/// Must use the CRM Lcid rather than the potentially custom language Lcid since Saved Queries are CRM concept.
		/// </summary>
		public int LanguageCode { get; private set; }

		/// <summary>
		/// Defines a grid that displays results from the saved query.
		/// </summary>
		[XmlElement("LayoutXml")]
		public XElement LayoutXml { get; private set; }

		/// <summary>
		/// <see cref="EntityMetadata"/> for the target entity of the savedquery.
		/// </summary>
		public EntityMetadata EntityMetadata { get; private set; }

		/// <summary>
		/// Get the columns of the savedquery view with the localized display names.
		/// </summary>
		public List<ViewColumn> Columns
		{
			get
			{
				if (EntityMetadata == null)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to get EntityMetadata for entity of type '{0}'.", EntityNamePrivacy.GetEntityName(EntityLogicalName)));

                    return new List<ViewColumn>();
				}

				if (LanguageCode == 0)
				{
					LanguageCode = HttpContext.Current?.GetCrmLcid() ?? CultureInfo.CurrentCulture.LCID;
				}

				var columns = CellNames.Select(name => ConvertLayoutCellToViewColumn(ServiceContext, EntityMetadata, name.Value, CellWidths, DisabledSortCellNames, FetchXml, LanguageCode, AliasColumnNameStringFormat)).ToList();

				return columns;
			}
		}

		/// <summary>
		/// The view (savedquery) entity record.
		/// </summary>
		public Entity SavedQuery { get; private set; }

		/// <summary>
		/// The sort expression defined by the orderby element of the view's fetchXml
		/// </summary>
		public string SortExpression { get; private set; }

		/// <summary>
		/// OrganizationServiceContext
		/// </summary>
		protected OrganizationServiceContext ServiceContext { get; private set; }

		private string SetSortExpression(XElement fetchXml)
		{
			var entityElement = fetchXml.XPathSelectElement("//entity");

			if (entityElement == null)
			{
				return string.Empty;
			}

			var orderElements = entityElement.Elements("order").ToArray();
			var orderby = orderElements.Select(orderElement => orderElement.Attribute("descending").Value == "true" ? orderElement.Attribute("attribute").Value + " DESC" : orderElement.Attribute("attribute").Value + " ASC").ToArray();
			var sortExpression = string.Join(",", orderby);
			return sortExpression;
		}

		private static Entity GetSavedQuery(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName)
		{
			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentNullException("entityLogicalName");
			}

			if (string.IsNullOrWhiteSpace(savedQueryName))
			{
				throw new ArgumentNullException("savedQueryName");
			}

			var savedQuery = serviceContext.CreateQuery("savedquery")
				.FirstOrDefault(e => e.GetAttributeValue<string>("name") == savedQueryName
					&& e.GetAttributeValue<string>("returnedtypecode") == entityLogicalName);

			if (savedQuery == null)
			{
				throw new SavedQueryNotFoundException(entityLogicalName);
			}

			return savedQuery;
		}

		private static Entity GetSavedQuery(OrganizationServiceContext serviceContext, Guid id)
		{
			var savedQuery = serviceContext.CreateQuery("savedquery").FirstOrDefault(e => e.GetAttributeValue<Guid>("savedqueryid") == id);

			if (savedQuery == null)
			{
				throw new SavedQueryNotFoundException(id);
			}

			return savedQuery;
		}

		private static Entity GetSavedQuery(OrganizationServiceContext serviceContext, string entityLogicalName, int queryType, bool isDefault)
		{
			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentNullException("entityLogicalName");
			}

			var savedQuery =
				serviceContext.CreateQuery("savedquery")
					.FirstOrDefault(
						s =>
							s.GetAttributeValue<string>("returnedtypecode") == entityLogicalName &&
							s.GetAttributeValue<bool>("isdefault") == isDefault && s.GetAttributeValue<int>("querytype") == queryType);

			if (savedQuery == null)
			{
				throw new SavedQueryNotFoundException(entityLogicalName, queryType, isDefault);
			}

			return savedQuery;
		}

		private static ViewColumn ConvertLayoutCellToViewColumn(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string cellName, Dictionary<string, int> cellWidths, IEnumerable<string> disabledSortCellNames, XElement fetchXml, int languageCode, string aliasColumnNameStringFormat)
		{
			var label = GetLabel(serviceContext, entityMetadata, cellName, fetchXml, languageCode, aliasColumnNameStringFormat);
			var metadata = GetMetadata(serviceContext, entityMetadata, cellName, fetchXml);
			var sortDisabled = disabledSortCellNames.Contains(cellName);
			var width = cellWidths[cellName];
			return new ViewColumn(cellName, label, metadata, width, sortDisabled);
		}

		private static AttributeMetadata GetMetadata(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string attributeName, XElement fetchXml)
		{
			AttributeMetadata attributeMetadata;

			if (!TryGetAttributeMetadata(entityMetadata, attributeName, out attributeMetadata))
			{
				if (!TryGetAttributeMetadataFromLinkEntityAlias(serviceContext, attributeName, fetchXml, out attributeMetadata))
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to get AttributeMetadata");
                }
			}

			return attributeMetadata;
		}

		private static bool TryGetAttributeMetadata(EntityMetadata entityMetadata, string attributeName, out AttributeMetadata metadata)
		{
			metadata = null;
			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(attribute => attribute.LogicalName == attributeName);
			if (attributeMetadata == null)
			{
				return false;
			}
			metadata = attributeMetadata;
			return true;
		}

		private static bool TryGetAttributeMetadataFromLinkEntityAlias(OrganizationServiceContext serviceContext, string aliasAttributeName, XElement fetchXml, out AttributeMetadata metadata)
		{
			metadata = null;

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

			if (!TryGetAttributeMetadata(response.EntityMetadata, attributeName, out metadata))
			{
				return false;
			}

			return true;
		}

		private static string GetLabel(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string attributeName, XElement fetchXml, int languageCode, string aliasColumnNameStringFormat)
		{
			string label;

			return TryGetLabelFromAttributeMetadata(entityMetadata, attributeName, languageCode, out label)
				|| TryGetLabelFromLinkEntityAlias(serviceContext, entityMetadata, attributeName, fetchXml, languageCode, aliasColumnNameStringFormat, out label)
				? label
				: attributeName;
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

		private static bool TryGetLabelFromLinkEntityAlias(OrganizationServiceContext serviceContext, EntityMetadata entityMetadata, string aliasAttributeName, XElement fetchXml, int languageCode, string aliasColumnNameStringFormat, out string label)
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

			label = aliasColumnNameStringFormat.FormatWith(linkAttributeLabel, linkRelationshipLabel);

			return true;
		}
	}
}
