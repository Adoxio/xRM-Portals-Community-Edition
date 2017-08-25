/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Diagnostics.Trace;

//Begin Internal Documentation

namespace Adxstudio.Xrm.Web.Handlers
{
	public class AboutProductHandler : BaseDiagnosticsHandler, IRouteHandler, IRequiresSessionState
	{
		#region IRouteHandler

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AboutProductHandler();
		}

		#endregion

		private const string XsrfTokenKey = nameof(AboutProductHandler) + "XSRFToken";

		public override void ProcessRequest(HttpContext context)
		{
			if (string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)
				&& HasAdminPrivileges() && ValidateXSRFToken(context))
			{
				HandleAction(context);
			}

			Render(context);
		}

		private bool ValidateXSRFToken(HttpContext context)
		{
			var token = context.Request.Form[XsrfTokenKey];

			if (string.IsNullOrEmpty(token))
			{
				return false;
			}

			var tokens = token.Split(':');

			if (tokens.Length != 2)
			{
				return false;
			}

			try
			{
				AntiForgery.Validate(tokens[0], tokens[1]);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private string GenerateXSRFToken()
		{
			string cookieToken, formToken;

			AntiForgery.GetTokens(null, out cookieToken, out formToken);

			return cookieToken + ":" + formToken;
		}

		private void HandleAction(HttpContext context)
		{
			if (context.Request.Form["clearCache"] != null)
			{
				ClearCache();
			}
		}

		private void ClearCache()
		{
			Caching.CacheInvalidation.InvalidateAllCache();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "PortalCacheClearAction completed");
		}

		private bool HasAdminPrivileges()
		{
			var contentMap = AdxstudioCrmConfigurationManager.CreateContentMapProvider();
			var webSiteEntity = PortalContext.Current.Website;

			if (webSiteEntity == null)
			{
				return false;
			}

			WebsiteNode webSite = null;
			contentMap.Using(map => map.TryGetValue(webSiteEntity, out webSite));

			if (webSite == null)
			{
				return false;
			}

			// Get names of current user roles
			var roleNames = Roles.GetRolesForUser();

			// Select these role nodes
			var userRoles = webSite.WebRoles.Where(role => roleNames.Contains(role.Name));

			// Select web site access permissions
			var permissions = userRoles.SelectMany(role => role.WebsiteAccesses);
 
			// Check if there is permission with all options active
			var hasAcccess =
				permissions.Any(p => p.ManageContentSnippets.Value && p.ManageSiteMarkers.Value
					&& p.ManageWebLinkSets.Value && p.PreviewUnpublishedEntities.Value);

			return hasAcccess;
		}

		internal override IEnumerable<XObject> ToBody(HttpContext context, ProductInfo product)
		{
			if (HasAdminPrivileges())
			{
				return
					ToSection("Details", new string[0], RenderDetails)
					.Union(RenderTools(context));
			}

			return Enumerable.Empty<XObject>();
		}

		private IEnumerable<XObject> RenderDetails()
		{
			var details = PortalDetail.Instance;

			yield return ToRow("Geo", details.Geo);
			yield return ToRow("Org Id", details.OrgId);
			yield return ToRow("Portal Id", details.PortalId);
			yield return ToRow("Portal Type", details.PortalType);
			yield return ToRow("Tenant Id", details.TenantId);
		}

		private IEnumerable<XObject> RenderTools(HttpContext context)
		{
			yield return new XElement("h3", "Tools");

			var xsrfToken = new XElement("input",
				new XAttribute("type", "hidden"),
				new XAttribute("name", XsrfTokenKey),
				new XAttribute("value", GenerateXSRFToken()));

			var cleanCacheButton = new XElement("input",
				new XAttribute("type", "submit"),
				new XAttribute("name", "clearCache"),
				new XAttribute("value", "Clear Cache"));

			var toolForm = new XElement("form",
				new XAttribute("method", "post"),
				xsrfToken,
				cleanCacheButton,
				new XElement("span", "Warning: Clearing the cache will result in a temporary slowness in your portal as it reloads data from Dynamics 365"));

			yield return toolForm;
		}
	}
}

//End Internal Documentation
