/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Modules;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

//Begin Internal Documentation

namespace Adxstudio.Xrm.Web.Handlers
{
	public abstract class BaseDiagnosticsHandler : IHttpHandler
	{
		private static readonly string ProductName = ResourceManager.GetString("Adxstudio_Portals");

		public bool IsReusable
		{
			get { return false; }
		}

		internal virtual void Render(HttpContext context)
		{
			var product = new ProductInfo();
			var xml = ToXml(context, product);

			using (var writer = XmlWriter.Create(context.Response.Output, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
			{
				xml.WriteTo(writer);
			}
		}

		internal virtual XDocument ToXml(HttpContext context, ProductInfo product)
		{
			var httpContextBase = new HttpContextWrapper(context);
			var routeData = new RouteData();
			var requestContext = new RequestContext(httpContextBase, routeData);
			var urlHelper = new UrlHelper(requestContext);

			var body = new XObject[]
			{
				new XElement(
					"h2",
					ProductName + " ",
					new XElement("small", string.Join(" ", product.Assembly.Version, PortalDetail.Instance.PortalId)))
			}.Concat(ToBody(context, product));

			return new XDocument(
				new XDocumentType("html", null, null, null),
				new XElement("html",
					new XElement("head",
						new XElement("title", ProductName + " " + product.Assembly.Version),
						new XElement("link", new XAttribute("href", urlHelper.Content("~/css/bootstrap.min.css")), new XAttribute("rel", "stylesheet"))),
					new XElement("body",
						new XElement("div", new XAttribute("class", "container"),
							new XElement("div", body)))),
				new XComment(" Page OK "));
		}

		protected virtual IEnumerable<XObject> ToSection(string title, IEnumerable<string> headings, Func<IEnumerable<XObject>> toBody)
		{
			var body = toBody();

			if (!body.Any()) yield break;

			yield return new XElement("h3", title);
			yield return new XElement("table", new XAttribute("class", "table"),
				new XElement("thead",
					new XElement("tr", headings.Select(h => new XElement("th", h)))),
				new XElement("tbody", body));
		}

		protected static XElement ToRow(params object[] values)
		{
			return new XElement("tr", values.Select(value => new XElement("td", value)));
		}

		public abstract void ProcessRequest(HttpContext context);
		internal abstract IEnumerable<XObject> ToBody(HttpContext context, ProductInfo product);
	}

	public class DiagnosticsHandler : BaseDiagnosticsHandler, IRouteHandler
	{
		#region IRouteHandler

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new DiagnosticsHandler();
		}

		#endregion

		public override void ProcessRequest(HttpContext context)
		{
			if (!context.Request.IsLocal)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Not Found.");
			}

			Render(context);
		}

		internal override IEnumerable<XObject> ToBody(HttpContext context, ProductInfo product)
		{
			var assemblies = ToSection("Assemblies", new[] { "Name", "Version", "Location" }, () => product.Assemblies.Select(ToAssembly));
			var iis = ToSection("IIS", new[] { "Name", "Value" }, () => ToIis(context));
			var identity = ToSection("Identity", new[] { "Name", "Value" }, () => ToIdentity(context));
			var claims = ToSection("Claims", new[] { "Type", "Value", "Value Type", "Issuer", "Original Issuer" }, () => ToClaims(context));
			var errors = ToSection("Errors", new[] { "Timestamp", "Dropped", "Message" }, ToErrors);

			var connectionStringName = context.Request["connectionStringName"]
				?? CrmConfigurationManager.GetConnectionStringNameFromContext(null, true)
				?? "Xrm";

			if (!string.IsNullOrWhiteSpace(connectionStringName))
			{
				var connection = new CrmConnection(connectionStringName);

				using (var service = new OrganizationService(connection))
				{
					var connectionSection = ToSection("Connection", new[] { "Name", "Value" }, () => ToConnection(connectionStringName, service));
					var organization = ToSection("Organization", new[] { "Name", "Value" }, () => ToOrganization(service));
					var solutions = ToSection("Solutions", new[] { "UniqueName", "Name", "Version", "Publisher" }, () => ToSolutions(service));
					var portal = ToSection("Portal", new[] { "Name", "Value" }, ToPortal);

					return assemblies.Concat(connectionSection).Concat(organization).Concat(solutions).Concat(portal).Concat(iis).Concat(identity).Concat(claims).Concat(errors);
				}
			}

			var connectionStringNameSection = ToSection("Connection", new[] { "Name", "Value" }, () => new[] { ToRow("ConnectionStringName", connectionStringName) });

			return assemblies.Concat(connectionStringNameSection).Concat(iis).Concat(identity).Concat(claims).Concat(errors);
		}

		private static XElement ToAssembly(ProductInfo.ProductAssembly assembly)
		{
			return ToRow(assembly.Name, assembly.Version, assembly.Location);
		}

		private static IEnumerable<XObject> ToConnection(string connectionStringName, OrganizationService service)
		{
			yield return ToRow("ConnectionStringName", connectionStringName);

			var proxy = service.InnerService;

			yield return ToRow("Proxy", proxy);

			var orgSvcProxy = proxy as OrganizationServiceProxy;

			if (orgSvcProxy != null)
			{
				yield return ToRow("IsAuthenticated", orgSvcProxy.IsAuthenticated);
				yield return ToRow("Timeout", orgSvcProxy.Timeout.ToString());
				yield return ToRow("AuthenticationType", orgSvcProxy.ServiceConfiguration.AuthenticationType);
				yield return ToRow("Address", orgSvcProxy.ServiceConfiguration.CurrentServiceEndpoint.Address);
				yield return ToRow("Identity", orgSvcProxy.ServiceConfiguration.CurrentServiceEndpoint.Address.Identity);

				var authType = orgSvcProxy.ServiceConfiguration.AuthenticationType;
				var uri = orgSvcProxy.ServiceConfiguration.CurrentServiceEndpoint.Address.Uri;
				var organizationName = GetOrganizationName(authType, uri);

				yield return ToRow("OrganizationName", organizationName);
			}
		}

		private static IEnumerable<XObject> ToOrganization(IOrganizationService service)
		{
			var verResponse = service.Execute(new OrganizationRequest("RetrieveVersion"));

			yield return ToRow("Version", verResponse["Version"]);

			var waiResponse = service.Execute(new OrganizationRequest("WhoAmI"));

			yield return ToRow("WhoAmI", waiResponse["UserId"]);

			var orgResponse = service.Execute(new RetrieveMultipleRequest
			{
				Query = new QueryExpression("organization") { ColumnSet = new ColumnSet("name") }
			}) as RetrieveMultipleResponse;

			var organizations = orgResponse.EntityCollection.Entities;

			foreach (var organization in organizations)
			{
				yield return ToRow("Name", organization.GetAttributeValue<string>("name"));
			}
		}

		private static IEnumerable<XObject> ToSolutions(IOrganizationService service)
		{
			using (var context = new CrmOrganizationServiceContext(service))
			{
				var solutions = context.CreateQuery("solution")
					.Where(s => s.GetAttributeValue<bool>("isvisible"))
					.Select(s => new
					{
						UniqueName = s.GetAttributeValue<string>("uniquename"),
						Name = s.GetAttributeValue<string>("friendlyname"),
						Version = s.GetAttributeValue<string>("version"),
						Publisher = s.GetAttributeValue<EntityReference>("publisherid"),
					})
					.ToList();

				return solutions.Select(s => ToRow(s.UniqueName, s.Name, s.Version, s.Publisher.Name));
			}
		}

		private static IEnumerable<XObject> ToPortal()
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			yield return ToRow("PortalContext", portal);
			yield return ToRow("ServiceContext", portal.ServiceContext);

			var container = portal.ServiceContext as IOrganizationServiceContainer;

			if (container != null)
			{
				yield return ToRow("InnerService", container.Service);
			}

			yield return ToRow("Website", portal.Website);

			if (portal.Website != null)
			{
				yield return ToRow("Website Name", portal.Website.GetAttributeValue<string>("adx_name"));
			}
		}

		private static IEnumerable<XObject> ToIis(HttpContext context)
		{
			yield return ToRow("Site Name", HostingEnvironment.SiteName);
			yield return ToRow("Virtual Path", HostingEnvironment.ApplicationVirtualPath);
			yield return ToRow("Host", context.Request.Url.Host);
			yield return ToRow("Port", context.Request.Url.Port);
		}

		private static IEnumerable<XObject> ToIdentity(HttpContext context)
		{
			if (context.User == null) yield break;

			yield return ToRow("User", context.User.ToString());
			yield return ToRow("Identity", context.User.Identity.ToString());
			yield return ToRow("Name", context.User.Identity.Name);
			yield return ToRow("IsAuthenticated", context.User.Identity.IsAuthenticated);
			yield return ToRow("AuthenticationType", context.User.Identity.AuthenticationType);

			var identity = context.User.Identity as ClaimsIdentity;

			if (identity != null)
			{
				yield return ToRow("BootstrapContext", identity.BootstrapContext);
				yield return ToRow("Label", identity.Label);
				yield return ToRow("NameClaimType", identity.NameClaimType);
				yield return ToRow("RoleClaimType", identity.RoleClaimType);
			}
		}

		private static IEnumerable<XObject> ToClaims(HttpContext context)
		{
			if (context.User == null) yield break;

			var identity = context.User.Identity as ClaimsIdentity;

			if (identity != null)
			{
				foreach (var claim in identity.Claims)
				{
					yield return ToRow(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer);
				}
			}
		}

		private static IEnumerable<XObject> ToErrors()
		{
			if (ErrorNotifierModule._errors == null || ErrorNotifierModule._errors.IsEmpty) yield break;

			foreach (var ei in ErrorNotifierModule._errors.Values.OrderBy(e => e.Error.Message))
			{
				yield return ToRow(ei.Timestamp, ei.DropCount, ei.Error.Message);
			}
		}

		private static string GetOrganizationName(AuthenticationProviderType authType, Uri uri)
		{
			if (authType == AuthenticationProviderType.LiveId)
			{
				// take the first sub-domain of the host

				return GetOrganizationNameFromHost(uri);
			}

			if ((authType == AuthenticationProviderType.Federation || authType == AuthenticationProviderType.OnlineFederation)
				&& uri.AbsolutePath.IndexOf("/xrmservices/2011/organization.svc", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return GetOrganizationNameFromHost(uri);
			}

			if (uri.Segments.Length < 2) return null;

			var orgName = uri.Segments[1];

			return orgName.TrimEnd('/');
		}

		private static string GetOrganizationNameFromHost(Uri uri)
		{
			var host = uri.Host;
			var parts = host.Split('.');
			return parts.First();
		}
	}
}

//End Internal Documentation
