/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Portal.Access;

namespace Microsoft.Xrm.Portal.Core
{
	public static class OrganizationServiceContextExtensions
	{
		public static IEnumerable<Entity> GetSubjects(this OrganizationServiceContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			return context.CreateQuery("subject").ToList();
		}

		public static Entity GetArticle(this OrganizationServiceContext context, Guid articleId)
		{
			if (context == null) throw new ArgumentNullException("context");

			var findArticle =
				from c in context.CreateQuery("kbarticle")
				where c.GetAttributeValue<Guid>("kbarticleid") == articleId
				select c;

			return findArticle.FirstOrDefault();
		}

		public static Entity GetCase(this OrganizationServiceContext context, Guid caseId)
		{
			if (context == null) throw new ArgumentNullException("context");

			var findCase =
				from c in context.CreateQuery("incident")
				where c.GetAttributeValue<Guid>("incidentid") == caseId
				select c;

			return findCase.FirstOrDefault();
		}

		public static IEnumerable<Entity> GetCasesByCustomer(this OrganizationServiceContext context, Guid? customerId)
		{
			if (context == null) throw new ArgumentNullException("context");

			var result = new List<Entity>();

			var findCases =
				from c in context.CreateQuery("incident")
				where c.GetAttributeValue<Guid?>("customerid") == customerId
				select c;

			result.AddRange(findCases);

			var findAccounts =
				from a in context.CreateQuery("account")
				where a.GetAttributeValue<Guid?>("accountid") == customerId
				select a;

			var account = findAccounts.FirstOrDefault();

			if (account == null) return result;

			var contacts = account.GetRelatedEntities(context, "contact_customer_accounts");

			foreach (var contact in contacts)
			{
				result.AddRange(context.GetCasesByCustomer(contact.GetAttributeValue<Guid?>("contactid")));
			}

			return result;
		}

		public static IEnumerable<Entity> GetCasesForCurrentUser(this OrganizationServiceContext context, Entity contact, int scope)
		{
			if (context == null) throw new ArgumentNullException("context");

			var emptyResult = new List<Entity>();

			if (contact == null) return emptyResult;

			var access = context.GetCaseAccessByContact(contact);

			if (access == null || !access.GetAttributeValue<bool?>("adx_read").GetValueOrDefault()) return emptyResult;

			var parentCustomer = contact.GetAttributeValue<Guid?>("parentcustomerid");

			return ((access.GetAttributeValue<int?>("adx_scope") == scope) && (parentCustomer != null))
				? GetCasesByCustomer(context, parentCustomer)
				: GetCasesByCustomer(context, contact.GetAttributeValue<Guid?>("contactid"));
		}

		/// <summary>
		/// Add a Note to an entity.
		/// </summary>
		/// <param name="context">The service context</param>
		/// <param name="entity">The entity to which a note will be attached.</param>
		/// <param name="noteTitle"></param>
		/// <param name="noteText">The text of the note.</param>
		/// <returns>True if successful; otherwise, false.</returns>
		/// <remarks>
		/// <para>The provided <paramref name="entity"/> must already be persisted to the CRM for this operation to succeed.</para>
		/// <para>It it not necessary to SaveChanges after this operation--this operation fully persists this note to CRM.</para>
		/// </remarks>
		public static bool AddNoteAndSave(this OrganizationServiceContext context, Entity entity, string noteTitle, string noteText)
		{
			if (context == null) throw new ArgumentNullException("context");

			return context.AddNoteAndSave(entity, noteTitle, noteText, null, null, null);
		}

		/// <summary>
		/// Add a note to an entity.
		/// </summary>
		/// <param name="context">The service context</param>
		/// <param name="entity">The entity to which a note will be attached.</param>
		/// <param name="noteTitle"></param>
		/// <param name="noteText">The text of the note.</param>
		/// <param name="fileName">The name of the file to attach to this note.</param>
		/// <param name="contentType">The MIME type of the file to attach to this note.</param>
		/// <param name="fileContent">The raw byte data of the file to attach to this note.</param>
		/// <returns>True if successful; otherwise, false.</returns>
		/// <remarks>
		/// <para>The provided <paramref name="entity"/> must already be persisted to the CRM for this operation to succeed.</para>
		/// <para>It it not necessary to SaveChanges after this operation--this operation fully persists this note to CRM.</para>
		/// </remarks>
		public static bool AddNoteAndSave(this OrganizationServiceContext context, Entity entity, string noteTitle, string noteText, string fileName, string contentType, byte[] fileContent)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (entity == null) throw new ArgumentNullException("entity");

			var entityName = entity.LogicalName;

			var note = new Entity("annotation");

			note.SetAttributeValue("subject", noteTitle);
			note.SetAttributeValue("notetext", noteText);
			note.SetAttributeValue("isdocument", false);
			note.SetAttributeValue("objectid", entity.ToEntityReference());
			note.SetAttributeValue("objecttypecode", entityName);

			if (fileContent != null && fileContent.Length > 0 && !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(contentType))
			{
				note.SetAttributeValue("documentbody", Convert.ToBase64String(fileContent));
				note.SetAttributeValue("filename", EnsureValidFileName(fileName));
				note.SetAttributeValue("mimetype", contentType);
			}

			context.AddObject(note);
			context.SaveChanges();

			return true;
		}

		/// <summary>
		/// Add a note to an entity.
		/// </summary>
		/// <param name="context">The service context</param>
		/// <param name="entity">The entity to which a note will be attached.</param>
		/// <param name="noteTitle"></param>
		/// <param name="noteText">The text of the note.</param>
		/// <param name="file">The file to attach with this note.</param>
		/// <returns>True if successful; otherwise, false.</returns>
		/// <remarks>
		/// <para>The provided <paramref name="entity"/> must already be persisted to the CRM for this operation to succeed.</para>
		/// <para>It it not necessary to SaveChanges after this operation--this operation fully persists this note to CRM.</para>
		/// </remarks>
		public static bool AddNoteAndSave(this OrganizationServiceContext context, Entity entity, string noteTitle, string noteText, HttpPostedFile file)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (file == null || file.ContentLength <= 0)
			{
				return context.AddNoteAndSave(entity, noteTitle, noteText, null, null, null);
			}

			var fileContent = new byte[file.ContentLength];

			file.InputStream.Read(fileContent, 0, fileContent.Length);

			return context.AddNoteAndSave(entity, noteTitle, noteText, file.FileName, file.ContentType, fileContent);
		}

		private static string EnsureValidFileName(string fileName)
		{
			return fileName.IndexOf("\\") >= 0 ? fileName.Substring(fileName.LastIndexOf("\\") + 1) : fileName;
		}

		public static Entity GetContactByFullNameAndEmailAddress(this OrganizationServiceContext context, string firstName, string lastName, string emailAddress)
		{
			var findContact =
				from c in context.CreateQuery("contact")
				where c.GetAttributeValue<string>("firstname") == firstName
					&& c.GetAttributeValue<string>("lastname") == lastName
						&& c.GetAttributeValue<string>("emailaddress1") == emailAddress
				select c as Entity;

			return findContact.FirstOrDefault();
		}

		public static Entity GetContactByUsername(this OrganizationServiceContext context, string username)
		{
			var findContact =
				from c in context.CreateQuery("contact")
				where c.GetAttributeValue<string>("adx_username") == username
				select c;

			return findContact.FirstOrDefault();
		}

		public static Entity GetContactByInvitationCode(this OrganizationServiceContext context, string invitationCode)
		{
			var now = DateTime.Now.Floor(RoundTo.Minute);

			var findContact =
				from c in context.CreateQuery("contact")
				where c.GetAttributeValue<string>("adx_invitationcode") == invitationCode // MSBug #119992: No need for better string compare, since query provider won't interpret it anyway.
					&& (c.GetAttributeValue<DateTime?>("adx_invitationcodeexpirydate") == null || c.GetAttributeValue<DateTime?>("adx_invitationcodeexpirydate") < now)
				select c;

			return findContact.FirstOrDefault();
		}

		public static IEnumerable<Entity> GetProductsBySubject(this OrganizationServiceContext context, Entity subject)
		{
			subject.AssertEntityName("subject");

			var subjectID = subject.GetAttributeValue<Guid>("subjectid");

			return context.GetProductsBySubject(subjectID);
		}

		public static IEnumerable<Entity> GetProductsBySubject(this OrganizationServiceContext context, Guid subjectID)
		{
			var findProducts =
				from p in context.CreateQuery("product")
				where p.GetAttributeValue<Guid?>("subjectid") == subjectID
				select p;

			return findProducts.ToList();
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Entity product, string priceListName)
		{
			product.AssertEntityName("product");

			return context.GetProductPriceByPriceListName(product.GetAttributeValue<Guid>("productid"), priceListName);
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Guid productID, string priceListName)
		{
			var priceListItem = context.GetPriceListItemByPriceListName(productID, priceListName);

			return priceListItem != null ? priceListItem.GetAttributeValue<Money>("amount") : null;
		}

		public static Entity GetPriceListItemByPriceListName(this OrganizationServiceContext context, Entity product, string priceListName)
		{
			product.AssertEntityName("product");

			return context.GetPriceListItemByPriceListName(product.GetAttributeValue<Guid>("productid"), priceListName);
		}

		public static Entity GetPriceListItemByPriceListName(this OrganizationServiceContext context, Guid productID, string priceListName)
		{
			// Get price lists of the given name that are applicable to the current date/time
			// MSBug #119992: No need for better string compare on price level name, since query provider won't interpret it anyway.
			var webPriceLists =
				from pl in context.CreateQuery("pricelevel").ToList()
				where pl.GetAttributeValue<string>("name") == priceListName && pl.IsApplicableTo(DateTime.UtcNow)
				select pl;

			// Get the product price levels in our applicable price lists for our desired product
			var priceListItems =
				from ppl in context.CreateQuery("productpricelevel").ToList()
				join pl in webPriceLists on ppl.GetAttributeValue<Guid?>("pricelevelid") equals pl.GetAttributeValue<Guid>("pricelevelid")
				where ppl.GetAttributeValue<Guid?>("productid") == productID
				select ppl;

			return priceListItems.FirstOrDefault();
		}

		/// <summary>
		/// Change the state of a case.
		/// </summary>
		/// <param name="context">The service context</param>
		/// <param name="incident">The Case to change.</param>
		/// <param name="state">The state to change to.</param>
		/// <param name="resolutionSubject"></param>
		/// <returns>The updated Case.</returns>
		/// <remarks>
		/// <para>The provided <paramref name="incident"/> must already be persisted to the CRM for this operation to succeed.</para>
		/// <para>It it not necessary to SaveChanges after this operation--this operation fully persists the state change to CRM.</para>
		/// </remarks>
		public static Entity SetCaseStatusAndSave(this OrganizationServiceContext context, Entity incident, string state, string resolutionSubject)
		{
			incident.AssertEntityName("incident");

			return SetCaseStatusAndSave(incident, state, resolutionSubject, context);
		}

		private static Entity SetCaseStatusAndSave(Entity incident, string state, string resolutionSubject, OrganizationServiceContext service)
		{
			var id = incident.Id;

			if (string.Compare(state, "Active", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				service.SetState(0, -1, incident);
			}
			else if (string.Compare(state, "Resolved", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				var resolution = new Entity("incidentresolution");
				resolution.SetAttributeValue("incidentid", incident.ToEntityReference());
				resolution.SetAttributeValue("statuscode", new OptionSetValue(-1));
				resolution.SetAttributeValue("subject", resolutionSubject);

				service.CloseIncident(resolution, -1);
			}
			else // Canceled
			{
				service.SetState(2, -1, incident);
			}

			return service.CreateQuery("incident").First(i => i.GetAttributeValue<Guid?>("incidentid") == id);
		}

		/// <summary>
		/// Change the state of an opportunity.
		/// </summary>
		/// <param name="context">The service context</param>
		/// <param name="opportunity">The Opportunity to change.</param>
		/// <param name="state">The state to change to.</param>
		/// <param name="actualRevenue">The actual revenue of the opportunity.</param>
		/// <returns>The updated Opportunity.</returns>
		/// <remarks>
		/// <para>The provided <paramref name="opportunity"/> must already be persisted to the CRM for this operation to succeed.</para>
		/// <para>It it not necessary to SaveChanges after this operation--this operation fully persists the state change to CRM.</para>
		/// </remarks>
		public static void SetOpportunityStatusAndSave(this OrganizationServiceContext context, Entity opportunity, string state, decimal actualRevenue)
		{
			opportunity.AssertEntityName("opportunity");

			if (string.Compare(state, "Open", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				context.SetState(0, -1, opportunity);
			}
			else if (string.Compare(state, "Won", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				var opportunityCloseId = Guid.NewGuid();
				var closeOpportunity = new Entity("opportunityclose");
				closeOpportunity.SetAttributeValue("opportunityid", opportunity.ToEntityReference());
				closeOpportunity.SetAttributeValue("statuscode", new OptionSetValue(-1));
				closeOpportunity.SetAttributeValue("actualrevenue", actualRevenue);
				closeOpportunity.SetAttributeValue("subject", opportunity.GetAttributeValue("name"));
				closeOpportunity.SetAttributeValue("actualend", DateTime.UtcNow.Floor(RoundTo.Day));
				closeOpportunity.SetAttributeValue("activityid", opportunityCloseId);
				context.WinOpportunity(closeOpportunity, -1);
			}
			else // Lost
			{
				var opportunityCloseId = Guid.NewGuid();
				var closeOpportunity = new Entity("opportunityclose");
				closeOpportunity.SetAttributeValue("opportunityid", opportunity.ToEntityReference());
				closeOpportunity.SetAttributeValue("statuscode", new OptionSetValue(-1));
				closeOpportunity.SetAttributeValue("actualrevenue", actualRevenue);
				closeOpportunity.SetAttributeValue("subject", opportunity.GetAttributeValue("name"));
				closeOpportunity.SetAttributeValue("actualend", DateTime.UtcNow.Floor(RoundTo.Day));
				closeOpportunity.SetAttributeValue("activityid", opportunityCloseId);
				context.LoseOpportunity(closeOpportunity, -1);
			}
		}
	}
}
