/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Identity;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Routing;

namespace Adxstudio.Xrm
{
	/// <summary>
	/// Contains the <see cref="Entity"/> instances that are relevant to a single portal page request.
	/// </summary>
	public class PortalContext : Microsoft.Xrm.Portal.PortalContext
	{
		private readonly Lazy<Entity> _user;

		public override Entity User
		{
			get { return _user.Value; }
		}

		private readonly Lazy<Entity> _entity;

		public override Entity Entity
		{
			get { return _entity.Value; }
		}

		private readonly Lazy<CrmSiteMapNode> _node;

		public override HttpStatusCode StatusCode
		{
			get { return _node.Value != null ? _node.Value.StatusCode : HttpStatusCode.OK; }
		}

		public override string Path
		{
			get { return _node.Value != null ? _node.Value.RewriteUrl : null; }
		}

		private readonly Lazy<Entity> _website;

		public override Entity Website
		{
			get { return _website.Value; }
		}

		public PortalContext(string contextName, RequestContext request)
			: base(contextName, request)
		{
			// Lazy-load context information.
			_user = new Lazy<Entity>(GetUser);
			_node = new Lazy<CrmSiteMapNode>(() => GetNode(Request));
			_entity = new Lazy<Entity>(() => GetEntity(ServiceContext, _node.Value));
			_website = new Lazy<Entity>(GetWebsite);
		}

		private Entity GetWebsite()
		{
			var website = Request.HttpContext.GetWebsite();

			var entity = website.Entity.Clone();
			ServiceContext.ReAttach(entity);
			return entity;
		}

		private Entity GetUser()
		{
			var identity = Request.HttpContext.User.Identity;

			if (!identity.IsAuthenticated) return null;

			var user = Request.HttpContext.GetUser();

			if (user == null || user == CrmUser.Anonymous) return null;

			var contact = ServiceContext.RetrieveSingle(user.ContactId, new ColumnSet(true));

			return contact;
		}

		private static CrmSiteMapNode GetNode(RequestContext request)
		{
			if (request == null) return null;
			
			var node = request.HttpContext.GetNode();

			return node ?? PortalRouteHandler.GetNode(request);
		}

		private static Entity GetEntity(OrganizationServiceContext context, CrmSiteMapNode node)
		{
			return node != null ? context.MergeClone(node.Entity) : null;
		}
	}
}
