/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Portal.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.Mvc;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Metadata;
	using Adxstudio.Xrm;
	using Adxstudio.Xrm.Activity;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Text;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Order = Adxstudio.Xrm.Services.Query.Order;

	/// <summary>
	/// Controller that responds to ActivityPointer requests for information.
	/// </summary>
	public sealed class EntityActivityController : Controller
	{
		public sealed class ActivityRecord : EntityRecord
		{
			private readonly string[] predefinedTemplates = new string[] { "email", "phonecall", "appointment", "adx_portalcomment" };
			public IDictionary<string, object> ViewFields { get; set; }

			public DateTime CreatedOn { get; private set; }

			public string CreatedOnDisplay { get; private set; }

			public string PostedByName { get; private set; }

			public bool DisplayToolbar { get; private set; }
			public bool IsCustomActivity { get; private set; }

			public ActivityRecord(IActivity activity, DataAdapterDependencies dataAdapterDependencies,
				CrmEntityPermissionProvider provider, EntityMetadata entityMetadata = null, bool readGranted = false, int? crmLcid = null)
				: base(
					activity.Entity, dataAdapterDependencies.GetServiceContext(), provider, entityMetadata, readGranted,
					activity.Regarding, crmLcid: crmLcid)
			{
				if (activity == null) throw new ArgumentNullException("activity");

				SetPropertyValues(activity, dataAdapterDependencies);
			}

			private void SetPropertyValues(IActivity activity, DataAdapterDependencies dataAdapterDependencies)
			{
				var attributes = activity.Entity.Attributes;

				IsCustomActivity = false;

				ViewFields = attributes.SelectMany(FlattenAllPartiesAttribute).ToDictionary(attribute => attribute.Key, attribute =>
				{
					var optionSetValue = attribute.Value as OptionSetValue;

					if (optionSetValue != null)
					{
						return optionSetValue.Value;
					}

					if (attribute.Key == "activitytypecode" && !predefinedTemplates.Contains((string)attribute.Value))
					{
						IsCustomActivity = true;
					}

					if (attribute.Key == "description")
					{
						string formattedValue = FormatViewFieldsValue(attribute.Value);
						if (!String.IsNullOrWhiteSpace(formattedValue))
						{
							return formattedValue;
						}
					}
					return attribute.Value;
				});

				CreatedOn = activity.Entity.GetAttributeValue<DateTime>("createdon");
				CreatedOnDisplay = CreatedOn.ToString(DateTimeClientFormat);

				var noteContact = activity.Entity.GetAttributeValue<EntityReference>("from");
				PostedByName = noteContact == null
					? activity.Entity.GetAttributeValue<EntityReference>("createdby").Name
					: noteContact.Name;

				DisplayToolbar = false;
			}

			/// <summary>
			/// If valueObj can be converted to string - formats it with SimpleHtmlFormatter.
			/// </summary>
			/// <param name="valueObj">attribute.Value</param>
			/// <returns>Formatted string if success. Otherwise null</returns>
			private string FormatViewFieldsValue(object valueObj)
			{
				string valueText = valueObj as string;
				if (String.IsNullOrWhiteSpace(valueText))
				{
					return null;
				}
				try
				{
					string formattedText =
						(new SimpleHtmlFormatter().Format(valueText)).ToString();
					return formattedText;

				}
				catch (Exception)
				{
					return null;
				}
			}
		}

		private static IEnumerable<KeyValuePair<string, object>> FlattenAllPartiesAttribute(KeyValuePair<string, object> attribute)
		{
			var attributeCollection = new List<KeyValuePair<string, object>> { };
			var toRecipients = new List<EntityReference>();
			var ccRecipients = new List<EntityReference>();
			var requiredAttendees = new List<EntityReference>();

			if (attribute.Key.Equals("allparties"))
			{
				// Iterate through each entity in allparties and assign to Sender, To, or CC
				foreach (var entity in ((EntityCollection)attribute.Value).Entities.Where(entity => entity.Attributes.ContainsKey("participationtypemask") && entity.Attributes.ContainsKey("partyid")))
				{
					switch (entity.GetAttributeValue<OptionSetValue>("participationtypemask").Value)
					{
						// Sender or Organizer should be represented as "from"
						case (int)Activity.ParticipationTypeMaskOptionSetValue.Sender:
						case (int)Activity.ParticipationTypeMaskOptionSetValue.Organizer:
							attributeCollection.Add(new KeyValuePair<string, object>("from", entity.GetAttributeValue<EntityReference>("partyid")));
							break;
						case (int)Activity.ParticipationTypeMaskOptionSetValue.ToRecipient:
							toRecipients.Add(entity.GetAttributeValue<EntityReference>("partyid"));
							break;
						case (int)Activity.ParticipationTypeMaskOptionSetValue.CcRecipient:
							ccRecipients.Add(entity.GetAttributeValue<EntityReference>("partyid"));
							break;
						case (int)Activity.ParticipationTypeMaskOptionSetValue.RequiredAttendee:
							requiredAttendees.Add(entity.GetAttributeValue<EntityReference>("partyid"));
							break;
					}
				}

				// flatten lists for to and cc recipient
				if (toRecipients.Any())
				{
					attributeCollection.Add(new KeyValuePair<string, object>("to", toRecipients));
				}
				if (ccRecipients.Any())
				{
					attributeCollection.Add(new KeyValuePair<string, object>("cc", ccRecipients));
				}
				if (requiredAttendees.Any())
				{
					attributeCollection.Add(new KeyValuePair<string, object>("requiredattendees", requiredAttendees));
				}
			}
			else
			{
				attributeCollection.Add(attribute);
			}
			return attributeCollection;
		}

		private const int DefaultPageSize = 10;

		/// <summary>
		/// Retrieves Json representation of Activity Pointers filtered by regarding filter.
		/// </summary>
		/// <param name="regarding"></param>
		/// <param name="orders"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult GetActivities(EntityReference regarding, List<Order> orders, int page, int pageSize = DefaultPageSize)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var dataAdapter = new ActivityDataAdapter(dataAdapterDependencies);
			var entityMetadata = portalContext.ServiceContext.GetEntityMetadata(regarding.LogicalName, EntityFilters.All);
			var result = dataAdapter.GetActivities(regarding, orders, page, pageSize, entityMetadata);
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var crmLcid = HttpContext.GetCrmLcid();
			var records = result.Select(r => new ActivityRecord(r, dataAdapterDependencies, entityPermissionProvider, entityMetadata, true, crmLcid)).ToArray();
			var data = new PaginatedGridData(records, result.TotalCount, page, pageSize);

			return new JsonResult { Data = data, MaxJsonLength = int.MaxValue };
		}

		/// <summary>
		/// Method for creating a PortalComment entity. Will auto-populate From field with regarding entity's owner,
		/// To field with portal user, DirectionCode with Incoming, State code with Completed, and Status code
		/// with Received.
		/// </summary>
		/// <param name="regardingEntityLogicalName"></param>
		/// <param name="regardingEntityId"></param>
		/// <param name="text"></param>
		/// <param name="file"></param>
		/// <param name="attachmentSettings"></param>
		/// <returns></returns>
		[HttpPost]
		[AjaxFormStatusResponse]
		[ValidateAntiForgeryToken]
		public ActionResult AddPortalComment(string regardingEntityLogicalName, string regardingEntityId, string text,
			HttpPostedFileBase file = null, string attachmentSettings = null)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(StringHelper.StripHtml(text)))
			{
				return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed, ResourceManager.GetString("Required_Field_Error").FormatWith(ResourceManager.GetString("Comment_DefaultText")));
			}

			Guid regardingId;
			Guid.TryParse(regardingEntityId, out regardingId);
			var regarding = new EntityReference(regardingEntityLogicalName, regardingId);
			
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var dataAdapter = new ActivityDataAdapter(dataAdapterDependencies);
			var settings = EntityNotesController.GetAnnotationSettings(serviceContext, attachmentSettings);
			var crmUser = dataAdapter.GetCRMUserActivityParty(regarding, "ownerid");
			var portalUser = new Entity("activityparty");
			portalUser["partyid"] = dataAdapterDependencies.GetPortalUser();

			var portalComment = new PortalComment
			{
				Description = text,
				From = portalUser,
				To = crmUser,
				Regarding = regarding,
				AttachmentSettings = settings,
				StateCode = StateCode.Completed,
				StatusCode = StatusCode.Received,
				DirectionCode = PortalCommentDirectionCode.Incoming
			};

			if (file != null && file.ContentLength > 0)
			{
				// Soon we will change the UI/controller to accept multiple attachments during the create dialog, so the data adapter takes in a list of attachments
				portalComment.FileAttachments = new IAnnotationFile[]
				{ AnnotationDataAdapter.CreateFileAttachment(file, settings.StorageLocation) };
			}

			var result = dataAdapter.CreatePortalComment(portalComment);

			if (!result.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
					ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			if (!result.CanCreate)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
					ResourceManager.GetString("No_Permissions_To_Create_Notes"));
			}

			if (!result.CanAppendTo)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
					ResourceManager.GetString("No_Permissions_To_Append_Record"));
			}

			if (!result.CanAppend)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
					ResourceManager.GetString("No_Permissions_To_Append_Notes"));
			}

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Comments, this.HttpContext, "create_comment_" + regardingEntityLogicalName, 1, regarding, "create");
			}

			return new HttpStatusCodeResult(HttpStatusCode.Created);
		}

		/// <summary>
		/// Retrieves Json representation of Attachments filtered by regarding filter.
		/// </summary>
		/// <param name="regarding"></param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult GetAttachments(EntityReference regarding)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var dataAdapter = new ActivityDataAdapter(dataAdapterDependencies);
			var attachments = dataAdapter.GetAttachments(regarding).ToArray();
		
			if (attachments.Any())
			{
				return new JsonResult { Data = attachments, MaxJsonLength = int.MaxValue };
			}
			return new EmptyResult();
		}
	}
}
