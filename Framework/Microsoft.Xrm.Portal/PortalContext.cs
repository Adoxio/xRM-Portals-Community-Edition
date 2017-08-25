/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Cms.WebsiteSelectors;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal
{
	/// <summary>
	/// Contains the <see cref="Entity"/> instances that are relevant to a single portal page request.
	/// </summary>
	public class PortalContext : IPortalContext, IInitializable, IUserResolutionSettings
	{
		/// <summary>
		/// An accessor to the current portal instance.
		/// </summary>
		public static IPortalContext Current
		{
			get { return PortalCrmConfigurationManager.CreatePortalContext(); }
		}

		private IWebsiteSelector _websiteSelector;

		/// <summary>
		/// The <see cref="OrganizationServiceContext"/> used to obtain the <see cref="Entity"/> instances of the portal context.
		/// </summary>
		public OrganizationServiceContext ServiceContext { get; private set; }

		/// <summary>
		/// The current request.
		/// </summary>
		public RequestContext Request { get; private set; }

		private readonly Lazy<Entity> _website;

		/// <summary>
		/// The configured website.
		/// </summary>
		public virtual Entity Website
		{
			get { return _website.Value; }
		}

		private readonly Lazy<Entity> _user;

		/// <summary>
		/// The current authenticated contact.
		/// </summary>
		public virtual Entity User
		{
			get { return _user.Value; }
		}

		private readonly Lazy<Entity> _entity;

		/// <summary>
		/// The current entity determined by the <see cref="CrmSiteMapProvider"/>.
		/// </summary>
		public virtual Entity Entity
		{
			get { return _entity.Value; }
		}

		private readonly Lazy<CrmSiteMapNode> _node;

		/// <summary>
		/// The current status code determined by the <see cref="CrmSiteMapProvider"/>.
		/// </summary>
		public virtual HttpStatusCode StatusCode
		{
			get { return _node.Value != null ? _node.Value.StatusCode : HttpStatusCode.OK; }
		}

		/// <summary>
		/// The current virtual path determined by the <see cref="CrmSiteMapProvider"/>.
		/// </summary>
		public virtual string Path
		{
			get { return _node.Value != null ? _node.Value.RewriteUrl : null; }
		}

		/// <summary>
		/// The metadata attribute name of the username attribute.
		/// </summary>
		public virtual string AttributeMapUsername { get; private set; }

		/// <summary>
		/// The metadata entity name of the user entity.
		/// </summary>
		public virtual string MemberEntityName { get; private set; }

		public PortalContext(string contextName)
			: this(contextName, null)
		{
		}

		public PortalContext(string contextName, RequestContext request)
			: this(contextName, request, null)
		{
		}

		public PortalContext(string contextName, RequestContext request, IWebsiteSelector websiteSelector)
			: this(CrmConfigurationManager.CreateContext(contextName), request, websiteSelector)
		{
		}

		public PortalContext(OrganizationServiceContext context)
			: this(context, null)
		{
		}

		public PortalContext(OrganizationServiceContext context, RequestContext request)
			: this(context, request, null)
		{
		}

		public PortalContext(OrganizationServiceContext context, RequestContext request, IWebsiteSelector websiteSelector)
		{
			Request = request ?? GetRequestContext();
			ServiceContext = context;
			_websiteSelector = websiteSelector;

			_website = new Lazy<Entity>(() => GetWebsite(ServiceContext, Request, _websiteSelector));
			_user = new Lazy<Entity>(() => GetUser(ServiceContext, Request));
			_node = new Lazy<CrmSiteMapNode>(() => GetNode(Request));
			_entity = new Lazy<Entity>(() => GetEntity(ServiceContext, _node.Value));
		}

		private static RequestContext GetRequestContext()
		{
			var http = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(http) ?? new RouteData();
			var request = new RequestContext(http, routeData);
			return request;
		}

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
			var portalElement = PortalCrmConfigurationManager.GetPortalContextElement(name);
			_websiteSelector = portalElement.WebsiteSelector.CreateWebsiteSelector(portalElement.Name);

			var mergeOptionText = config["mergeOption"];

			if (!string.IsNullOrWhiteSpace(mergeOptionText))
			{
				ServiceContext.MergeOption = mergeOptionText.ToEnum<MergeOption>();
			}

			AttributeMapUsername = config["attributeMapUsername"];
			MemberEntityName = config["memberEntityName"];
		}

		/// <summary>
		/// Returns the timezone for the website.
		/// </summary>
		/// <returns></returns>
		public virtual TimeZoneInfo GetTimeZone()
		{
			var timeZone = ServiceContext.GetTimeZone(Website);
			return timeZone;
		}

		private Entity GetUser(OrganizationServiceContext context, RequestContext request)
		{
			if (request == null || request.HttpContext.User == null || !request.HttpContext.User.Identity.IsAuthenticated) return null;

			var username = request.HttpContext.User.Identity.Name;
			var attributeMapUsername = AttributeMapUsername ?? "adx_username";
			var memberEntityName = MemberEntityName ?? "contact";

			var findContact =
				from c in context.CreateQuery(memberEntityName)
				where c.GetAttributeValue<string>(attributeMapUsername) == username
				select c;

			return findContact.FirstOrDefault();
		}

		private static Entity GetWebsite(OrganizationServiceContext context, RequestContext request, IWebsiteSelector websiteSelector)
		{
			return websiteSelector.GetWebsite(context, request);
		}

		private static CrmSiteMapNode GetNode(RequestContext request)
		{
			if (request == null) return null;

			var path = request.RouteData.Values["path"] as string;

			path = !string.IsNullOrWhiteSpace(path)
				? path.StartsWith("/") ? path : "/" + path
				: "/";

			var node = SiteMap.Provider.FindSiteMapNode(path) as CrmSiteMapNode;
			return node;
		}

		private static Entity GetEntity(OrganizationServiceContext context, CrmSiteMapNode node)
		{
			return context.MergeClone(node.Entity);
		}
	}
}
