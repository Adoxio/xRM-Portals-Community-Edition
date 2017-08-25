/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Services.Messages;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Utility class to convert a Subscription message into a Portal Cache message
	/// </summary>
	internal sealed class NotificationMessageTransformer
	{
		private static NotificationMessageTransformer notificationMessageTransformer;

		/// <summary>
		/// Special cased entities and their corresponding name attributes
		/// </summary>
		private static readonly IDictionary<string, string> targetNames = new Dictionary<string, string>
		{
			{ "adx_sitesetting", "adx_name" },
			{ "adx_setting", "adx_name" },
		};

		/// <summary>
		/// Gets the instance of this class
		/// </summary>
		public static NotificationMessageTransformer Instance
		{
			get
			{
				return NotificationMessageTransformer.notificationMessageTransformer ??
						(NotificationMessageTransformer.notificationMessageTransformer = new NotificationMessageTransformer());
			}
		}

		/// <summary>
		/// private constructor
		/// </summary>
		private NotificationMessageTransformer() { }

		/// <summary>
		/// Converts IChangedItem collection into a collection of NotificationMessages
		/// </summary>
		/// <param name="changes">List of IChangedItem</param>
		/// <param name="entityRecordMessages">collection of entityname to EntityRecordMessage</param>
		/// <returns>IEnumerable of NotificationMessage</returns>
		internal IEnumerable<PluginMessage> Convert(List<IChangedItem> changes, Dictionary<string, EntityRecordMessage> entityRecordMessages)
		{
			// Convert IChangedItems to type PluginMessage
			// This is the type of Message that is expected in the axd handlers
			return changes.Select(change => this.CreateMessage(change, entityRecordMessages));
		}

		/// <summary>
		/// Creates a Metadata refresh message
		/// </summary>
		/// <returns>NotificationMessage</returns>
		internal PluginMessage CreateMetadataMessage()
		{
			return new PluginMessage
			{
				MessageName = Constants.Metadata,
			};
		}

		/// <summary>
		/// Casts changed Item to specific type and populates the message.
		/// </summary>
		/// <param name="changedItem">ChangedItem to convert</param>
		/// <param name="entityRecordMessages">collection of entityname to EntityRecordMessage</param>
		/// <returns>Portal cache message converted from ChangedItem</returns>
		private PluginMessage CreateMessage(IChangedItem changedItem, Dictionary<string, EntityRecordMessage> entityRecordMessages)
		{
			switch (changedItem.Type)
			{
				case ChangeType.NewOrUpdated:

					var newOrUpdated = changedItem as NewOrUpdatedItem;
					if (newOrUpdated == null || newOrUpdated.NewOrUpdatedEntity == null)
						break;

					if (!entityRecordMessages.ContainsKey(newOrUpdated.NewOrUpdatedEntity.LogicalName))
					{
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Collection of entity name to EntityRecordMessages is not holding the changed item with entity name {0}, which shouldn't happen.", newOrUpdated.NewOrUpdatedEntity.LogicalName));
						break;
					}

					return this.CreateMessageForNewOrUpdatedItem(newOrUpdated, entityRecordMessages[newOrUpdated.NewOrUpdatedEntity.LogicalName]);
				case ChangeType.RemoveOrDeleted:

					var removedOrDeleted = changedItem as RemovedOrDeletedItem;
					if (removedOrDeleted == null || removedOrDeleted.RemovedItem == null)
						break;

					if (!entityRecordMessages.ContainsKey(removedOrDeleted.RemovedItem.LogicalName))
					{
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Collection of entity name to EntityRecordMessages is not holding the the changed item with entity name {0} which shouldn't happen.", removedOrDeleted.RemovedItem.LogicalName));
						break;
					}

					return this.CreateMessageForRemovedOrDeletedItem(removedOrDeleted, entityRecordMessages[removedOrDeleted.RemovedItem.LogicalName]);
				default:
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("While casting the changed item to specific type, found unhandled ChangeType: {0}", changedItem.Type.ToString()));
					break;
			}

			return null;
		}

		/// <summary>
		/// For special case target entities, get the Name on the target.
		/// </summary>
		/// <param name="entity">Entity to get the name for</param>
		/// <returns>Target's Name if the entity is to be special cased and the attribute exists on the entity passed in</returns>
		private string TryGetTargetName(Entity entity)
		{
			// for particular entities, return the value of the primary name attribute
			string name;

			return entity != null && targetNames.TryGetValue(entity.LogicalName, out name) && entity.Attributes.ContainsKey(name)
				? entity.Attributes[name] as string
				: null;
		}

		/// <summary>
		/// Converts a ChangedItem to a protal cache message
		/// </summary>
		/// <param name="changedItem">ChangedItem to convert</param>
		/// <param name="entityRecordMessage">EntityRecordMessage with the corresponding entityName</param>
		/// <returns>Portal cache message converted from ChangedItem</returns>
		private PluginMessage CreateMessageForNewOrUpdatedItem(NewOrUpdatedItem changedItem, EntityRecordMessage entityRecordMessage)
		{
			if (changedItem == null || entityRecordMessage == null)
				return null;

			PluginMessage message;
			AssociateDisassociateMessage associateDisassociateMessage = entityRecordMessage as AssociateDisassociateMessage;
			string name = this.TryGetTargetName(changedItem.NewOrUpdatedEntity);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Converting ChangedItem with EntityName: {0} and Type:{1} to Portal Cache Message ", changedItem.NewOrUpdatedEntity.LogicalName, changedItem.Type.ToString()));

			if (associateDisassociateMessage == null || changedItem.NewOrUpdatedEntity.LogicalName == "adx_webpageaccesscontrolrule_webrole")
			{
				message = new PluginMessage
				{
					MessageName = Constants.CreatedOrUpdated,
					Target = new PluginMessageEntityReference
					{
						Name = name,
						Id = changedItem.NewOrUpdatedEntity.Id,
						LogicalName = changedItem.NewOrUpdatedEntity.LogicalName,
					},
					RelatedEntities = null,
					Relationship = null,
				};
			}
			else
			{
				message = new PluginMessage
				{
					MessageName = Constants.Associate,
					Target = new PluginMessageEntityReference
					{
						Name = name,
						Id = (Guid)changedItem.NewOrUpdatedEntity.Attributes
						[
							CrmChangeTrackingManager.Instance.TryGetPrimaryKey(associateDisassociateMessage.RelatedEntity1Name)
						],
						LogicalName = associateDisassociateMessage.RelatedEntity1Name,
					},
					RelatedEntities = new List<PluginMessageEntityReference>
					{
						new PluginMessageEntityReference
						{
							Name = null,
							Id = (Guid)changedItem.NewOrUpdatedEntity.Attributes
							[
								CrmChangeTrackingManager.Instance.TryGetPrimaryKey(associateDisassociateMessage.RelatedEntity2Name)
							],
							LogicalName = associateDisassociateMessage.RelatedEntity2Name,
						}
					},
					Relationship = new PluginMessageRelationship
					{
						PrimaryEntityRole = null,
						SchemaName = associateDisassociateMessage.RelationshipName,
					},
				};
			}

			return message;
		}

		/// <summary>
		/// Converts a ChangedItem to a protal cache message
		/// </summary>
		/// <param name="changedItem">ChangedItem to convert</param>
		/// <param name="entityRecordMessage">EntityRecordMessage with the corresponding entityName</param>
		/// <returns>Portal cache message converted from ChangedItem</returns>
		private PluginMessage CreateMessageForRemovedOrDeletedItem(RemovedOrDeletedItem changedItem, EntityRecordMessage entityRecordMessage)
		{
			if (changedItem == null)
				return null;

			PluginMessage message;
			AssociateDisassociateMessage associateDisassociateMessage = entityRecordMessage as AssociateDisassociateMessage;

			ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Converting ChangedItem/Deleted with EntityName: {0} and Type:{1} to Portal Cache Message", changedItem.RemovedItem.LogicalName, changedItem.Type.ToString()));

			if (associateDisassociateMessage == null || changedItem.RemovedItem.LogicalName == "adx_webpageaccesscontrolrule_webrole")
			{
				message = new PluginMessage
				{
					MessageName = Constants.RemovedOrDeleted,
					Target = new PluginMessageEntityReference
					{
						Name = null,
						Id = changedItem.RemovedItem.Id,
						LogicalName = changedItem.RemovedItem.LogicalName,
					},
				};
			}
			else
			{
				message = new PluginMessage
				{
					MessageName = Constants.Disassociate,
					Target = new PluginMessageEntityReference
					{
						// Special Handling : We want this information to pass to adx code for dissociate and message schema is fixed / nothing can be added, this name/guid would have been null/empty else .
						Name = associateDisassociateMessage.EntityName,
						Id = changedItem.RemovedItem.Id,
						LogicalName = associateDisassociateMessage.RelatedEntity1Name,
					},
					RelatedEntities = new List<PluginMessageEntityReference>
					{
						new PluginMessageEntityReference
						{
							Name = null,
							Id = Guid.Empty,
							LogicalName = associateDisassociateMessage.RelatedEntity2Name,
						}
					},
					Relationship = new PluginMessageRelationship
					{
						PrimaryEntityRole = null,
						SchemaName = associateDisassociateMessage.RelationshipName,
					},
				};
			}
			return message;
		}
	}
}
