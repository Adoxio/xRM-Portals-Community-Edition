/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Portal
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.Serialization;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Metadata;
	using DateTimeFormat = Microsoft.Xrm.Sdk.Metadata.DateTimeFormat;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Security;
	using DateTimeFormatInfo = Adxstudio.Xrm.Globalization.DateTimeFormatInfo;

	[DataContract]
	public class EntityRecord
	{
		public const string DateTimeClientFormat = DateTimeFormatInfo.ISO8601Pattern;

		public EntityRecord(Entity entity, OrganizationServiceContext serviceContext, CrmEntityPermissionProvider provider, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null, OrganizationMoneyFormatInfo organizationMoneyFormatInfo = null, int? crmLcid = null)
		{
			var entityPermissionResult = provider.TryAssert(serviceContext, entity, entityMetadata, readGranted, regarding);

			CanRead = entityPermissionResult.CanRead;
			CanWrite = entityPermissionResult.CanWrite;
			CanDelete = entityPermissionResult.CanDelete;
			CanAppend = entityPermissionResult.CanAppend;
			CanAppendTo = entityPermissionResult.CanAppendTo;

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			if (statecode != null)
			{
				StateCode = statecode.Value;
			}

			var statuscode = entity.GetAttributeValue<OptionSetValue>("statuscode");

			if (statuscode != null)
			{
				StatusCode = statuscode.Value;
			}

			ConvertToEntityRecord(entity, entityMetadata, serviceContext, organizationMoneyFormatInfo, crmLcid);
		}

		public EntityRecord(Entity entity, EntityMetadata entityMetadata = null, OrganizationServiceContext serviceContext = null, OrganizationMoneyFormatInfo organizationMoneyFormatInfo = null, int? crmLcid = null)
		{
			ConvertToEntityRecord(entity, entityMetadata, serviceContext, organizationMoneyFormatInfo, crmLcid);
		}

		protected void ConvertToEntityRecord(Entity entity, EntityMetadata entityMetadata = null, OrganizationServiceContext serviceContext = null, OrganizationMoneyFormatInfo organizationMoneyFormatInfo = null, int? crmLcid = null)
		{
			var recordAttributes = new List<EntityRecordAttribute>();
			var attributes = entity.Attributes;
			var formattedAttributes = entity.FormattedValues;
			
			if (serviceContext == null)
			{
				serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			}

			organizationMoneyFormatInfo = organizationMoneyFormatInfo ?? new OrganizationMoneyFormatInfo(serviceContext);
			var recordMoneyFormatInfo = new EntityRecordMoneyFormatInfo(serviceContext, entity);

			foreach (var attribute in attributes)
			{
				var aliasedValue = attribute.Value as AliasedValue;
				var value = aliasedValue != null ? aliasedValue.Value : attribute.Value;
				var type = value.GetType().ToString();
				var formattedValue = string.Empty;
				var displayValue = value;
				DateTimeFormat format = DateTimeFormat.DateAndTime;
				DateTimeBehavior behavior = null;
				AttributeMetadata attributeMetadata = null;

				if (formattedAttributes.Contains(attribute.Key))
				{
					formattedValue = formattedAttributes[attribute.Key];
					displayValue = formattedValue;
				}

				if (aliasedValue != null)
				{
					var aliasedEntityMetadata = serviceContext.GetEntityMetadata(aliasedValue.EntityLogicalName, EntityFilters.Attributes);

					if (aliasedEntityMetadata != null)
					{
						attributeMetadata = aliasedEntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == aliasedValue.AttributeLogicalName);
					}
				}
				else
				{
					if (entityMetadata != null)
					{
						attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attribute.Key);
					}
				}

				if (attributeMetadata != null)
				{
					switch (attributeMetadata.AttributeType)
					{
						case AttributeTypeCode.State:
						case AttributeTypeCode.Status:
						case AttributeTypeCode.Picklist:
							var optionSetValue = (OptionSetValue)value;
							formattedValue = Adxstudio.Xrm.Core.OrganizationServiceContextExtensions.GetOptionSetValueLabel(attributeMetadata,
								optionSetValue.Value, crmLcid.GetValueOrDefault(CultureInfo.CurrentCulture.LCID));
							 displayValue = formattedValue;
							break; 
						case AttributeTypeCode.Customer:
						case AttributeTypeCode.Lookup:
						case AttributeTypeCode.Owner:
							var entityReference = value as EntityReference;
							if (entityReference != null)
							{
								displayValue = entityReference.Name ?? string.Empty;
							}
							break;
						case AttributeTypeCode.DateTime:
							var datetimeAttributeMetadata = attributeMetadata as DateTimeAttributeMetadata;
							behavior = datetimeAttributeMetadata.DateTimeBehavior;
							format = datetimeAttributeMetadata.Format.GetValueOrDefault(DateTimeFormat.DateAndTime);
							if (datetimeAttributeMetadata != null)
							{
								if (format != DateTimeFormat.DateOnly && behavior == DateTimeBehavior.UserLocal)
								{
									// Don't use the formatted value, as the connection user's timezone is used to format the datetime value. Use the UTC value for display.
									var date = (DateTime)value;
									displayValue = date.ToString(DateTimeClientFormat);
								}
								if (behavior == DateTimeBehavior.TimeZoneIndependent)
								{
									// JSON serialization converts the time from server local to UTC automatically
									// to avoid this we can convert to UTC before serialization
									value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
								}
							}
							break;
						case AttributeTypeCode.BigInt:
						case AttributeTypeCode.Integer:
							displayValue = string.Format("{0}", value);
							break;
						case AttributeTypeCode.Decimal:
							var decimalAttributeMetadata = attributeMetadata as DecimalAttributeMetadata;
							if (decimalAttributeMetadata != null && value is decimal)
							{
								displayValue = ((decimal)value).ToString("N{0}".FormatWith(decimalAttributeMetadata.Precision.GetValueOrDefault(2)));
							}
							break;
						case AttributeTypeCode.Money:
							var moneyAttributeMetadata = attributeMetadata as MoneyAttributeMetadata;
							if (moneyAttributeMetadata != null && value is Money)
							{
								var moneyFormatter = new MoneyFormatter(organizationMoneyFormatInfo, recordMoneyFormatInfo, moneyAttributeMetadata);

								displayValue = string.Format(moneyFormatter, "{0}", (Money)value);
							}
							break;
					}
				}
				else
				{
					if (attribute.Value is EntityReference)
					{
						var entityReference = (EntityReference)attribute.Value;
						if (entityReference != null)
						{
							displayValue = entityReference.Name ?? string.Empty;
						}
					}
					else if (attribute.Value is DateTime)
					{
						format = DateTimeFormat.DateAndTime;
						var dtAttributeValue = (DateTime)attribute.Value;
						// Don't use the formatted value, as the connection user's timezone is used to format the datetime value. Use the UTC value for display.
						if (dtAttributeValue.Kind == DateTimeKind.Utc) // Indicates this is not a date only attribute
						{
							var date = (DateTime)value;
							displayValue = date.ToString(DateTimeClientFormat);
							behavior = DateTimeBehavior.UserLocal;
						}
						// This below logic fails in one condition: when DateTimeBehavior = TimeZoneIndependent and DateTimeFormat = DateAndTime with value having ex: 20-01-2017 12:00 AM
						else if (dtAttributeValue.TimeOfDay.TotalSeconds == 0) 
						{
							behavior = DateTimeBehavior.DateOnly;
							format = DateTimeFormat.DateOnly;
						}
						else
						{
							behavior = DateTimeBehavior.TimeZoneIndependent;
							// JSON serialization converts the time from server local to UTC automatically
							// to avoid this we can convert to UTC before serialization
							value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
						}
					}
				}

				recordAttributes.Add(new EntityRecordAttribute
				{
					Name = attribute.Key,
					Value = value,
					FormattedValue = formattedValue,
					DisplayValue = displayValue,
					DateTimeBehavior = behavior,
					DateTimeFormat = format.ToString(),
					Type = type,
					AttributeMetadata = attributeMetadata
				});
			}

			Id = entity.Id;
			EntityName = entity.LogicalName;
			Attributes = recordAttributes;
		}

		[DataMember(Name = "id")]
		public Guid Id { get; set; }

		[DataMember]
		public string EntityName { get; set; }

		[DataMember]
		public IEnumerable<EntityRecordAttribute> Attributes { get; set; }

		[DataMember]
		public bool CanRead { get; set; }

		[DataMember]
		public bool CanWrite { get; set; }

		[DataMember]
		public bool CanDelete { get; set; }

		[DataMember]
		public bool CanAppend { get; set; }

		[DataMember]
		public bool CanAppendTo { get; set; }

		[DataMember]
		public int StateCode { get; private set; }

		[DataMember]
		public int StatusCode { get; private set; }
	}

	[DataContract]
	public class EntityRecordAttribute
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public object Value { get; set; }

		[DataMember]
		public string FormattedValue { get; set; }

		[DataMember]
		public string DateTimeFormat { get; set; }

		[DataMember]
		public object DisplayValue { get; set; }

		[DataMember]
		public AttributeMetadata AttributeMetadata { get; set; }

		[DataMember]
		public DateTimeBehavior DateTimeBehavior { get; set; }

	}

	public class PaginatedGridData
	{
		public PaginatedGridData(IEnumerable<EntityRecord> records, int itemCount, int page, int pageSize, IEnumerable<DisabledItemActionLink> disabledLinks = null)
		{
			Records = records;
			ItemCount = itemCount;
			PageCount = itemCount > 0 ? (int)Math.Ceiling(itemCount / (double)pageSize) : 0;
			PageNumber = page;
			PageSize = pageSize;
			DisabledItemActionLinks = disabledLinks;
		}

		public bool MoreRecords { get; set; }
		public IEnumerable<EntityRecord> Records { get; set; }
		public int ItemCount { get; set; }
		public int PageCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }

		public CreateActionMetadata CreateActionMetadata { get; set; }

		public IEnumerable<DisabledItemActionLink> DisabledItemActionLinks { get; set; }
	}

	public class CreateActionMetadata
	{
		public CreateActionMetadata(bool disabled, string disabledMessage = null)
		{
			Disabled = disabled;
			DisabledMessage = disabledMessage;
		}

		public bool Disabled { get; set; }

		public string DisabledMessage { get; set; }

		public static CreateActionMetadata Default = new CreateActionMetadata(false);
	}

	public class EntityPermissionResult
	{
		public EntityPermissionResult(bool accessDenied)
		{
			AccessDenied = accessDenied;
		}

		public bool AccessDenied { get; set; }
	}

	public class DisabledItemActionLink
	{
		public DisabledItemActionLink(Guid entityId, Guid linkUniqueId)
		{
			this.EntityId = entityId;
			this.LinkUniqueId = linkUniqueId;
		}

		public Guid EntityId { get; set; }

		public Guid LinkUniqueId { get; set; }
	}
}
