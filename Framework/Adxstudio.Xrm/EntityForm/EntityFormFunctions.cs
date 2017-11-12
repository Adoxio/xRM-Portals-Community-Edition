/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EntityForm
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.UI.EntityForm;
	using Adxstudio.Xrm.Web.UI.WebControls;
	using Adxstudio.Xrm.Globalization;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;

	internal class EntityFormFunctions
	{
		internal static string DefaultPreviousButtonCssClass = "button previous";
		internal static string DefaultNextButtonCssClass = "button next";
		internal static string DefaultSubmitButtonCssClass = "button submit";
		public static readonly string DefaultAttachFileLabel = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Attach_A_File_DefaultText"));
		public static readonly string DefaultPreviousButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Previous_Button_Label"));
		public static readonly string DefaultNextButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Next_Button_Text"));
		public static readonly string DefaultSubmitButtonText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Submit_Button_Label_Text"));
		public static readonly string DefaultSubmitButtonBusyText = HttpUtility.JavaScriptStringEncode(ResourceManager.GetString("Default_Modal_Processing_Text"));

		internal static bool TryGetRecord(OrganizationServiceContext context, FormEntitySourceDefinition definition, out Entity record)
		{
			record = context.CreateQuery(definition.LogicalName).FirstOrDefault(o => o.GetAttributeValue<Guid>(definition.PrimaryKeyLogicalName) == definition.ID);

			return record != null;
		}

		internal static void TrySetState(OrganizationServiceContext context, EntityReference entityReference, int state)
		{
			try
			{
				context.SetState(state, -1, entityReference);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to set statecode. {0}", ex.ToString()));
			}
		}

		internal static string TryConvertAttributeValueToString(OrganizationServiceContext context, Dictionary<string, AttributeTypeCode?> attributeTypeCodeDictionary, string entityName, string attributeName, object value)
		{
			if (context == null || string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(attributeName))
			{
				return string.Empty;
			}

			var newValue = string.Empty;
			var attributeTypeCode = attributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Unable to recognize the attribute specified.");
				return string.Empty;
			}

			try
			{
				switch (attributeTypeCode)
				{
					case AttributeTypeCode.BigInt:
						newValue = value == null ? string.Empty : Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Boolean:
						newValue = value == null ? string.Empty : Convert.ToBoolean(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Customer:
						if (value is EntityReference)
						{
							var entityref = value as EntityReference;
							newValue = entityref.Id.ToString();
						}
						break;
					case AttributeTypeCode.DateTime:
						newValue = value == null ? string.Empty : Convert.ToDateTime(value).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Decimal:
						newValue = value == null ? string.Empty : Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Double:
						newValue = value == null ? string.Empty : Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Integer:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Lookup:
						if (value is EntityReference)
						{
							var entityref = value as EntityReference;
							newValue = entityref.Id.ToString();
						}
						break;
					case AttributeTypeCode.Memo:
						newValue = value as string;
						break;
					case AttributeTypeCode.Money:
						newValue = value == null ? string.Empty : Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Picklist:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.State:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Status:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.String:
						newValue = value as string;
						break;
					case AttributeTypeCode.Uniqueidentifier:
						if (value is Guid)
						{
							var id = (Guid)value;
							newValue = id.ToString();
						}
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported.", attributeTypeCode));
						break;
				}
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute specified is expecting a {0}. The value provided is not valid.", attributeTypeCode));
			}
			return newValue;
		}

		internal static void DisplayMessage(Web.UI.WebControls.EntityForm sender, string message, string cssClass, bool hideForm)
		{
			if (sender == null || string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			DisplayMessage(message, cssClass, hideForm, sender);
		}

		internal static void DisplayMessage(object sender, string message, string cssClass, bool hideForm)
		{
			var formView = (CrmEntityFormView)sender;
			var container = formView.Parent;

			DisplayMessage(message, cssClass, hideForm, container);
		}

		internal static void DisplayMessage(Control container, string message, string cssClass, bool hideForm)
		{
			if (container == null || string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			DisplayMessage(message, cssClass, hideForm, container);
		}

		internal static void DisplayMessage(string message, string cssClass, bool hideForm, Control container)
		{
			var messagePanel = (Panel)container.FindControl("MessagePanel");
			var formPanel = (Panel)container.FindControl("EntityFormPanel");
			var messageLabel = (System.Web.UI.WebControls.Label)container.FindControl("MessageLabel");

			if (messagePanel == null)
			{
				messagePanel = new Panel { ID = "MessagePanel", CssClass = "message alert" };

				container.Controls.Add(messagePanel);
			}

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			if (formPanel != null)
			{
				formPanel.Visible = !hideForm;
			}

			if (messageLabel == null)
			{
				messageLabel = new System.Web.UI.WebControls.Label
				{
					ID = "MessageLabel",
					Text = string.IsNullOrWhiteSpace(message) ? string.Empty : message
				};

				messagePanel.Controls.Add(messageLabel);
			}
			else
			{
				messageLabel.Text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
			}

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			messagePanel.Visible = true;
		}

		internal static void Associate(OrganizationServiceContext serviceContext, EntityReference related)
		{
			var targetEntityLogicalName = HttpContext.Current.Request["refentity"];
			var targetEntityId = HttpContext.Current.Request["refid"];
			var relationshipName = HttpContext.Current.Request["refrel"];
			var relationshipRole = HttpContext.Current.Request["refrelrole"];
			Guid targetId;

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName) || string.IsNullOrWhiteSpace(targetEntityId) ||
				string.IsNullOrWhiteSpace(relationshipName) || related == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Request did not contain parameters 'refentity', 'refid', 'refrel'");
				return;
			}

			if (!Guid.TryParse(targetEntityId, out targetId))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Request did not contain a valid guid 'refid'");

				return;
			}

			try
			{
				var relationship = new Relationship(relationshipName);

				if (!string.IsNullOrWhiteSpace(relationshipRole))
				{
					switch (relationshipRole)
					{
						case "Referenced":
							relationship.PrimaryEntityRole = EntityRole.Referenced;
							break;
						case "Referencing":
							relationship.PrimaryEntityRole = EntityRole.Referencing;
							return;
							break;
						default:
							ADXTrace.Instance.TraceError(TraceCategory.Application, "Relationship Primary Entity Role provided by parameter named 'refrelrole' is not valid.");
							break;
					}
				}

				var associateRequest = new AssociateRequest
				{
					Target = new EntityReference(targetEntityLogicalName, targetId),
					Relationship = relationship,
					RelatedEntities = new EntityReferenceCollection { related }
				};

				serviceContext.Execute(associateRequest);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());

			}
		}

		internal static void AssociateEntity(OrganizationServiceContext context, Entity entityform, Guid sourceEntityId)
		{
			var setEntityReference = entityform.GetAttributeValue<bool?>("adx_setentityreference") ?? false;

			if (!setEntityReference) return;

			var targetEntityId = Guid.Empty;
			var targetEntityPrimaryKey = string.Empty;
			var sourceEntityName = entityform.GetAttributeValue<string>("adx_entityname");
			var sourceEntityPrimaryKey = entityform.GetAttributeValue<string>("adx_primarykeyname");
			var targetEntityName = entityform.GetAttributeValue<string>("adx_referenceentitylogicalname");
			var relationshipName = entityform.GetAttributeValue<string>("adx_referenceentityrelationshipname") ?? string.Empty;

			if (string.IsNullOrWhiteSpace(relationshipName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Entity Relationship Name not provided. Entity Association not required.");
				return;
			}

			try
			{
				var referenceQueryStringName = entityform.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
				var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
				var querystringIsPrimaryKey = entityform.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;

				if (!querystringIsPrimaryKey)
				{
					var referenceQueryAttributeName = entityform.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
					var entity =
						context.CreateQuery(targetEntityName).FirstOrDefault(
							o => o.GetAttributeValue<string>(referenceQueryAttributeName) == referenceQueryStringValue);

					if (entity != null) { targetEntityId = entity.Id; }
				}
				else
				{
					Guid.TryParse(referenceQueryStringValue, out targetEntityId);
				}

				if (sourceEntityId == Guid.Empty || targetEntityId == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Source and Target entity ids must not be null or empty.");
					return;
				}

				// get the source entity

				if (string.IsNullOrWhiteSpace(sourceEntityName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_targetentitylogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(sourceEntityPrimaryKey))
				{
					sourceEntityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, sourceEntityName);
				}

				if (string.IsNullOrWhiteSpace(sourceEntityPrimaryKey))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to determine source entity primary key logical name.");
					return;
				}

				var sourceEntity = context.CreateQuery(sourceEntityName).FirstOrDefault(o => o.GetAttributeValue<Guid>(sourceEntityPrimaryKey) == sourceEntityId);

				if (sourceEntity == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Source entity is null.");
					return;
				}

				// Get the target entity

				if (string.IsNullOrWhiteSpace(targetEntityName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Target entity name must not be null or empty.");
					return;
				}

				if (string.IsNullOrWhiteSpace(targetEntityPrimaryKey))
				{
					targetEntityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, targetEntityName);
				}

				if (string.IsNullOrWhiteSpace(targetEntityPrimaryKey))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to determine target entity primary key logical name.");
					return;
				}

				var targetEntity = context.CreateQuery(targetEntityName).FirstOrDefault(o => o.GetAttributeValue<Guid>(targetEntityPrimaryKey) == targetEntityId);

				if (targetEntity == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Target entity is null.");
					return;
				}

				context.AddLink(sourceEntity, relationshipName, targetEntity);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("{0}", ex.ToString()));
			}
		}

		internal static void CalculateValueOpportunity()
		{
			if (string.IsNullOrEmpty(HttpContext.Current.Request["refid"])) return;

			var targetEntityId = HttpContext.Current.Request["refid"];

			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var adapter = new CoreDataAdapter(portal, serviceContext);

			Guid id;

			if (!Guid.TryParse(targetEntityId, out id)) return;

			if (string.IsNullOrEmpty(HttpContext.Current.Request["refentity"])) return;

			var entityReference = new EntityReference(HttpContext.Current.Request["refentity"], id);

			adapter.CalculateActualValueOfOpportunity(entityReference);

			var entity =
				serviceContext.CreateQuery(entityReference.LogicalName)
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("opportunityid") == entityReference.Id);

			if (entity == null) return;

			serviceContext.TryRemoveFromCache(entity);

			serviceContext.UpdateObject(entity);

			serviceContext.SaveChanges();
		}

		internal static dynamic TryConvertAttributeValue(OrganizationServiceContext context, string entityName, string attributeName, object value, Dictionary<string, AttributeTypeCode?> AttributeTypeCodeDictionary)
		{

			if (context == null || string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(attributeName)) return null;

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entityName);
			}

			object newValue = null;
			var attributeTypeCode = AttributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Unable to recognize the attribute '{0}' specified.", attributeName));
				return null;
			}

			try
			{
				switch (attributeTypeCode)
				{
					case AttributeTypeCode.BigInt:
						newValue = value == null ? (object)null : Convert.ToInt64(value);
						break;
					case AttributeTypeCode.Boolean:
						newValue = value == null ? (object)null : Convert.ToBoolean(value);
						break;
					case AttributeTypeCode.Customer:
						if (value is EntityReference)
						{
							newValue = value as EntityReference;
						}
						else if (value is Guid)
						{
							var metadata = MetadataHelper.GetEntityMetadata(context, entityName);
							var attribute = metadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);
							if (attribute != null)
							{
								var lookupAttribute = attribute as LookupAttributeMetadata;
								if (lookupAttribute != null && lookupAttribute.Targets.Length == 1)
								{
									var lookupEntityType = lookupAttribute.Targets[0];
									newValue = new EntityReference(lookupEntityType, (Guid)value);
								}
							}
						}
						break;
					case AttributeTypeCode.DateTime:
						newValue = value == null ? (object)null : Convert.ToDateTime(value).ToUniversalTime();
						break;
					case AttributeTypeCode.Decimal:
						newValue = value == null ? (object)null : Convert.ToDecimal(value);
						break;
					case AttributeTypeCode.Double:
						newValue = value == null ? (object)null : Convert.ToDouble(value);
						break;
					case AttributeTypeCode.Integer:
						newValue = value == null ? (object)null : Convert.ToInt32(value);
						break;
					case AttributeTypeCode.Lookup:
						if (value is EntityReference)
						{
							newValue = value as EntityReference;
						}
						else if (value is Guid)
						{
							var metadata = MetadataHelper.GetEntityMetadata(context, entityName);
							var attribute = metadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);
							if (attribute != null)
							{
								var lookupAttribute = attribute as LookupAttributeMetadata;
								if (lookupAttribute != null && lookupAttribute.Targets.Length == 1)
								{
									var lookupEntityType = lookupAttribute.Targets[0];
									newValue = new EntityReference(lookupEntityType, (Guid)value);
								}
							}
						}
						break;
					case AttributeTypeCode.Memo:
						newValue = value as string;
						break;
					case AttributeTypeCode.Money:
						newValue = value == null ? (object)null : Convert.ToDecimal(value);
						break;
					case AttributeTypeCode.Picklist:
						var plMetadata = MetadataHelper.GetEntityMetadata(context, entityName);
						var plAttribute = plMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);
						if (plAttribute != null)
						{
							var picklistAttribute = plAttribute as PicklistAttributeMetadata;
							if (picklistAttribute != null)
							{
								int picklistInt;
								OptionMetadata picklistValue;
								if (int.TryParse(string.Empty + value, out picklistInt))
								{
									picklistValue = picklistAttribute.OptionSet.Options.FirstOrDefault(o => o.Value == picklistInt);
								}
								else
								{
									picklistValue = picklistAttribute.OptionSet.Options.FirstOrDefault(o => o.Label.GetLocalizedLabelString() == string.Empty + value);
								}

								if (picklistValue != null && picklistValue.Value.HasValue)
								{
									newValue = value == null ? null : new OptionSetValue(picklistValue.Value.Value);
								}
							}
						}
						break;
					case AttributeTypeCode.State:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute '{0}' type '{1}' is unsupported. The state attribute is created automatically when the entity is created. The options available for this attribute are read-only.", attributeName, attributeTypeCode));
						break;
					case AttributeTypeCode.Status:
						if (value == null)
						{
							return false;
						}
						var optionSetValue = new OptionSetValue(Convert.ToInt32(value));
						newValue = optionSetValue;
						break;
					case AttributeTypeCode.String:
						newValue = value as string;
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute '{0}' type '{1}' is unsupported.", attributeName, attributeTypeCode));
						break;
				}
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute '{0}' specified is expecting a {1}. The value provided is not valid.", attributeName, attributeTypeCode));
			}
			return newValue;
		}
	}
}
