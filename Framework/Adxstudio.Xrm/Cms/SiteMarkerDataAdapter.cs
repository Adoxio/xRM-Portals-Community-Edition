/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;

	/// <summary> The SiteMarker data adapter. </summary>
	public class SiteMarkerDataAdapter : ContentMapDataAdapter, ISiteMarkerDataAdapter
	{
		/// <summary> Initializes a new instance of the <see cref="SiteMarkerDataAdapter"/> class. </summary>
		/// <param name="dependencies"> The dependencies. </param>
		public SiteMarkerDataAdapter(IDataAdapterDependencies dependencies) : base(dependencies)
		{
		}

		/// <summary> Selects the SiteMarker by name. </summary>
		/// <param name="siteMarkerName"> The site marker name. </param>
		/// <returns> The <see cref="ISiteMarkerTarget"/>. </returns>
		public ISiteMarkerTarget Select(string siteMarkerName)
		{
			if (string.IsNullOrEmpty(siteMarkerName))
			{
				return null;
			}

			return this.ContentMapProvider.Using(contentMap => this.Select(siteMarkerName, contentMap));
		}

		/// <summary> Selects the SiteMarker by name with read access. </summary>
		/// <param name="siteMarkerName"> The site marker name. </param>
		/// <returns> The <see cref="ISiteMarkerTarget"/>. </returns>
		public ISiteMarkerTarget SelectWithReadAccess(string siteMarkerName)
		{
			if (string.IsNullOrEmpty(siteMarkerName))
			{
				return null;
			}

			return this.ContentMapProvider.Using(contentMap => this.SelectWithReadAccess(siteMarkerName, contentMap));
		}

		/// <summary> The try get page node from site marker node. </summary>
		/// <param name="siteMarkerName"> The site marker name. </param>
		/// <param name="contentMap"> The content map. </param>
		/// <param name="targetNode"> The target node. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		private bool TryGetPageNodeFromSiteMarkerNode(string siteMarkerName, ContentMap contentMap, out WebPageNode targetNode)
		{
			var website = HttpContext.Current.GetWebsite().Entity;
			IDictionary<EntityReference, EntityNode> siteMarkers;
			targetNode = null;

			if (!contentMap.TryGetValue("adx_sitemarker", out siteMarkers))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No Sitemarkers found on Content Map");
				return false;
			}

			var siteMarkerNode = siteMarkers.Values
				.Cast<SiteMarkerNode>()
				.FirstOrDefault(e => e.Website.Id == website.Id && e.Name == siteMarkerName);

			if (siteMarkerNode == null || siteMarkerNode.WebPage == null)
			{
				return false;
			}

			if (!contentMap.TryGetValue(siteMarkerNode.WebPage, out targetNode))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("No WebPage found on Sitemarker:{0}", siteMarkerNode.Id));
				return false;
			}

			if (!this.Language.IsCrmMultiLanguageEnabled)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("WebPage found for Sitemarker:{0} Page:{1}", siteMarkerNode.Id, targetNode.Id));
				return true;
			}

			// MLP - Find the content webpage for the current language from the target page
			var contentWebPage = targetNode.LanguageContentPages.FirstOrDefault(p => p.WebPageLanguage == this.Language.ContextLanguage.WebsiteLanguageNode);
			if (contentWebPage != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("WebPage found for Sitemarker:{0} Language:{1}", siteMarkerNode.Id, this.Language.ContextLanguage.Lcid));
				targetNode = contentWebPage;
				return true;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("No WebPage found for Sitemarker:{0} Language:{1}", siteMarkerNode.Id, this.Language.ContextLanguage.Lcid));
			return false;
		}

		/// <summary> Select the site marker target page. </summary>
		/// <param name="siteMarkerName"> The site marker name. </param>
		/// <param name="contentMap"> The content map. </param>
		/// <returns> The <see cref="ISiteMarkerTarget"/>. </returns>
		private ISiteMarkerTarget Select(string siteMarkerName, ContentMap contentMap)
		{
			WebPageNode target;

			if (!this.TryGetPageNodeFromSiteMarkerNode(siteMarkerName, contentMap, out target))
			{
				return null;
			}

			return this.ToSiteMarkerTarget(target);
		}

		/// <summary> Select site marker target page with read access. </summary>
		/// <param name="siteMarkerName"> The site marker name. </param>
		/// <param name="contentMap"> The content map. </param>
		/// <returns> The <see cref="ISiteMarkerTarget"/>. </returns>
		private ISiteMarkerTarget SelectWithReadAccess(string siteMarkerName, ContentMap contentMap)
		{
			var securityProvider = this.Dependencies.GetSecurityProvider();
			var serviceContext = this.Dependencies.GetServiceContext();
			WebPageNode target;

			if (!this.TryGetPageNodeFromSiteMarkerNode(siteMarkerName, contentMap, out target))
			{
				return null;
			}

			if (!securityProvider.TryAssert(serviceContext, target.ToEntity(), CrmEntityRight.Read))
			{
				return null;
			}

			return this.ToSiteMarkerTarget(target);
		}

		/// <summary> The to site marker target. </summary>
		/// <param name="webPageNode"> The web page node. </param>
		/// <returns> The <see cref="ISiteMarkerTarget"/>. </returns>
		private ISiteMarkerTarget ToSiteMarkerTarget(WebPageNode webPageNode)
		{
			if (webPageNode == null)
			{
				return null;
			}

			var urlProvider = this.Dependencies.GetUrlProvider();
			var securityProvider = this.Dependencies.GetSecurityProvider();
			var serviceContext = this.Dependencies.GetServiceContext();
			var entity = webPageNode.AttachTo(serviceContext);

			var portalViewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider);
			var siteMarkerTarget = new SiteMarkerTarget(entity, portalViewEntity, urlProvider.GetApplicationPath(serviceContext, entity));

			return siteMarkerTarget;
		}
	}
}
