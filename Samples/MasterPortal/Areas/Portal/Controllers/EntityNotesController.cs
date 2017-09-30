/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Portal.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Security;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Newtonsoft.Json;
	using Adxstudio.Xrm;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Text;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Order = Adxstudio.Xrm.Services.Query.Order;

	public class EntityNotesController : Controller
	{
		public class NoteRecord : EntityRecord
		{
			public string AttachmentUrl { get; private set; }

			public string AttachmentContentType { get; private set; }

			public string AttachmentFileName { get; private set; }

			public bool AttachmentIsImage { get; private set; }

			public FileSize AttachmentSize { get; private set; }

			public string AttachmentSizeDisplay { get; private set; }

			public DateTime CreatedOn { get; private set; }

			public string CreatedOnDisplay { get; private set; }

			public bool HasAttachment { get; private set; }

			public string Subject { get; private set; }

			public string Text { get; private set; }

			public string UnformattedText { get; private set; }

			public bool IsPostedByCurrentUser { get; private set; }

			public string PostedByName { get; private set; }

			public bool DisplayToolbar { get; private set; }

			public bool IsPrivate { get; private set; }

			public NoteRecord(IAnnotation annotation, DataAdapterDependencies dataAdapterDependencies, CrmEntityPermissionProvider provider, EntityMetadata entityMetadata = null, bool readGranted = false, int? crmLcid = null)
				: base(annotation.Entity, dataAdapterDependencies.GetServiceContext(), provider, entityMetadata, readGranted, annotation.Regarding, crmLcid: crmLcid)
			{
				if (annotation == null) throw new ArgumentNullException("annotation");

				SetPropertyValues(annotation, dataAdapterDependencies);
			}

			protected void SetPropertyValues(IAnnotation annotation, DataAdapterDependencies dataAdapterDependencies)
			{
				CreatedOn = annotation.CreatedOn;
				CreatedOnDisplay = CreatedOn.ToString(DateTimeClientFormat);
				var text = annotation.NoteText;
				Text = AnnotationHelper.FormatNoteText(text).ToString();
				UnformattedText = text.Replace(AnnotationHelper.WebAnnotationPrefix, string.Empty);
				if (annotation.FileAttachment != null)
				{
					AttachmentFileName = annotation.FileAttachment.FileName;
					HasAttachment = annotation.FileAttachment != null;
					AttachmentContentType = annotation.FileAttachment.MimeType;
					AttachmentUrl = HasAttachment
						? annotation.Entity.GetFileAttachmentUrl(dataAdapterDependencies.GetWebsite())
						: string.Empty;
					AttachmentSize = annotation.FileAttachment.FileSize;
					AttachmentSizeDisplay = AttachmentSize.ToString();
					AttachmentIsImage = HasAttachment &&
						(new List<string> { "image/jpeg", "image/gif", "image/png" }).Contains(AttachmentContentType);
				}
				var subject = annotation.Subject;
				Subject = subject;
				IsPrivate = AnnotationHelper.GetNotePrivacy(annotation);
				var noteContact = AnnotationHelper.GetNoteContact(subject);
				var user = dataAdapterDependencies.GetPortalUser();
				IsPostedByCurrentUser = noteContact != null && user != null && noteContact.Id == user.Id;
				PostedByName = noteContact == null ? AnnotationHelper.GetNoteCreatedByName(annotation) : noteContact.Name;
				if (CanWrite)
				{
					CanWrite = IsPostedByCurrentUser;
				}
				if (CanDelete)
				{
					CanDelete = IsPostedByCurrentUser;
				}
				DisplayToolbar = CanWrite || CanDelete;
			}
		}

		private const int DefaultPageSize = 10;

		[AcceptVerbs(HttpVerbs.Post)]
        [AjaxValidateAntiForgeryToken]
		public ActionResult GetNotes(EntityReference regarding, List<Order> orders, int page, int pageSize = DefaultPageSize)
		{
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var entityMetadata = portalContext.ServiceContext.GetEntityMetadata(regarding.LogicalName, EntityFilters.All);
			var result = dataAdapter.GetAnnotations(regarding, orders, page, pageSize, entityMetadata: entityMetadata);
			var totalRecordCount = result.TotalCount;
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			var crmLcid = HttpContext.GetCrmLcid();
			var records = result.Select(r => new NoteRecord(r, dataAdapterDependencies, entityPermissionProvider, entityMetadata, true, crmLcid));
			var data = new PaginatedGridData(records, totalRecordCount, page, pageSize);

			return new JsonResult { Data = data, MaxJsonLength = int.MaxValue };
		}

		protected void AddPaginationToFetch(Fetch fetch, string cookie, int page, int count, bool returnTotalRecordCount)
		{
			if (cookie != null)
			{
				fetch.PagingCookie = cookie;
			}

			fetch.PageNumber = page;

			fetch.PageSize = count;

			fetch.ReturnTotalRecordCount = returnTotalRecordCount;
		}

		/// <summary>
		/// Executes the Fetch and returns the resulting records.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="fetch"></param>
		/// <param name="permissionDenied"></param>
		/// <returns><see cref="FetchResult"/></returns>
		protected virtual FetchResult FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch, bool permissionDenied = false)
		{
			if (fetch == null || permissionDenied)
			{
				return new FetchResult(Enumerable.Empty<Entity>(), 0, permissionDenied);
			}

			var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

			if (!string.IsNullOrEmpty(response.EntityCollection.PagingCookie))
			{
				fetch.PagingCookie = response.EntityCollection.PagingCookie;
			}

			return new FetchResult(response.EntityCollection.Entities, response.EntityCollection.TotalRecordCount);
		}

		/// <summary>
		/// Result returned by executing a fetch expression
		/// </summary>
		public class FetchResult
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="records">Collection of <see cref="Entity"/> records returned by the execution of a fetch expression</param>
			/// <param name="totalRecordCount">Total number of records</param>
			/// <param name="entityPermissionDenied">Indicates if access to the records was denied or granted.</param>
			public FetchResult(IEnumerable<Entity> records, int totalRecordCount = 0, bool entityPermissionDenied = false)
			{
				Records = records;
				TotalRecordCount = totalRecordCount;
				EntityPermissionDenied = entityPermissionDenied;
			}

			/// <summary>
			/// Collection of <see cref="Entity"/> records
			/// </summary>
			public IEnumerable<Entity> Records { get; private set; }

			/// <summary>
			/// The total number of records
			/// </summary>
			public int TotalRecordCount { get; private set; }

			/// <summary>
			/// Indicates if the user does not have permission to read the entity records.
			/// </summary>
			public bool EntityPermissionDenied { get; private set; }
		}

		[HttpPost]
		[AjaxFormStatusResponse]
		[ValidateAntiForgeryToken]
		public ActionResult AddNote(string regardingEntityLogicalName, string regardingEntityId, string text, bool isPrivate = false, HttpPostedFileBase file = null, string attachmentSettings = null)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(StringHelper.StripHtml(text)))
			{
				return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed, ResourceManager.GetString("Required_Field_Error").FormatWith(ResourceManager.GetString("Note_DefaultText")));
			}

			Guid regardingId;
			Guid.TryParse(regardingEntityId, out regardingId);
			var regarding = new EntityReference(regardingEntityLogicalName, regardingId);
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var user = Request.GetOwinContext().GetUser();

			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var settings = GetAnnotationSettings(serviceContext, attachmentSettings);

			var annotation = new Annotation
			{
				NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, text),
				Subject = AnnotationHelper.BuildNoteSubject(serviceContext, user.ContactId, isPrivate),
				Regarding = regarding
			};
			if (file != null && file.ContentLength > 0)
			{
				annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(file, settings.StorageLocation);
			}

			var result = (AnnotationCreateResult)dataAdapter.CreateAnnotation(annotation, settings);
			
			if (!result.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			if (!result.CanCreate)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, string.Format(ResourceManager.GetString("No_Entity_Permissions"), "create notes"));
			}

			if (!result.CanAppendTo)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, string.Format(ResourceManager.GetString("No_Entity_Permissions"), "append to record"));
			}

			if (!result.CanAppend)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, string.Format(ResourceManager.GetString("No_Entity_Permissions"), "append notes"));
			}

			return new HttpStatusCodeResult(HttpStatusCode.Created);
		}

		[HttpPost]
		[AjaxFormStatusResponse]
		[ValidateAntiForgeryToken]
		public ActionResult UpdateNote(string id, string text, string subject, bool isPrivate = false, HttpPostedFileBase file = null, string attachmentSettings = null)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(StringHelper.StripHtml(text)))
			{
				return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed, ResourceManager.GetString("Required_Field_Error").FormatWith(ResourceManager.GetString("Note_DefaultText")));
			}
			Guid annotationId;
			Guid.TryParse(id, out annotationId);
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var settings = GetAnnotationSettings(dataAdapterDependencies.GetServiceContext(), attachmentSettings);

			var annotation = dataAdapter.GetAnnotation(annotationId);

			annotation.AnnotationId = annotationId;

			annotation.NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, text);

			if (!isPrivate && !string.IsNullOrWhiteSpace(subject) && subject.Contains(AnnotationHelper.PrivateAnnotationPrefix))
			{
				annotation.Subject = subject.Replace(AnnotationHelper.PrivateAnnotationPrefix, string.Empty);
			}

			if (isPrivate && !string.IsNullOrWhiteSpace(subject) && !subject.Contains(AnnotationHelper.PrivateAnnotationPrefix))
			{
				annotation.Subject = subject + AnnotationHelper.PrivateAnnotationPrefix;
			}

			if (file != null && file.ContentLength > 0)
			{
				annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(file, settings.StorageLocation);
			}

			try
			{
				var result = dataAdapter.UpdateAnnotation(annotation, settings);

				if (!result.PermissionsExist)
				{
					return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
						ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
				}

				if (!result.PermissionGranted)
				{
					return new HttpStatusCodeResult(HttpStatusCode.Forbidden, string.Format(ResourceManager.GetString("No_Entity_Permissions"), "update notes"));
				}

				return new HttpStatusCodeResult(HttpStatusCode.OK);
			}
			catch (AnnotationException ex)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ex.Message);
			}
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult DeleteNote(string id)
		{
			Guid annotationId;
			Guid.TryParse(id, out annotationId);
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var annotation = dataAdapter.GetAnnotation(annotationId);

			var result = dataAdapter.DeleteAnnotation(annotation, new AnnotationSettings(dataAdapterDependencies.GetServiceContext(), true));
			
			if (!result.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			if (!result.PermissionGranted)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, string.Format(ResourceManager.GetString("No_Entity_Permissions"), "delete this record"));
			}
			
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		internal static AnnotationSettings GetAnnotationSettings(OrganizationServiceContext serviceContext, string annotationSettings = null)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");

			if (string.IsNullOrEmpty(annotationSettings))
			{
				return new AnnotationSettings(serviceContext, true);
			}

			AnnotationSettings settings;

			try
			{
				var bytes = MachineKey.Unprotect(Convert.FromBase64String(annotationSettings), "Secure Notes Configuration");

				if (bytes == null)
				{
					throw new InvalidOperationException("Failed to decrypt secure annotation settings.");
				}

				var json = Encoding.UTF8.GetString(bytes);

				settings = JsonConvert.DeserializeObject<AnnotationSettings>(json)
					?? new AnnotationSettings(serviceContext, true);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to decrypt secure annotation settings: {0}", e.ToString()));

                throw new SecurityException("Notes configuration is invalid. Permission denied.");
			}

			// Require that permissions always be enabled for this service.
			if (!settings.RespectPermissions)
			{
				throw new InvalidOperationException("Enabled Entity Permissions are required by this service.");
			}

			return settings;
		}
	}
}
