/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	internal abstract class DirectoryType
	{
		public abstract bool SupportsUpload { get; }

		public abstract string WebFileForeignKeyAttribute { get; }

		public virtual string GetDirectoryName(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			return entity.GetAttributeValue<string>("adx_name");
		}

		public abstract Entity GetEntity(OrganizationServiceContext serviceContext, Guid id, EntityReference website);

		public abstract IEnumerable<Entity> GetEntityChildren(OrganizationServiceContext serviceContext, Entity entity, EntityReference website);

		public abstract IEnumerable<Tuple<Entity, EntityReference>> GetTreeParents(OrganizationServiceContext serviceContext, EntityReference website);
	}

	internal class WebPageDirectoryType : DirectoryType
	{
		public override bool SupportsUpload
		{
			get { return true; }
		}

		public override string WebFileForeignKeyAttribute
		{
			get { return "adx_parentpageid"; }
		}

		public override string GetDirectoryName(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			var title = entity.GetAttributeValue<string>("adx_title");

			return string.IsNullOrWhiteSpace(title) ? base.GetDirectoryName(entity) : title;
		}

		public override Entity GetEntity(OrganizationServiceContext serviceContext, Guid id, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			return serviceContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == id
					&& e.GetAttributeValue<EntityReference>("adx_websiteid") == website);
		}

		public override IEnumerable<Entity> GetEntityChildren(OrganizationServiceContext serviceContext, Entity entity, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (entity == null) throw new ArgumentNullException("entity");
			if (website == null) throw new ArgumentNullException("website");

			return serviceContext.GetChildPages(entity)
				.Union(serviceContext.GetChildFiles(entity))
				.Union(GetRelatedEntities(serviceContext, entity, new Relationship("adx_webpage_blog")))
				.Union(GetRelatedEntities(serviceContext, entity, new Relationship("adx_webpage_event")))
				.Union(GetRelatedEntities(serviceContext, entity, new Relationship("adx_webpage_communityforum")));
		}

		public override IEnumerable<Tuple<Entity, EntityReference>> GetTreeParents(OrganizationServiceContext serviceContext, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			return serviceContext.CreateQuery("adx_webpage")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.ToArray()
				.Select(e => new Tuple<Entity, EntityReference>(e, e.GetAttributeValue<EntityReference>("adx_parentpageid")));
		}

		private static IEnumerable<Entity> GetRelatedEntities(OrganizationServiceContext serviceContext, Entity entity, Relationship relationship)
		{
			try
			{
				return entity.GetRelatedEntities(serviceContext, relationship);
			}
			catch
			{
				return new Entity[] { };
			}
		}
	}

	internal class BlogDirectoryType : DirectoryType
	{
		public override bool SupportsUpload
		{
			get { return false; }
		}

		public override string WebFileForeignKeyAttribute
		{
			get { return null; }
		}

		public override Entity GetEntity(OrganizationServiceContext serviceContext, Guid id, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			return serviceContext.CreateQuery("adx_blog")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogid") == id
					&& e.GetAttributeValue<EntityReference>("adx_websiteid") == website);
		}

		public override IEnumerable<Entity> GetEntityChildren(OrganizationServiceContext serviceContext, Entity entity, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (entity == null) throw new ArgumentNullException("entity");
			if (website == null) throw new ArgumentNullException("website");

			return entity.GetRelatedEntities(serviceContext, new Relationship("adx_blog_blogpost"));
		}

		public override IEnumerable<Tuple<Entity, EntityReference>> GetTreeParents(OrganizationServiceContext serviceContext, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			try
			{
				return serviceContext.CreateQuery("adx_blog")
					.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
					.ToArray()
					.Select(e => new Tuple<Entity, EntityReference>(e, e.GetAttributeValue<EntityReference>("adx_parentpageid")));
			}
			catch (FaultException<OrganizationServiceFault>)
			{
				return new Tuple<Entity, EntityReference>[] { };
			}
		}
	}

	internal class BlogPostDirectoryType : DirectoryType
	{
		public override bool SupportsUpload
		{
			get { return true; }
		}

		public override string WebFileForeignKeyAttribute
		{
			get { return "adx_blogpostid"; }
		}

		public override Entity GetEntity(OrganizationServiceContext serviceContext, Guid id, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			var query = from post in serviceContext.CreateQuery("adx_blogpost")
				join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid") equals new EntityReference("adx_blog", blog.GetAttributeValue<Guid>("adx_blogid"))
				where blog.GetAttributeValue<EntityReference>("adx_websiteid") == website
				where post.GetAttributeValue<Guid>("adx_blogpostid") == id
				select post;

			return query.FirstOrDefault();
		}

		public override IEnumerable<Entity> GetEntityChildren(OrganizationServiceContext serviceContext, Entity entity, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (entity == null) throw new ArgumentNullException("entity");
			if (website == null) throw new ArgumentNullException("website");

			return entity.GetRelatedEntities(serviceContext, new Relationship("adx_blogpost_webfile"));
		}

		public override IEnumerable<Tuple<Entity, EntityReference>> GetTreeParents(OrganizationServiceContext serviceContext, EntityReference website)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (website == null) throw new ArgumentNullException("website");

			try
			{
				return (from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid") equals new EntityReference("adx_blog", blog.GetAttributeValue<Guid>("adx_blogid"))
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") == website
					select post)
					.ToArray()
					.Select(e => new Tuple<Entity, EntityReference>(e, e.GetAttributeValue<EntityReference>("adx_blogid")));
			}
			catch (FaultException<OrganizationServiceFault>)
			{
				return new Tuple<Entity, EntityReference>[] { };
			}
		}
	}
}
