/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cases
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web;
	using Microsoft.Crm.Sdk.Messages;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Core;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;

	/// <summary>
	/// Provides data operations for a single case, as represented by a Case (incident) entity.
	/// </summary>
	public class CaseDataAdapter : ICaseDataAdapter
	{
		public CaseDataAdapter(EntityReference incident, IDataAdapterDependencies dependencies)
		{
			if (incident == null) throw new ArgumentNullException("incident");
			if (incident.LogicalName != "incident") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), incident.LogicalName), "incident");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Incident = incident;
			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected EntityReference Incident { get; set; }

		public virtual void AddNote(string text, string fileName = null, string contentType = null, byte[] fileContent = null, EntityReference ownerId = null)
		{
			try
			{
				var da = new AnnotationDataAdapter(Dependencies);
				var annotation = new Annotation
				{
					Subject = AnnotationHelper.BuildNoteSubject(Dependencies),
					NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, text),
					Regarding = Incident,
					Owner = ownerId
				};
				if (fileContent != null && fileContent.Length > 0 && !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(contentType))
				{
					annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(EnsureValidFileName(fileName), contentType, fileContent);
				}
				da.CreateAnnotation(annotation);
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericErrorException(new Exception("Create annotation error", e));
				throw;
			}
		}

		public virtual void Cancel()
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();

			var incident = serviceContext.GetCase(Incident.Id);

			if (incident == null)
			{
				throw new InvalidOperationException("Unable to retrieve the case with ID {0}.".FormatWith(Incident.Id));
			}

			CancelRelatedActivities(serviceContext, incident);

			serviceContext.SetState((int)IncidentState.Canceled, -1, incident.ToEntityReference());
		}

		public virtual void Reopen()
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();

			serviceContext.SetState((int)IncidentState.Active, -1, Incident);
		}

		public virtual void Resolve(int? customerSatisfactionCode, string resolutionSubject, string resolutionDescription)
		{
			Resolve(customerSatisfactionCode, resolutionSubject, resolutionDescription, null);
		}

		public virtual void Resolve(int? customerSatisfactionCode, string resolutionSubject, string resolutionDescription, int? statuscode)
		{
			if (string.IsNullOrWhiteSpace(resolutionDescription))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "resolutionDescription");
			}

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var incident = serviceContext.GetCase(Incident.Id);

			if (incident == null)
			{
				throw new InvalidOperationException("Unable to retrieve the case with ID {0}.".FormatWith(Incident.Id));
			}

			CancelRelatedActivities(serviceContext, incident);

			// Here, we both create a standard incidentresolution activity, but also set a more permanent
			// resolution text and date on the incident itself. This makes it easier to retieve and reference
			// a canonical resolution for the incident, without having to use a kbarticle, or risk having
			// resolutions be canceled/deleted in the reopening of cases.

			if (customerSatisfactionCode != null)
				incident["customersatisfactioncode"] = new OptionSetValue((int)customerSatisfactionCode);

			incident["adx_resolution"] = resolutionDescription;
			incident["adx_resolutiondate"] = DateTime.UtcNow;

			serviceContext.UpdateObject(incident);
			serviceContext.SaveChanges();

			var resolution = new Entity("incidentresolution");

			resolution["incidentid"] = incident.ToEntityReference();
			resolution["statuscode"] = new OptionSetValue(-1);
			resolution["subject"] = resolutionSubject;
			resolution["description"] = resolutionDescription;

			var status = statuscode ?? -1;

			serviceContext.Execute(new CloseIncidentRequest
			{
				IncidentResolution = resolution,
				Status = new OptionSetValue(status)
			});
		}

		public virtual ICase Select()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var incident = serviceContext.GetCase(Incident.Id);

			if (incident == null)
			{
				return null;
			}

			var responsibleContact = GetResponsibleContact(serviceContext, incident);
			var url = Dependencies.GetUrlProvider().GetUrl(serviceContext, incident);

			return new Case(incident, GetIncidentMetadata(serviceContext), url, responsibleContact);
		}

		public virtual ICaseAccess SelectAccess()
		{
			var @case = Select();

			if (@case == null)
			{
				return CaseAccess.None;
			}

			var @public = @case.PublishToWeb && @case.IsResolved;

			var user = Dependencies.GetPortalUser();

			if (user == null)
			{
				return new CaseAccess(@public: @public);
			}

			var permissionScopes = Dependencies.GetPermissionScopesProviderForPortalUser().SelectPermissionScopes();

			// If the user *is* the customer on the case, case access is based on any Self-scoped permissions
			// they have.
			if (@case.HasCustomer(user))
			{
				return CaseAccess.FromPermissions(permissionScopes.Self, @public);
			}

			// If the customer on the case is a contact, look up the parent customer account for that case, and
			// base case access on the merger of any Account-scoped permissions the user has for that account.
			EntityReference parentcustomerid;

			if (TryGetContactParentCustomer(@case.Customer, out parentcustomerid))
			{
				return CaseAccess.FromPermissions(
					permissionScopes.Accounts.Where(permissions =>
						permissions.Account.Equals(parentcustomerid)), @public);
			}

			// Otherwise, their access is based on the merger of any Account-scoped permissions they
			// have for the case customer account.
			//
			// Permissions are merged by ORing individual rights. For example, if the user is granted the Read
			// right on any of their case access permissions, they get Read access to the case.
			return CaseAccess.FromPermissions(
				permissionScopes.Accounts.Where(permissions =>
					@case.HasCustomer(permissions.Account)), @public);
		}

		public virtual IEnumerable<IAnnotation> SelectNotes()
		{
			var serviceContext = Dependencies.GetServiceContext();
			IAnnotationDataAdapter annotationDataAdapter = new AnnotationDataAdapter(Dependencies);

			return serviceContext.CreateQuery("annotation")
				.Where(e => e.GetAttributeValue<EntityReference>("objectid") == Incident
					&& e.GetAttributeValue<string>("objecttypecode") == Incident.LogicalName
					&& e.GetAttributeValue<string>("notetext").Contains(AnnotationHelper.WebAnnotationPrefix))
				.ToArray()
				.Select(entity => annotationDataAdapter.GetAnnotation(entity))
				.OrderBy(e => e.CreatedOn)
				.ToArray();
		}

		public virtual IEnumerable<ICaseResolution> SelectResolutions()
		{
			var @case = Select();

			if (@case == null || string.IsNullOrEmpty(@case.Resolution))
			{
				return Enumerable.Empty<ICaseResolution>();
			}

			return new[] { new CaseResolution(@case.Resolution, @case.ResolutionDate.GetValueOrDefault()) };
		}

		private bool TryGetContactParentCustomer(EntityReference customer, out EntityReference parentcustomerid)
		{
			parentcustomerid = null;

			if (customer == null || customer.LogicalName != "contact")
			{
				return false;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var contact = serviceContext.CreateQuery("contact")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == customer.Id);

			if (contact == null)
			{
				return false;
			}

			parentcustomerid = contact.GetAttributeValue<EntityReference>("parentcustomerid");

			return parentcustomerid != null;
		}

		internal static EntityMetadata GetIncidentMetadata(OrganizationServiceContext serviceContext)
		{
			var retrieveAttributeRequest = new RetrieveEntityRequest
			{
				LogicalName = "incident", EntityFilters = EntityFilters.Attributes
			};

			var response = (RetrieveEntityResponse)serviceContext.Execute(retrieveAttributeRequest);

			return response.EntityMetadata;
		}

		private enum ActivityPointerState
		{
			Open = 0,
			Completed = 1,
			Canceled = 2,
			Scheduled = 3,
		}

		/// <summary>
		/// Cancel any activity pointers related to the incident, as case resolution will fail if there are any
		/// open activities on the case.
		/// </summary>
		private static void CancelRelatedActivities(OrganizationServiceContext serviceContext, Entity incident)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (incident == null) throw new ArgumentNullException("incident");

			var activityPointers = incident.GetRelatedEntities(serviceContext, new Relationship("Incident_ActivityPointers"));

			foreach (var activityPointer in activityPointers)
			{
				var statecode = activityPointer.GetAttributeValue<OptionSetValue>("statecode");

				if (statecode == null)
				{
					continue;
				}

				if (!(statecode.Value == (int)ActivityPointerState.Open || statecode.Value == (int)ActivityPointerState.Scheduled))
				{
					continue;
				}

				var activityid = activityPointer.GetAttributeValue<Guid>("activityid");
				var activitytypecode = activityPointer.GetAttributeValue<string>("activitytypecode");

				var activityEntity = serviceContext.CreateQuery(activitytypecode).FirstOrDefault(e => e.GetAttributeValue<Guid>("activityid") == activityid);

				if (activityEntity == null)
				{
					continue;
				}

				serviceContext.SetState(activityEntity.ToEntityReference(), new OptionSetValue((int)ActivityPointerState.Canceled), new OptionSetValue(-1));
			}
		}

		private static string EnsureValidFileName(string fileName)
		{
			return fileName.IndexOf("\\", StringComparison.Ordinal) >= 0 ? fileName.Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName;
		}

		private static string GetNoteSubject(OrganizationServiceContext serviceContext, EntityReference user)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");

			var now = DateTime.UtcNow;

			if (user == null || user.LogicalName != "contact")
			{
				return string.Format(ResourceManager.GetString("Note_Created_On_Message"), now);
			}

			var contact = serviceContext.CreateQuery("contact").FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == user.Id);

			if (contact == null)
			{
				return string.Format(ResourceManager.GetString("Note_Created_On_Message"), now);
			}

			// Tack the contact entity reference onto the end of the note subject, so that if we really wanted to, we
			// could parse this subject and find the portal user that submitted the note.
			return string.Format(ResourceManager.GetString("Note_Created_On_DateTime_By_Message"), now, contact.GetAttributeValue<string>("fullname"), contact.LogicalName, contact.Id);
		}

		private static Entity GetResponsibleContact(OrganizationServiceContext serviceContext, Entity incident)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (incident == null) throw new ArgumentNullException("incident");

			var responsibleContact = incident.GetAttributeValue<EntityReference>("responsiblecontactid");

			if (responsibleContact != null)
			{
				return serviceContext.CreateQuery("contact").FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == responsibleContact.Id);
			}

			var customer = incident.GetAttributeValue<EntityReference>("customerid");

			if (customer != null && customer.LogicalName == "contact")
			{
				return serviceContext.CreateQuery("contact").FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == customer.Id);
			}

			return null;
		}
	}
}
