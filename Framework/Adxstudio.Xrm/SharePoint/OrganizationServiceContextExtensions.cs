/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.SharePoint
{
	/// <summary>
	/// A set of helpers for managing 'sharepointsite' and 'sharepointdocumentlocation' entities.
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		private const string _sharepointdocumentlocation = "sharepointdocumentlocation";
		private const string _sharepointdocumentlocationid = "sharepointdocumentlocationid";
		private const string _sharepointdoclocationparentrelationship = "sharepointdocumentlocation_parent_sharepointdocumentlocation";
		private const string _sharepointdoclocationsiterelationship = "sharepointdocumentlocation_parent_sharepointsite";
		private const string _sharepointsite = "sharepointsite";

		/// <summary>
		/// Builds a sequence of URLs for all existing document locations related to a given entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns>
		/// If the entity is a 'sharepointsite', returns the 'absoluteurl' of the entity.
		/// If the entity is a 'sharepointdocumentlocation', returns the 'absoluteurl' of the entity or builds the document location path.
		/// Otherwise, builds the paths for all document locations related to the entity.
		/// </returns>
		public static IEnumerable<Uri> GetDocumentLocationUrls(this OrganizationServiceContext context, Entity entity)
		{
			var paths = GetDocumentLocationPaths(context, entity);

			return paths.Select(GetDocumentLocationUrl);
		}

		/// <summary>
		/// Builds the URL related to an existing 'sharepointsite' or 'sharepointdocumentlocation' entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns>
		/// If the entity is a 'sharepointsite', returns the 'absoluteurl' of the entity.
		/// If the entity is a 'sharepointdocumentlocation', returns the 'absoluteurl' of the entity or builds the document location path.
		/// </returns>
		public static Uri GetDocumentLocationUrl(this OrganizationServiceContext context, Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			entity.AssertEntityName(_sharepointsite, _sharepointdocumentlocation);

			// if the entity is a SharePoint site, just return the absolute URL

			if (entity.LogicalName == _sharepointsite)
			{
				var absoluteUrl = entity.GetAttributeValue<string>("absoluteurl");
				return new Uri(absoluteUrl);
			}

			// if the entity is a document location with an absolute URL, just return the absolute URL

			if (entity.LogicalName == _sharepointdocumentlocation)
			{
				var absoluteUrl = entity.GetAttributeValue<string>("absoluteurl");

				if (!string.IsNullOrWhiteSpace(absoluteUrl)) return new Uri(absoluteUrl);
			}

			var path = GetDocumentLocationPath(context, entity);

			return GetDocumentLocationUrl(path);
		}

		/// <summary>
		/// Builds a sequence of a sequence of existing 'sharepointsite' and 'sharepointdocumentlocation' segments representing a set of paths to an entity.
		/// </summary>
		public static IEnumerable<IEnumerable<Entity>> GetDocumentLocationPaths(this OrganizationServiceContext context, Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			if (entity.LogicalName == _sharepointdocumentlocation)
			{
				// return a single path for this document location

				var path = GetDocumentLocationPath(context, entity);

				yield return path;
			}
			else
			{
				// this is an arbitrary entity, find all document locations associated with the entity

				var locations = context.CreateQuery(_sharepointdocumentlocation)
					.Where(sdl => sdl.GetAttributeValue<EntityReference>("regardingobjectid") == entity.ToEntityReference());

				foreach (var location in locations)
				{
					// return a path for each document location

					var path = GetDocumentLocationPath(context, location);

					yield return path;
				}
			}
		}

		/// <summary>
		/// Builds a sequence of existing 'sharepointsite' and 'sharepointdocumentlocation' segments representing a path.
		/// </summary>
		public static IEnumerable<Entity> GetDocumentLocationPath(this OrganizationServiceContext context, Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			entity.AssertEntityName(_sharepointdocumentlocation);

			// if this is an absolute path, just return the lone document location

			var absoluteUrl = entity.GetAttributeValue<string>("absoluteurl");

			if (!string.IsNullOrWhiteSpace(absoluteUrl))
			{
				yield return entity;
				yield break;
			}

			// this is a document location, build the path recursively

			var reference = entity.GetAttributeValue<EntityReference>("parentsiteorlocation");

			if (reference == null)
			{
				// refresh the entity

				entity = context.CreateQuery(_sharepointdocumentlocation).FirstOrDefault(l => l.GetAttributeValue<Guid>(_sharepointdocumentlocationid) == entity.Id);
				reference = entity.GetAttributeValue<EntityReference>("parentsiteorlocation");
			}

			// the parent is either a site or another document location

			var parent = reference.LogicalName == _sharepointdocumentlocation
				? entity.GetRelatedEntity(context, _sharepointdoclocationparentrelationship, EntityRole.Referencing)
				: entity.GetRelatedEntity(context, _sharepointdoclocationsiterelationship);
			
			if (parent != null)
			{
				if (parent.LogicalName == _sharepointdocumentlocation)
				{
					// continue traversing up the path

					var path = GetDocumentLocationPath(context, parent);

					foreach (var next in path)
					{
						yield return next;
					}
				}
				else
				{
					// this is a site

					yield return parent;
				}
			}

			yield return entity;
		}

		private static Uri GetDocumentLocationUrl(IEnumerable<Entity> path)
		{
			var head = path.First();
			var tail = path.Skip(1).Select(segment => segment.GetAttributeValue<string>("relativeurl"));

			// the head should be a SharePoint site entity or an absolute document location

			var baseUrl = head.GetAttributeValue<string>("absoluteurl");

			if (!tail.Any()) return new Uri(baseUrl);

			// the tail should be a sequence of document location entities

			var relativeUrl = string.Join("/", tail.ToArray());
			var url = Combine(baseUrl, relativeUrl);

			return new Uri(url);
		}

		private static string Combine(string baseUrl, string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath)) return baseUrl;

			var url = "{0}/{1}".FormatWith(baseUrl.TrimEnd('/'), relativePath.TrimStart('/'));
			return url;
		}

		/// <summary>
		/// Retrieves the SharePoint list name and folder name for the document location.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="listUrl"></param>
		/// <param name="folderUrl"></param>
		public static void GetDocumentLocationListAndFolder(this OrganizationServiceContext context, Entity entity, out string listUrl, out string folderUrl)
		{
			var absoluteUrl = entity.GetAttributeValue<string>("absoluteurl");

			if (!string.IsNullOrWhiteSpace(absoluteUrl))
			{
				throw new NotImplementedException("Add support for 'absoluteurl' based document locations.");
			}

			var path = context.GetDocumentLocationPath(entity);

			var tail = path.Skip(1).ToList(); // path.First() is SP site.

			var listEntity = tail.First();
			var folderEntities = tail.Skip(1);

			listUrl = listEntity.GetAttributeValue<string>("relativeurl");

			var segments = folderEntities.Select(e => e.GetAttributeValue<string>("relativeurl"));

			folderUrl = string.Join("/", segments);
		}

		public static Entity GetSharePointSiteFromUrl(this OrganizationServiceContext context, Uri absoluteUrl)
		{
			var sharePointSites = context.CreateQuery("sharepointsite").Where(site => site.GetAttributeValue<int?>("statecode") == 0).ToArray(); // all active sites

			var siteUrls = new Dictionary<Guid, string>();

			foreach (var sharePointSite in sharePointSites)
			{
				var siteUrl = sharePointSite.GetAttributeValue<string>("absoluteurl") ?? string.Empty;

				var parentSiteReference = sharePointSite.GetAttributeValue<EntityReference>("parentsite");

				if (parentSiteReference != null)
				{
					var parentSite = sharePointSites.FirstOrDefault(site => site.Id == parentSiteReference.Id);

					if (parentSite != null)
					{
						siteUrl = "{0}/{1}".FormatWith(parentSite.GetAttributeValue<string>("absoluteurl").TrimEnd('/'), sharePointSite.GetAttributeValue<string>("relativeurl"));
					}
				}

				siteUrls.Add(sharePointSite.Id, siteUrl);
			}

			var siteKeyAndUrl = siteUrls.Select(pair => pair as KeyValuePair<Guid, string>?)
				.FirstOrDefault(pair => string.Equals(pair.Value.Value.TrimEnd('/'), absoluteUrl.ToString().TrimEnd('/'), StringComparison.InvariantCultureIgnoreCase));

			if (siteKeyAndUrl == null)
			{
				throw new ApplicationException("Couldn't find an active SharePoint site with the URL {0}.".FormatWith(absoluteUrl));
			}

			return sharePointSites.First(site => site.Id == siteKeyAndUrl.Value.Key);
		}

		/// <summary>
		/// Returns a 'sharepointdocumentlocation' that represents the path from an existing 'sharepointsite' to an existing entity.
		/// If the document location path does not exist, a relative path of locations is created between the two entities.
		/// The folder path structure defined by the site is respected when retrieving or creating the document location path.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="sharePointSite"></param>
		/// <param name="entity"></param>
		/// <param name="relativeUrl"></param>
		/// <returns>
		/// The 'sharepointdocumentlocation' referencing the folder of the entity.
		/// </returns>
		public static T AddOrGetExistingDocumentLocation<T>(this OrganizationServiceContext context, Entity sharePointSite, Entity entity, string relativeUrl)
			where T : Entity, new()
		{
			if (sharePointSite == null) throw new ArgumentNullException("sharePointSite");
			if (entity == null) throw new ArgumentNullException("entity");
			sharePointSite.AssertEntityName(_sharepointsite);

			// Replace the following invalid characters ~ " # % & * : < > ? / \ { | } . with hyphens (Same as what CRM does).
			var spSafeRelativeUrl = Regex.Replace(relativeUrl, @"[\~\""\#\%\&\*\:\<\>\?\/\\\{\|\}\.]", "-");

			var siteName = sharePointSite.GetAttributeValue<string>("name");
			var locationName = "Documents on {0}".FormatWith(siteName);

			var folderStructureEntity = sharePointSite.GetAttributeValue<string>("folderstructureentity");

			if (folderStructureEntity == "contact" && entity.LogicalName != "contact")
			{
				// <site>/contact/<contact name>/<entity name>/<record name>

				var related = context.GetRelatedForEntityCentricFolderStructure(entity, folderStructureEntity);

				if (related != null)
				{
					return context.AddOrGetEntityCentricDocumentLocation<T>(locationName, spSafeRelativeUrl, entity, related, sharePointSite);
				}
			}
			
			if (folderStructureEntity == "account" && entity.LogicalName != "account")
			{
				// <site>/account/<account name>/<entity name>/<record name>

				var related = context.GetRelatedForEntityCentricFolderStructure(entity, folderStructureEntity);

				if (related != null)
				{
					return context.AddOrGetEntityCentricDocumentLocation<T>(locationName, spSafeRelativeUrl, entity, related, sharePointSite);
				}
			}

			// <site>/<entity name>/<record name>

			var entitySetLocation = sharePointSite
				.GetRelatedEntities(context, _sharepointdoclocationsiterelationship)
				.FirstOrDefault(loc => loc.GetAttributeValue<string>("relativeurl") == entity.LogicalName);

			if (entitySetLocation == null)
			{
				entitySetLocation = context.CreateDocumentLocation<T>(locationName, entity.LogicalName, sharePointSite, _sharepointdoclocationsiterelationship);
				return context.CreateDocumentLocation<T>(locationName, spSafeRelativeUrl, entitySetLocation, _sharepointdoclocationparentrelationship, entity.ToEntityReference());
			}

			return context.CreateOrUpdateRecordDocumentLocation<T>(locationName, spSafeRelativeUrl, entitySetLocation, entity);
		}

		public static T AddOrGetExistingDocumentLocationAndSave<T>(this OrganizationServiceContext context, Entity sharePointSite, Entity entity, string relativeUrl)
			where T : Entity, new()
		{
			var location = context.AddOrGetExistingDocumentLocation<T>(sharePointSite, entity, relativeUrl);

			if (location.EntityState == EntityState.Created || location.EntityState == EntityState.Changed)
			{
				context.SaveChanges();

				// refresh the context and result entity

				context.ClearChanges();

				location = context.CreateQuery(_sharepointdocumentlocation).FirstOrDefault(loc => loc.GetAttributeValue<Guid>(_sharepointdocumentlocationid) == location.Id) as T;
			}

			return location;
		}

		private static T AddOrGetEntityCentricDocumentLocation<T>(this OrganizationServiceContext context, string locationName, string relativeUrl, Entity record, EntityReference related, Entity sharePointSite) where T : Entity, new()
		{
			var entityCentricSetLocation = sharePointSite
				.GetRelatedEntities(context, _sharepointdoclocationsiterelationship)
				.FirstOrDefault(loc => loc.GetAttributeValue<string>("relativeurl") == related.LogicalName);

			T entityCentricNameLocation;
			T entityNameLocation;

			if (entityCentricSetLocation == null)
			{
				entityCentricSetLocation = context.CreateDocumentLocation<T>(locationName, related.LogicalName, sharePointSite, _sharepointdoclocationsiterelationship);
				entityCentricNameLocation = context.CreateDocumentLocation<T>(locationName, related.Name, entityCentricSetLocation, _sharepointdoclocationparentrelationship, related);
				entityNameLocation = context.CreateDocumentLocation<T>(locationName, record.LogicalName, entityCentricNameLocation, _sharepointdoclocationparentrelationship);
				return context.CreateDocumentLocation<T>(locationName, relativeUrl, entityNameLocation, _sharepointdoclocationparentrelationship, record.ToEntityReference());
			}

			entityCentricNameLocation = entityCentricSetLocation
				.GetRelatedEntities(context, _sharepointdoclocationparentrelationship, EntityRole.Referenced)
				.FirstOrDefault(loc => loc.GetAttributeValue<string>("relativeurl") == related.Name) as T;

			if (entityCentricNameLocation == null)
			{
				entityCentricNameLocation = context.CreateDocumentLocation<T>(locationName, related.Name, entityCentricSetLocation, _sharepointdoclocationparentrelationship, related);
				entityNameLocation = context.CreateDocumentLocation<T>(locationName, record.LogicalName, entityCentricNameLocation, _sharepointdoclocationparentrelationship);
				return context.CreateDocumentLocation<T>(locationName, relativeUrl, entityNameLocation, _sharepointdoclocationparentrelationship, record.ToEntityReference());
			}

			entityNameLocation = entityCentricNameLocation
				.GetRelatedEntities(context, _sharepointdoclocationparentrelationship, EntityRole.Referenced)
				.FirstOrDefault(loc => loc.GetAttributeValue<string>("relativeurl") == record.LogicalName) as T;

			if (entityNameLocation == null)
			{
				entityNameLocation = context.CreateDocumentLocation<T>(locationName, record.LogicalName, entityCentricNameLocation, _sharepointdoclocationparentrelationship);
				return context.CreateDocumentLocation<T>(locationName, relativeUrl, entityNameLocation, _sharepointdoclocationparentrelationship, record.ToEntityReference());
			}

			return context.CreateOrUpdateRecordDocumentLocation<T>(locationName, relativeUrl, entityNameLocation, record);
		}

		private static T CreateDocumentLocation<T>(this OrganizationServiceContext context, string name, string relativeUrl, Entity parentLocation, string parentRelationship, EntityReference regarding = null) where T : Entity, new()
		{
			var location = new T { LogicalName = _sharepointdocumentlocation };
			location.SetAttributeValue("name", name);
			location.SetAttributeValue("relativeurl", relativeUrl);
			
			if (regarding != null)
			{
				location.SetAttributeValue("regardingobjectid", regarding);
			}

			if (!context.IsAttached(parentLocation))
			{
				context.Attach(parentLocation);
			}

			context.AddRelatedObject(parentLocation, parentRelationship, location, EntityRole.Referenced);

			return location;
		}

		private static T CreateOrUpdateRecordDocumentLocation<T>(this OrganizationServiceContext context, string name, string relativeUrl, Entity parentLocation, Entity record) where T : Entity, new()
		{
			var recordLocation = parentLocation
				.GetRelatedEntities(context, _sharepointdoclocationparentrelationship, EntityRole.Referenced)
				.FirstOrDefault(loc => loc.GetAttributeValue<string>("relativeurl") == relativeUrl) as T;

			if (recordLocation == null)
			{
				return context.CreateDocumentLocation<T>(name, relativeUrl, parentLocation, _sharepointdoclocationparentrelationship, record.ToEntityReference());
			}

			if (recordLocation.GetAttributeValue<EntityReference>("regardingobjectid") == null)
			{
				recordLocation.SetAttributeValue("regardingobjectid", record.ToEntityReference());

				context.UpdateObject(recordLocation);
			}

			return recordLocation;
		}

		private static EntityReference GetRelatedForEntityCentricFolderStructure(this OrganizationServiceContext context, Entity entity, string folderStructureEntity)
		{
			var entityMetadata = context.GetEntityMetadata(entity.LogicalName, EntityFilters.Relationships);

			var systemRelationships = new List<OneToManyRelationshipMetadata>();
			var customRelationships = new List<OneToManyRelationshipMetadata>();

			foreach (var relationship in entityMetadata.ManyToOneRelationships.Where(relationship => relationship.ReferencedEntity == folderStructureEntity))
			{
				if (relationship.IsCustomRelationship.GetValueOrDefault())
				{
					customRelationships.Add(relationship);
				}
				else
				{
					systemRelationships.Add(relationship);
				}
			}

			if (systemRelationships.Count() > 1)
			{
				foreach (var systemRelationship in systemRelationships.Where(systemRelationship =>
					systemRelationship.ReferencedEntity == "account" && (systemRelationship.SchemaName == "opportunity_customer_accounts" || systemRelationship.SchemaName == "contract_customer_accounts")
					|| systemRelationship.ReferencedEntity == "contact" && (systemRelationship.SchemaName == "opportunity_customer_contacts" || systemRelationship.SchemaName == "contract_customer_contacts")))
				{
					systemRelationships.Clear();
					systemRelationships.Add(systemRelationship);
					break;
				}
			}

			var lookupAttribute = systemRelationships.Count() == 1 ? systemRelationships.First().ReferencingAttribute
				: customRelationships.Count() == 1 ? customRelationships.First().ReferencingAttribute
				: string.Empty;

			return !string.IsNullOrEmpty(lookupAttribute)
				? entity.GetAttributeValue<EntityReference>(lookupAttribute)
				: null;
		}
	}
}
