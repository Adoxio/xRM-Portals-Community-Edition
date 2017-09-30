/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for Adxstudio Portals applications.
	/// </summary>
	public static class PortalExtensions
	{
		/// <summary>
		/// Key which the helpers in this class will use to attempt to retrieve a <see cref="IPortalViewContext"/> from view data.
		/// </summary>
		/// <remarks>
		/// If this <see cref="IPortalViewContext"/> is not found in view data, most of the helpers in this class will fail with
		/// an <see cref="InvalidOperationException"/> error.
		/// </remarks>
		public static readonly string PortalViewContextKey = typeof(IPortalViewContext).FullName;

		/// <summary>
		/// Gets the <see cref="IPortalViewEntity"/> for the current portal user. (In most configurations, this will be a Contact record.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <returns>
		/// The <see cref="IPortalViewEntity"/> for the current portal user. If there is no portal user associated with the current
		/// request context (such as in anonymous/unauthenticated requests), returns null.
		/// </returns>
		public static IPortalViewEntity PortalUser(this HtmlHelper html)
		{
			return GetPortalViewContext(html).User;
		}

		/// <summary>
		/// Gets the <see cref="IPortalViewEntity"/> for the current portal website (adx_website).
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <returns>
		/// The <see cref="IPortalViewEntity"/> for the current portal website.
		/// </returns>
		public static IPortalViewEntity Website(this HtmlHelper html)
		{
			return GetPortalViewContext(html).Website;
		}

		public static string GetPortalScopedRouteUrlByName(this HtmlHelper html, string routeName)
		{
			return GetUrlHelper().RouteUrl(routeName, new { __portalScopeId__ = GetPortalViewContext(html).Website.EntityReference.Id });
		}

		/// <summary>
		/// Gets the current <see cref="RequestContext"/>.
		/// </summary>
		/// <returns>The current <see cref="RequestContext"/>.</returns>
		public static RequestContext GetRequestContext()
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
			}

			var http = new HttpContextWrapper(current);
			var requestContext = new RequestContext(http, RouteTable.Routes.GetRouteData(http) ?? new RouteData());

			return requestContext;
		}

		private static UrlHelper GetUrlHelper()
		{
			var requestContext = GetRequestContext();

			return new UrlHelper(requestContext, RouteTable.Routes);
		}
		
		private static readonly IEnumerable<string> _editingScripts = new[]
		{
			"~/xrm-adx/js/xrm-preload.js",
			"~/xrm/js/xrm-combined-js.aspx",
			"~/xrm-adx/js/xrm-combined-js.aspx"
		};

		/// <summary>
		/// Returns HTML to render script includes for script dependencies required by the portal inline editing system. These
		/// script references are only rendered on the condition that the current view context indicates that inline editing
		/// support is required for the current view.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="dependencyScriptPaths">
		/// Scripts to be loaded before all framework scripts.
		/// </param>
		/// <param name="extensionScriptPaths">
		/// Scripts to be laoded after all framework scripts.
		/// </param>
		/// <returns>
		/// Returns HTML to render script includes for script dependencies required by the portal inline editing system. If the
		/// current view context indicates that editing support is not required by the current view, returns an empty string.
		/// </returns>
		public static IHtmlString EditingScripts(this HtmlHelper html, IEnumerable<string> dependencyScriptPaths = null, IEnumerable<string> extensionScriptPaths = null)
		{
			var portalViewContext = GetPortalViewContext(html);

			if (!portalViewContext.EnableEditing)
			{
				return new HtmlString(string.Empty);
			}

			dependencyScriptPaths = dependencyScriptPaths ?? new string[] { };
			extensionScriptPaths = extensionScriptPaths ?? new string[] { };

			var output = new StringBuilder();

			foreach (var path in dependencyScriptPaths.Concat(_editingScripts.Concat(extensionScriptPaths)))
			{
				var script = new TagBuilder("script");

				script.Attributes["src"] = VirtualPathUtility.IsAppRelative(path) ? VirtualPathUtility.ToAbsolute(path) : path;

				output.Append(script);
			}

			return new HtmlString(output.ToString());
		}

		private static readonly IEnumerable<string> _editingStyles = new[]
		{
			"~/xrm/css/editable.css",
			"~/xrm-adx/css/editable.css"
		};

		/// <summary>
		/// Returns HTML to render style imports for CSS stylesheets required by the portal inline editing system. These
		/// style imports are only rendered on the condition that the current view context indicates that inline editing
		/// support is required for the current view.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="dependencyStylePaths">
		/// Styles to be loaded before all framework styles.
		/// </param>
		/// <param name="extensionStylePaths">
		/// Styles to be laoded after all framework Styles.
		/// </param>
		/// <returns>
		/// Returns HTML to render script includes for style imports required by the portal inline editing system. If the
		/// current view context indicates that editing support is not required by the current view, returns an empty string.
		/// </returns>
		public static IHtmlString EditingStyles(this HtmlHelper html, IEnumerable<string> dependencyStylePaths = null, IEnumerable<string> extensionStylePaths = null)
		{
			var portalViewContext = GetPortalViewContext(html);

			if (!portalViewContext.EnableEditing)
			{
				return new HtmlString(string.Empty);
			}

			dependencyStylePaths = dependencyStylePaths ?? new string[] { };
			extensionStylePaths = extensionStylePaths ?? new string[] { };

			var output = new StringBuilder();

			foreach (var path in dependencyStylePaths.Concat(_editingStyles.Concat(extensionStylePaths)))
			{
				var script = new TagBuilder("style");

				script.Attributes["type"] = "text/css";
				script.InnerHtml = @"@import url(""{0}"");".FormatWith(VirtualPathUtility.ToAbsolute(path));

				output.Append(script.ToString());
			}

			return new HtmlString(output.ToString());
		}

		public static RelatedWebsites RelatedWebsites(this HtmlHelper html, string linkTitleSiteSettingName = "Site Name")
		{
			var portalViewContext = GetPortalViewContext(html);
			var serviceContext = portalViewContext.CreateServiceContext();

			var website =
				serviceContext.CreateQuery("adx_website")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_websiteid") == portalViewContext.Website.EntityReference.Id);

			if (website == null)
			{
				return new RelatedWebsites(Enumerable.Empty<RelatedWebsiteLink>());
			}

			var masterWebsite = website.GetRelatedEntity(serviceContext, new Relationship("adx_website_parentwebsite") { PrimaryEntityRole = EntityRole.Referencing })
				?? website;

			var subscriberWebsites = masterWebsite
				.GetRelatedEntities(serviceContext, new Relationship("adx_website_parentwebsite") { PrimaryEntityRole = EntityRole.Referenced })
				.Select(e => new RelatedWebsite(e, false, e.ToEntityReference().Equals(website.ToEntityReference())))
				.ToArray();

			if (!subscriberWebsites.Any())
			{
				return new RelatedWebsites(Enumerable.Empty<RelatedWebsiteLink>());
			}

			var relatedWebsites = subscriberWebsites.Union(new[]
			{
				new RelatedWebsite(masterWebsite, true, masterWebsite.ToEntityReference().Equals(website.ToEntityReference()))
			});

			var links = relatedWebsites.Select(relatedWebsite => new
			{
				Url = GetTargetUrl(serviceContext, relatedWebsite, portalViewContext.Entity),
				Title = serviceContext.GetSiteSettingValueByName(relatedWebsite.Entity, linkTitleSiteSettingName) ?? relatedWebsite.Entity.GetAttributeValue<string>("adx_name"),
				relatedWebsite.IsCurrent
			})
			.Where(e => e.Url != null && !string.IsNullOrEmpty(e.Title))
			.Select(e => new RelatedWebsiteLink(e.Title, e.Url, e.IsCurrent));

			return new RelatedWebsites(links);
		}

		internal static NameValueCollection AnonymousObjectToQueryStringParameters(object queryStringParameters)
		{
			var parameters = new NameValueCollection();

			if (queryStringParameters == null)
			{
				return parameters;
			}

			foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(queryStringParameters))
			{
				var propertyValue = propertyDescriptor.GetValue(queryStringParameters);

				parameters.Add(propertyDescriptor.Name, propertyValue == null ? string.Empty : propertyValue.ToString());
			}

			return parameters;
		}

		internal static string GetAttributeLogicalNameFromExpression<TEntity>(Expression<Func<TEntity, object>> attributeExpression)
		{
			if (attributeExpression == null)
			{
				throw new ArgumentNullException("attributeExpression");
			}

			if (attributeExpression.Body.NodeType != ExpressionType.MemberAccess)
			{
				throw new ArgumentException("Expression body must be of type {0}.".FormatWith(ExpressionType.MemberAccess), "attributeExpression");
			}

			var memberExpression = (MemberExpression)attributeExpression.Body;

			var propertyInfo = memberExpression.Member as PropertyInfo;

			if (propertyInfo == null)
			{
				throw new ArgumentException("Expression body must be a property accessor.", "attributeExpression");
			}

			var entityInfo = new EntityInfo(typeof(TEntity));

			AttributeInfo attributeInfo;

			if (!entityInfo.AttributesByPropertyName.TryGetValue(propertyInfo.Name, out attributeInfo))
			{
				throw new ArgumentException("Unable to retrieve the entity attribute metadata from expression property name.", "attributeExpression");
			}

			return attributeInfo.CrmPropertyAttribute.LogicalName;
		}

		public static IPortalViewContext GetPortalViewContext(HtmlHelper html)
		{
			if (html == null)
			{
				throw new ArgumentNullException("html");
			}

			object viewData;

			if (html.ViewData != null && html.ViewData.TryGetValue(PortalViewContextKey, out viewData))
			{
				var portalViewContext = viewData as IPortalViewContext;

				if (portalViewContext != null)
				{
					return portalViewContext;
				}
			}

			if (html.ViewContext != null && html.ViewContext.ViewData != null && html.ViewContext.ViewData.TryGetValue(PortalViewContextKey, out viewData))
			{
				var portalViewContext = viewData as IPortalViewContext;

				if (portalViewContext != null)
				{
					return portalViewContext;
				}
			}

			if (html.ViewDataContainer != null && html.ViewDataContainer.ViewData != null && html.ViewDataContainer.ViewData.TryGetValue(PortalViewContextKey, out viewData))
			{
				var portalViewContext = viewData as IPortalViewContext;

				if (portalViewContext != null)
				{
					return portalViewContext;
				}
			}

			throw new ArgumentException("Failed to retrieve IPortalViewContext from ViewData.", "html");
		}

		private class RelatedWebsite
		{
			public RelatedWebsite(Entity entity, bool isMaster, bool isCurrent)
			{
				if (entity == null) throw new ArgumentNullException("entity");

				Entity = entity;
				IsMaster = isMaster;
				IsCurrent = isCurrent;
			}

			public Entity Entity { get; private set; }

			public bool IsCurrent { get; private set; }

			public bool IsMaster { get; private set; }
		}

		private static string GetTargetUrl(OrganizationServiceContext serviceContext, RelatedWebsite website, IPortalViewEntity current)
		{
			if (website.IsCurrent)
			{
				return current.Url;
			}

			if (current.EntityReference.LogicalName == "adx_webpage")
			{
				if (website.IsMaster)
				{
					var masterWebPageQuery =
						serviceContext.CreateQuery("adx_webpage")
							.Join(serviceContext.CreateQuery("adx_webpage"),
								webPage => webPage.GetAttributeValue<Guid?>("adx_webpageid"),
								subscriberWebPage => subscriberWebPage.GetAttributeValue<EntityReference>("adx_masterwebpageid").Id,
								(webPage, subscriberWebPage) => new { webPage, subscriberWebPage })
							.Where(@t => @t.subscriberWebPage.GetAttributeValue<Guid?>("adx_webpageid") == current.EntityReference.Id)
							.Where(@t => @t.webPage.GetAttributeValue<EntityReference>("adx_websiteid") == website.Entity.ToEntityReference())
							.Where(@t => @t.webPage.GetAttributeValue<OptionSetValue>("statecode") != null && @t.webPage.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
							.Select(@t => @t.webPage);

					var masterWebPage = masterWebPageQuery.FirstOrDefault();

					if (masterWebPage != null)
					{
						return serviceContext.GetUrl(masterWebPage);
					}
				}
				else
				{
					var subscriberWebPage = serviceContext.CreateQuery("adx_webpage")
						.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_masterwebpageid") == current.EntityReference
							&& e.GetAttributeValue<EntityReference>("adx_websiteid") == website.Entity.ToEntityReference()
							&& e.GetAttributeValue<OptionSetValue>("statecode") != null && e.GetAttributeValue<OptionSetValue>("statecode").Value == 0);

					if (subscriberWebPage != null)
					{
						return serviceContext.GetUrl(subscriberWebPage);
					}
				}
			}

			var homePage = serviceContext.GetPageBySiteMarkerName(website.Entity, "Home");

			return homePage == null ? null : serviceContext.GetUrl(homePage);
		}
	}

	public class RelatedWebsites
	{
		public RelatedWebsites(IEnumerable<RelatedWebsiteLink> links)
		{
			Links = links.OrderBy(link => link.Title).ToArray();
			Current = Links.FirstOrDefault(e => e.IsCurrent);
			Any = Links.Count() > 1 && Current != null;
		}

		public bool Any { get; private set; }

		public RelatedWebsiteLink Current { get; private set; }

		public IEnumerable<RelatedWebsiteLink> Links { get; private set; }
	}

	public class RelatedWebsiteLink
	{
		public RelatedWebsiteLink(string title, string url, bool isCurrent)
		{
			Title = title;
			Url = url;
			IsCurrent = isCurrent;
		}

		public bool IsCurrent { get; private set; }

		public string Title { get; private set; }

		public string Url { get; private set; }
	}
}
