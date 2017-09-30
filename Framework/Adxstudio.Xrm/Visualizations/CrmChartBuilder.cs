/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web.Script.Serialization;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// Information required to build a chart.
	/// </summary>
	public class CrmChartBuilder : IChartBuilder
	{
		/// <summary>
		/// An infinite for loop to be prefixed in the json response for security reasons, this should be stripped in the client side code.
		/// </summary>
		private const string BadData = "for(;;);";

		/// <summary>
		/// A dictionary of <see cref="Microsoft.Xrm.Sdk.Metadata.EntityMetadata"/> for all entities contained in the query.
		/// </summary>
		private readonly Dictionary<string, EntityMetadata> entityMetadataCache = new Dictionary<string, EntityMetadata>();

		/// <summary>
		/// A dictionary of <see cref="Microsoft.Xrm.Sdk.Metadata.AttributeMetadata"/> for all attributes contained in the query. (entity logical name, attribute logical name, attribute metadata)
		/// </summary>
		private readonly List<Tuple<string, string, AttributeMetadata>> attributeMetadataCache = new List<Tuple<string, string, AttributeMetadata>>();

		/// <summary>
		/// Culture used for formatting currency and datetime.
		/// </summary>
		private readonly CultureInfo culture;

		/// <summary>
		/// Culture used for formatting currency and datetime.
		/// </summary>
		private readonly ContextLanguageInfo contextLanguage;

		/// <summary>
		/// A dictionary array of AttributeMetadata values to be used by the client in the AttributeMetadata constructor.
		/// </summary>
		public Dictionary<string, object>[] AttributeMetadata { get; set; }

		/// <summary>
		/// Serialized List of Dictionary of AttributeMetadata values to be used by the client in the AttributeMetadata constructor.
		/// </summary>
		public string AttributeMetadataSerialized { get; set; }

		/// <summary>
		/// The details that describes the CRM chart (savedqueryvisualization).
		/// </summary>
		public CrmChart ChartDefinition { get; set; }

		/// <summary>
		/// The data that was retrieved from the <see cref="Query"/>
		/// </summary>
		public IEnumerable<Entity> Data { get; set; }

		/// <summary>
		/// The <see cref="Data"/> serialized containing the information needed for the RecordCollectionModel that will be built on the client side.
		/// </summary>
		/// <remarks>String will be prefixed with an infinite for loop for security reasons, this should be stripped in the client side code.</remarks>
		public string DataJson { get; set; }

		/// <summary>
		/// A dictionary array of all the entity metadata needed for rendering the chart.
		/// </summary>
		public Dictionary<string, object>[] EntityMetadata { get; set; }

		/// <summary>
		/// Serialized dictionary array of all the entity metadata needed for rendering the chart.
		/// </summary>
		public string EntityMetadataSerialized { get; set; }

		/// <summary>
		/// If <see cref="EntityPermissionsEnabled"/> is true, this indicates whether <see cref="CrmEntityPermissionProvider"/> assertion denied read of the entity type associated with the chart.
		/// </summary>
		public bool EntityPermissionDenied { get; set; }

		/// <summary>
		/// Indicates whether the Entity Permissions are enabled or not. If enabled then the <see cref="CrmEntityPermissionProvider"/> will be used to inject the necessary filters and links into the <see cref="Query"/> to provide record level security trimming.
		/// </summary>
		public bool EntityPermissionsEnabled { get; set; }
		
		/// <summary>
		/// Identifier used to assign to the ID attribute on the chart container element in the target DOM.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The FetchXML query to be executed to retrieve the data to be plotted in the chart. If <see cref="ViewDefinition"/> has been specified, this FetchXML will be the result of a merge of the chart's <see cref="Fetch"/> and view's <see cref="Fetch"/>.
		/// </summary>
		public Fetch Query { get; set; }

		/// <summary>
		/// The raw FetchXML serialized from the <see cref="Query"/> to be executed to retrieve the data to be plotted in the chart. If <see cref="ViewDefinition"/> has been specified, this FetchXML will be the result of a merge of the chart's <see cref="Fetch"/> and view's <see cref="Fetch"/>.
		/// </summary>
		public string FetchXml { get; set; }

		/// <summary>
		/// Override the string values for chart UI labels specified in the CRM ResourceManager.
		/// </summary>
		public Dictionary<string, string> ResourceManagerStringOverrides { get; set; }

		/// <summary>
		/// Serialized string values for chart UI labels specified in the CRM ResourceManager.
		/// </summary>
		public string ResourceManagerStringOverridesSerialized { get; set; }

		/// <summary>
		/// An optional definition of a CRM view (savedquery) that if present will be used to merge the FetchXML into the chart's FetchXML.
		/// </summary>
		public CrmView ViewDefinition { get; set; }

		/// <summary>
		///  Initializes a new instance of the <see cref="CrmChartBuilder" /> class.
		/// </summary>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> to be used for data retrieval.</param>
		/// <param name="chartId">The ID of the CRM chart (savedqueryvisualization) record.</param>
		/// <param name="contextLanguage">The <see cref="ContextLanguageInfo"/> </param>
		/// <param name="viewId">An optional ID of a CRM view (savedquery) record that can be used to adjust the query filters.</param>
		/// <param name="entityPermissionsEnabled">An optional boolean indicating whether <see cref="CrmEntityPermissionProvider"/> assertion is executed. Default is true.</param>
		/// <param name="languageCode">The language code to be used for get <see cref="CultureInfo"/> for formatting currency, datetime, and when retrieving localized labels from metadata.</param>
		public CrmChartBuilder(OrganizationServiceContext serviceContext, Guid chartId, ContextLanguageInfo contextLanguage, Guid? viewId = null, bool entityPermissionsEnabled = true, int languageCode = 0)
		{
			this.culture = ContextLanguageInfo.GetCulture(languageCode);
			this.contextLanguage = contextLanguage;

			this.Data = Enumerable.Empty<Entity>();

			var chart = serviceContext.CreateQuery("savedqueryvisualization").FirstOrDefault(s => s.GetAttributeValue<Guid>("savedqueryvisualizationid") == chartId && s.GetAttributeValue<int>("componentstate") == 0);

			if (chart == null)
			{
				throw new ApplicationException(string.Format("Chart with ID '{0}' could not be found.", chartId));
			}

			this.Id = Guid.NewGuid().ToString("N");

			if (viewId != null && viewId != Guid.Empty)
			{
				var view = serviceContext.CreateQuery("savedquery").FirstOrDefault(s => s.GetAttributeValue<Guid>("savedqueryid") == viewId && s.GetAttributeValue<int>("componentstate") == 0);

				if (view != null)
				{
					this.ViewDefinition = new CrmView(view);
				}
			}

			this.ChartDefinition = new CrmChart(chart, serviceContext, contextLanguage.IsCrmMultiLanguageEnabled ? contextLanguage.ContextLanguage.CrmLcid : this.culture.LCID);

			this.EntityPermissionsEnabled = entityPermissionsEnabled;

			this.ResourceManagerStringOverrides = this.CreateResourceManagerStringOverrides();

			this.ResourceManagerStringOverridesSerialized =
				new JavaScriptSerializer().Serialize(this.ResourceManagerStringOverrides);

			if (this.ChartDefinition == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(this.ChartDefinition.DataDescriptionXml))
			{
				return;
			}

			this.MergeViewFetchIntoChartFetch();

			var metadata = this.ChartDefinition.PrimaryEntityMetadata;

			// If Name is not being retrieved and there is no grouping, add it to the attribute collection to
			// give chart labels a value
			if (!this.Query.Aggregate.GetValueOrDefault(false) && (!this.Query.Entity.Attributes.Any(a => a.Name.Equals(metadata.PrimaryNameAttribute, StringComparison.OrdinalIgnoreCase) || (a.GroupBy.HasValue && a.GroupBy.Value))))
			{
				this.Query.Entity.Attributes.Add(new FetchAttribute(metadata.PrimaryNameAttribute));
			}

			this.Data = this.RetrieveData(serviceContext);

			// Serialize the data in the format required by the CrmHighchartsLibrary.js

			this.entityMetadataCache.Add(metadata.LogicalName, metadata);

			this.PopulateEntityMetadataCacheForEntityLinks(serviceContext, this.Query.Entity.Links);

			var queryAttributes = this.Query.Entity.Attributes == null ? new List<FetchAttribute>() : this.Query.Entity.Attributes.ToList();

			var queryAttributeNames = queryAttributes.Select(a => a.Name).ToArray();

			foreach (var attributeMetadata in metadata.Attributes.Where(a => queryAttributeNames.Contains(a.LogicalName)))
			{
				this.attributeMetadataCache.Add(new Tuple<string, string, AttributeMetadata>(attributeMetadata.EntityLogicalName, attributeMetadata.LogicalName, attributeMetadata));
			}

			this.PopulateAttributeMetadataCacheForEntityLinkAttributes(this.Query.Entity.Links);
			
			var chartData = this.ConvertRecordsToDictionary(serviceContext, this.Data, metadata);

			var chartDataJson = new JavaScriptSerializer().Serialize(chartData);

			// An infinite for loop is prefixed in the json response for security reasons, this will be stripped in the client side code.
			this.DataJson = BadData + chartDataJson;

			// Build the entity metadata dictionary

			this.EntityMetadata = this.entityMetadataCache.Select(e => this.CreateEntityMetadataDictionary(e.Value)).ToArray();

			this.EntityMetadataSerialized = new JavaScriptSerializer().Serialize(this.EntityMetadata);

			// Build the attribute metadata dictionary

			this.AttributeMetadata = this.attributeMetadataCache.Select(a => this.CreateAttributeMetadataDictionary(a.Item3)).ToArray();

			this.AttributeMetadataSerialized = new JavaScriptSerializer().Serialize(this.AttributeMetadata);
		}
		
		/// <summary>
		/// Recursively clears the <see cref="Order"/> collection and <see cref="FetchAttribute"/> collection from <see cref="Link"/> so they can be merged into another FetchXML query.
		/// </summary>
		/// <param name="link">The <see cref="Link"/> to sanitize.</param>
		private static void RemoveAttributesAndOrdersFromLink(Link link)
		{
			if (link == null)
			{
				return;
			}

			link.Orders = null;

			link.Attributes = null;

			foreach (var nestedLink in link.Links)
			{
				RemoveAttributesAndOrdersFromLink(nestedLink);
			}
		}

		/// <summary>
		/// Given the <see cref="AttributeMetadata"/>, this produces a dictionary of all the attribute metadata properties that is needed by the client side chart rendering.
		/// </summary>
		/// <param name="attributeMetadata">The <see cref="AttributeMetadata"/> to transform into a dictionary.</param>
		/// <returns>A dictionary of attribute metadata properties.</returns>
		private Dictionary<string, object> CreateAttributeMetadataDictionary(AttributeMetadata attributeMetadata)
		{
			var guidDict = new Dictionary<string, string>
			{
				{ "rawguid", attributeMetadata.MetadataId.ToString() }
			};

			var metadata = new Dictionary<string, object>();

			metadata["id"] = guidDict;
			metadata["logicalname"] = attributeMetadata.LogicalName;
			metadata["entitylogicalname"] = attributeMetadata.EntityLogicalName;
			metadata["type"] = attributeMetadata.AttributeTypeName;
			metadata["sourcetype"] = attributeMetadata.SourceType;
			metadata["displayname"] = this.GetLocalizedLabel(attributeMetadata.DisplayName) ?? string.Empty;
			metadata["requiredlevel"] = attributeMetadata.RequiredLevel.Value;
			metadata["issecured"] = attributeMetadata.IsSecured;
			metadata["isvalidforcreate"] = attributeMetadata.IsValidForCreate;
			metadata["isvalidforupdate"] = attributeMetadata.IsValidForUpdate;
			metadata["isvalidforread"] = attributeMetadata.IsValidForRead;
			metadata["attributeof"] = attributeMetadata.AttributeOf;
			metadata["haschanged"] = attributeMetadata.HasChanged.GetValueOrDefault(false);
			metadata["issortableenabled"] = attributeMetadata.IsSortableEnabled;
			metadata["inheritsfrom"] = attributeMetadata.InheritsFrom;
			metadata["imemode"] = 0;
			metadata["maxlength"] = 0;
			metadata["minvalue"] = 0;
			metadata["maxvalue"] = 0;
			metadata["precision"] = 0;
			metadata["precisionsource"] = 0;
			metadata["format"] = 0;
			metadata["behavior"] = 0;
			metadata["defaultformvalue"] = 0;
			metadata["defaultvalue"] = false;
			metadata["isbasecurrency"] = true;
			metadata["islocalizable"] = true;

			return metadata;
		}

		/// <summary>
		/// Given the <see cref="EntityMetadata"/>, this produces a dictionary of all the entity metadata properties that is needed by the client side chart rendering.
		/// </summary>
		/// <param name="entityMetadata">The <see cref="EntityMetadata"/> to be transformed into a dictionary.</param>
		/// <returns>A dictionary of entity metadata properties.</returns>
		private Dictionary<string, object> CreateEntityMetadataDictionary(EntityMetadata entityMetadata)
		{
			var metadata = new Dictionary<string, object>();

			metadata["Id"] = entityMetadata.MetadataId;
			metadata["LogicalName"] = entityMetadata.LogicalName;
			metadata["DisplayName"] = this.GetLocalizedLabel(entityMetadata.DisplayName) ?? string.Empty;
			metadata["PluralName"] = this.GetLocalizedLabel(entityMetadata.DisplayCollectionName) ?? string.Empty;
			metadata["ObjectTypeCode"] = entityMetadata.ObjectTypeCode.GetValueOrDefault(0);
			metadata["PrimaryIdAttribute"] = entityMetadata.PrimaryIdAttribute;
			metadata["PrimaryNameAttribute"] = entityMetadata.PrimaryNameAttribute;
			metadata["EntityColor"] = entityMetadata.EntityColor;

			return metadata;
		}

		/// <summary>
		/// Get the localized string from a <see cref="Label"/>.
		/// </summary>
		/// <param name="label">The <see cref="Label"/> to get the localized string from.</param>
		/// <returns>A localized string.</returns>
		private string GetLocalizedLabel(Label label)
		{
			var lcid = this.contextLanguage.IsCrmMultiLanguageEnabled ? this.contextLanguage.ContextLanguage.CrmLcid : this.culture.LCID;
			var localizedLabel = label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == lcid);

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

		/// <summary>
		/// Creates a dictionary of the strings so we can provide our own values for those declared in the CRM ResourceManager used by the client side chart rendering.
		/// </summary>
		/// <returns>Dictionary of the strings declared in the CRM ResourceManager used by the client side chart rendering.</returns>
		private Dictionary<string, string> CreateResourceManagerStringOverrides()
		{
			var resources = new Dictionary<string, string>
			{
				{ "ActivityContainerControl.SubjectEllipsesText", ResourceManager.GetString("Visualization_SubjectEllipsesText") },
				{ "Web.Visualization.AxisTitle", ResourceManager.GetString("Visualization_AxisTitle") },
				{ "Web.Visualization.AxisTitle.DateGrouping", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping") },
				{ "Web.Visualization.EmptyAxisLabel", ResourceManager.GetString("Visualization_EmptyAxisLabel") },
				{ "Web.Visualization.AxisTitle.AVG", ResourceManager.GetString("Visualization_AxisTitle_AVG") },
				{ "Web.Visualization.AxisTitle.SUM", ResourceManager.GetString("Visualization_AxisTitle_SUM") },
				{ "Web.Visualization.AxisTitle.MIN", ResourceManager.GetString("Visualization_AxisTitle_MIN") },
				{ "Web.Visualization.AxisTitle.MAX", ResourceManager.GetString("Visualization_AxisTitle_MAX") },
				{ "Web.Visualization.AxisTitle.NONE", ResourceManager.GetString("Visualization_AxisTitle_NONE") },
				{ "Web.Visualization.AxisTitle.COUNT", ResourceManager.GetString("Visualization_AxisTitle_COUNT") },
				{ "Web.Visualization.AxisTitle.COUNTCOLUMN", ResourceManager.GetString("Visualization_AxisTitle_COUNTCOLUMN") },
				{ "Web.Visualization.AxisTitle.CurrencySymbol", ResourceManager.GetString("Visualization_AxisTitle_CurrencySymbol") },
				{ "Web.Visualization.AxisTitle.DateGrouping.DAY", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_DAY") },
				{ "Web.Visualization.AxisTitle.DateGrouping.FISCALPERIOD", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_FISCALPERIOD") },
				{ "Web.Visualization.AxisTitle.DateGrouping.FISCALYEAR", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_FISCALYEAR") },
				{ "Web.Visualization.AxisTitle.DateGrouping.MONTH", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_MONTH") },
				{ "Web.Visualization.AxisTitle.DateGrouping.QUARTER", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_QUARTER") },
				{ "Web.Visualization.AxisTitle.DateGrouping.WEEK", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_WEEK") },
				{ "Web.Visualization.AxisTitle.DateGrouping.YEAR", ResourceManager.GetString("Visualization_AxisTitle_DateGrouping_YEAR") },
				{ "Web.Visualization.Year.Quarter", ResourceManager.GetString("Visualization_Year_Quarter") },
				{ "Web.Visualization.Year.Week", ResourceManager.GetString("Visualization_Year_Week") },
				{ "Chart_NoData_Message", ResourceManager.GetString("Visualization_Chart_NoData_Message") }
			};

			var shortMonthCsv = new StringBuilder(string.Empty);
			for (int i = 1; i <= this.culture.Calendar.GetMonthsInYear(DateTime.Now.Year); i++)
			{
				DateTime month = new DateTime(DateTime.Now.Year, i, 1, this.culture.Calendar);
				shortMonthCsv.AppendFormat("{0},", month.ToString("MMM", this.culture));
			}
			resources.Add("Calendar_Short_Months", shortMonthCsv.ToString().TrimEnd(','));

			return resources;
		}

		/// <summary>
		/// Populates a dictionary with entity metadata for each link-entity.
		/// </summary>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> used to retrieve metadata.</param>
		/// <param name="links">The collection of <see cref="Link"/> to iterate over and get entity metadata.</param>
		private void PopulateEntityMetadataCacheForEntityLinks(OrganizationServiceContext serviceContext, IEnumerable<Link> links)
		{
			if (links == null)
			{
				return;
			}

			foreach (var link in links.Where(l => l.Intersect.GetValueOrDefault(false) == false))
			{
				if (this.entityMetadataCache.ContainsKey(link.Name))
				{
					continue;
				}

				var entityMetadata = MetadataHelper.GetEntityMetadata(serviceContext, link.Name);

				this.entityMetadataCache.Add(entityMetadata.LogicalName, entityMetadata);

				this.PopulateEntityMetadataCacheForEntityLinks(serviceContext, link.Links);
			}
		}

		/// <summary>
		/// Adds <see cref="FetchAttribute"/> nested within <see cref="Link"/>s to a list.
		/// </summary>
		/// <param name="links">The collection of <see cref="Link"/>s to iterate.</param>
		private void PopulateAttributeMetadataCacheForEntityLinkAttributes(IEnumerable<Link> links)
		{
			if (links == null)
			{
				return;
			}

			foreach (var link in links.Where(l => l.Attributes != null && l.Attributes.Any()))
			{
				if (this.entityMetadataCache.ContainsKey(link.Name))
				{
					var entityMetadata = this.entityMetadataCache[link.Name];

					foreach (var attribute in link.Attributes)
					{
						var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attribute.Name);

						if (attributeMetadata == null)
						{
							continue;
						}

						this.attributeMetadataCache.Add(new Tuple<string, string, AttributeMetadata>(link.Name, attribute.Name, attributeMetadata));
					}
				}

				this.PopulateAttributeMetadataCacheForEntityLinkAttributes(link.Links);
			}
		}

		/// <summary>
		/// Converts the collection of entity records to a serializable dictionary.
		/// </summary>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> used to retrieve metadata.</param>
		/// <param name="records">The collection of <see cref="Entity"/> records to convert.</param>
		/// <param name="entityMetadata">The <see cref="EntityMetadata"/> to be used to provide metadata for record and attributes.</param>
		/// <returns>List of a dictionary of records that can be serialized into the expected format the CrmHighchartsLibrary.js requires.</returns>
		private List<Dictionary<string, object>> ConvertRecordsToDictionary(OrganizationServiceContext serviceContext, IEnumerable<Entity> records, EntityMetadata entityMetadata)
		{
			var data = new List<Dictionary<string, object>>();
			var organizationMoneyFormatInfo = new OrganizationMoneyFormatInfo(serviceContext);

			foreach (var record in records)
			{
				var row = new Dictionary<string, object>
				{
					{ "RowId", record.Id.ToString("B", CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture) },
					{ "RowType", entityMetadata.ObjectTypeCode.GetValueOrDefault(0).ToString() }
				};

				// DateGrouping require us to also retrieve a datetime value so we can format series labels correctly. Grouping by day for example with a date like 9/23/2016 will result in the value 23 to be stored in the data. The DataDefinition is manually revised to add this aggregate attribute into the fetch so we get the actual date value 9/23/2016 displayed in the chart series.
				KeyValuePair<string, object> groupbyAttribute = new KeyValuePair<string, object>();
				object dategroupValue = null;
				var dategroupAttribute = record.Attributes.FirstOrDefault(a => a.Key.EndsWith("_dategroup_value"));
				if (!string.IsNullOrEmpty(dategroupAttribute.Key))
				{
					var aliasedDateGroupValue = dategroupAttribute.Value as AliasedValue;
					dategroupValue = aliasedDateGroupValue == null ? dategroupAttribute.Value : aliasedDateGroupValue.Value;
					groupbyAttribute = record.Attributes.FirstOrDefault(a =>
						a.Key != dategroupAttribute.Key
						&&
						a.Key.StartsWith(dategroupAttribute.Key.Substring(0, dategroupAttribute.Key.IndexOf("_dategroup_value", StringComparison.InvariantCulture))));
				}

				foreach (var attribute in record.Attributes)
				{
					var aliasedValue = attribute.Value as AliasedValue;
					var value = aliasedValue != null ? aliasedValue.Value : attribute.Value;
					var formattedValue = string.Empty;
					var attributeLogicalName = aliasedValue != null ? aliasedValue.AttributeLogicalName : attribute.Key;
					var attributeEntityLogicalName = aliasedValue != null ? aliasedValue.EntityLogicalName : entityMetadata.LogicalName;
					var attributeMetadata = this.GetAttributeMetadata(attributeLogicalName, attributeEntityLogicalName);
					
					if (record.FormattedValues.Contains(attribute.Key))
					{
						formattedValue = record.FormattedValues[attribute.Key];
					}

					if (attributeMetadata != null && value != null)
					{
						switch (attributeMetadata.AttributeType)
						{
							case AttributeTypeCode.Customer:
							case AttributeTypeCode.Lookup:
							case AttributeTypeCode.Owner:
								var entityReference = value as EntityReference;
								if (entityReference != null)
								{
									formattedValue = entityReference.Name;
								}
								break;
							case AttributeTypeCode.State:
							case AttributeTypeCode.Status:
							case AttributeTypeCode.Picklist:
								var optionSetValue = value as OptionSetValue;
								if (optionSetValue != null)
								{
									formattedValue =
										Adxstudio.Xrm.Core.OrganizationServiceContextExtensions.GetOptionSetValueLabel(attributeMetadata,
											optionSetValue.Value, this.contextLanguage.IsCrmMultiLanguageEnabled ? this.contextLanguage.ContextLanguage.CrmLcid : this.culture.LCID);
								}
								break;
							case AttributeTypeCode.Money:
								var money = value as Money;
								if (money != null)
								{
									value = money.Value;
									var moneyFormatter = new BaseCurrencyMoneyFormatter(organizationMoneyFormatInfo, CultureInfo.CurrentCulture);
									formattedValue = string.Format(moneyFormatter, "{0}", money);
								}
								break;
							case AttributeTypeCode.DateTime:
								if (!string.IsNullOrEmpty(dategroupAttribute.Key) && attribute.Key == groupbyAttribute.Key && dategroupValue != null)
								{
									value = dategroupValue;
								}
								if (value is DateTime)
								{
									formattedValue = ((DateTime)value).ToString(this.culture.DateTimeFormat.ShortDatePattern);
								}
								break;
							case AttributeTypeCode.BigInt:
							case AttributeTypeCode.Integer:
								if (value is int)
								{
									formattedValue = ((int)value).ToString("N", this.culture);
								}
								break;
							case AttributeTypeCode.Decimal:
								var decimalAttributeMetadata = attributeMetadata as DecimalAttributeMetadata;
								if (decimalAttributeMetadata != null && value is decimal)
								{
									formattedValue =
										((decimal)value).ToString(string.Format("N{0}", decimalAttributeMetadata.Precision.GetValueOrDefault(2)),
											this.culture);
								}
								break;
							case AttributeTypeCode.Double:
								var doubleAttributeMetadata = attributeMetadata as DoubleAttributeMetadata;
								if (doubleAttributeMetadata != null && value is double)
								{
									formattedValue =
										((double)value).ToString(string.Format("N{0}", doubleAttributeMetadata.Precision.GetValueOrDefault(2)),
											this.culture);
								}
								break;
						}
					}

					if (string.IsNullOrWhiteSpace(formattedValue))
					{
						try
						{
							formattedValue = value.ToString();
						}
						catch
						{
							// ignored
						}
					}

					if (!string.IsNullOrEmpty(formattedValue))
					{
						formattedValue = formattedValue.Replace("<", "&lt").Replace(">", "&gt");
					}

					row.Add(attribute.Key, formattedValue);
					row.Add(string.Format("{0}_Value", attribute.Key), value);
				}

				data.Add(row);
			}

			return data;
		}

		/// <summary>
		/// Retrieves the <see cref="Microsoft.Xrm.Sdk.Metadata.AttributeMetadata"/> for the specified attribute and entity logical names.
		/// </summary>
		/// <param name="attributeLogicalName">The logical name of the field for which to retrieve the <see cref="Microsoft.Xrm.Sdk.Metadata.AttributeMetadata"/>.</param>
		/// <param name="entityLogicalName">The logical name of the entity that the field belongs to.</param>
		/// <returns><see cref="Microsoft.Xrm.Sdk.Metadata.AttributeMetadata"/></returns>
		private AttributeMetadata GetAttributeMetadata(string attributeLogicalName, string entityLogicalName)
		{
			var attributeMetadata = this.attributeMetadataCache.FirstOrDefault(a => a.Item1 == entityLogicalName && a.Item2 == attributeLogicalName);

			return attributeMetadata == null ? null : attributeMetadata.Item3;
		}

		/// <summary>
		/// If a <see cref="ViewDefinition"/> has been specified, the view's FetchXML will be merged into the chart's FetchXML.
		/// </summary>
		private void MergeViewFetchIntoChartFetch()
		{
			if (this.ChartDefinition == null || this.ChartDefinition.Fetch == null || this.ChartDefinition.Fetch.Entity == null)
			{
				return;
			}

			var fetch = this.ChartDefinition.Fetch;

			if (this.ViewDefinition != null && this.ViewDefinition.Fetch != null && this.ViewDefinition.Fetch.Entity != null)
			{
				var chartFetchHasOrder = this.ChartDefinition.Fetch.Entity.Orders != null && this.ChartDefinition.Fetch.Entity.Orders.Any();

				var viewFetchHasOrder = this.ViewDefinition.Fetch.Entity.Orders != null && this.ViewDefinition.Fetch.Entity.Orders.Any();

				// If no sort order is specified in the chart FetchXml, and a sort order is specified in the view FetchXml, add the order.

				if (!chartFetchHasOrder && viewFetchHasOrder)
				{
					fetch.Entity.Orders = new List<Order>();

					var chartFetchHasAggregates = this.ChartDefinition.Fetch.Aggregate.GetValueOrDefault(false);

					if (chartFetchHasAggregates)
					{
						// For aggregate queries, only honor the primary sort order of the view if the sort is on a groupby attribute in the chart FetchXml.

						var chartFetchGroupByAttributes = this.ChartDefinition.Fetch.Entity.Attributes == null ? null : this.ChartDefinition.Fetch.Entity.Attributes.Where(a => a.Aggregate != null).ToArray();

						if (chartFetchGroupByAttributes != null)
						{
							foreach (var order in this.ViewDefinition.Fetch.Entity.Orders)
							{
								var order1 = order;
								foreach (var attribute in chartFetchGroupByAttributes.Where(attribute => order1.Attribute == attribute.Name))
								{
									// An attribute cannot be specified for an order clause for an aggregate query, must use an alias.

									fetch.Entity.Orders.Add(new Order { Alias = attribute.Alias, Direction = order.Direction ?? OrderType.Ascending });

									break;
								}
							}
						}
					}
					else
					{
						// For non-aggregate queries, always honor the primary sort order of the view.

						fetch.Entity.Orders = this.ViewDefinition.Fetch.Entity.Orders;
					}
				}

				var viewFetch = this.ViewDefinition.Fetch;

				// If the view fetch contains link entities, add them to the fetch query.

				if (viewFetch.Entity.Links != null && viewFetch.Entity.Links.Any())
				{
					var linksExist = fetch.Entity.Links != null && fetch.Entity.Links.Any();

					var sanitizedLinksToAdd = new List<Link>();

					foreach (var link in viewFetch.Entity.Links)
					{
						// If an existing link already exists, do not add a duplicate link

						if (linksExist)
						{
							var linkAlreadyExists = fetch.Entity.Links.Any(existingLink => existingLink.Name == link.Name);

							if (linkAlreadyExists)
							{
								continue;
							}
						}

						RemoveAttributesAndOrdersFromLink(link);

						sanitizedLinksToAdd.Add(link);
					}

					if (!linksExist)
					{
						fetch.Entity.Links = sanitizedLinksToAdd;
					}
					else
					{
						foreach (var link in sanitizedLinksToAdd)
						{
							fetch.Entity.Links.Add(link);
						}
					}
				}

				// If the view fetch contains filters, add them to the fetch query.

				if (viewFetch.Entity.Filters != null && viewFetch.Entity.Filters.Any())
				{
					if (fetch.Entity.Filters != null && fetch.Entity.Filters.Any())
					{
						foreach (var filter in viewFetch.Entity.Filters)
						{
							fetch.Entity.Filters.Add(filter);
						}
					}
					else
					{
						fetch.Entity.Filters = viewFetch.Entity.Filters;
					}
				}
			}

			this.Query = fetch;

			this.FetchXml = this.Query.ToXml().ToString();
		}

		/// <summary>
		/// Get the data for the chart by executing the <see cref="Query"/>. If <see cref="EntityPermissionsEnabled"/> then the <see cref="CrmEntityPermissionProvider"/> is used to apply filters and links to the <see cref="Query"/> to provide record level security filtering.
		/// </summary>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> to be used to make the service call to retrieve the data.</param>
		/// <returns>A collection of <see cref="Entity"/> records.</returns>
		private IEnumerable<Entity> RetrieveData(OrganizationServiceContext serviceContext)
		{
			if (this.Query == null)
			{
				return Enumerable.Empty<Entity>();
			}

			if (this.EntityPermissionsEnabled)
			{
				var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

				var result = crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(serviceContext, CrmEntityPermissionRight.Read, this.Query);

				this.EntityPermissionDenied = !result.GlobalPermissionGranted && !result.PermissionGranted;

				if (this.EntityPermissionDenied)
				{
					return Enumerable.Empty<Entity>();
				}
			}

			this.Query.NoLock = true;

			var response = (RetrieveMultipleResponse)serviceContext.Execute(this.Query.ToRetrieveMultipleRequest());

			var data = response.EntityCollection.Entities;

			return data;
		}
	}
}
