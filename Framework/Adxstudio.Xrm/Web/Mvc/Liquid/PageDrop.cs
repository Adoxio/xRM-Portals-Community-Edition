/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Security;
using Microsoft.Xrm.Portal.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PageDrop : PortalViewEntityDrop
	{
		private readonly Lazy<LanguageDrop[]> _availableLanguages;
		private readonly Lazy<LanguageDrop[]> _languages;
		private readonly Lazy<bool> _isPageless;

		public PageDrop(IPortalLiquidContext portalLiquidContext, IPortalViewEntity viewEntity, SiteMapNodeDrop siteMapNode) : base(portalLiquidContext, viewEntity)
		{
			if (siteMapNode == null) throw new ArgumentNullException("siteMapNode");

			SiteMapNode = siteMapNode;
			var current = HttpContext.Current;
			var contextLanguageInfo = portalLiquidContext.ContextLanguageInfo;

			if (!contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				this._availableLanguages = new Lazy<LanguageDrop[]>(() => new LanguageDrop[0]);
				this._languages = new Lazy<LanguageDrop[]>(() => new LanguageDrop[0]);

			}
			else
			{
				var previewPermission = new PreviewPermission(PortalContext.Current.ServiceContext, PortalContext.Current.Website);
				if (previewPermission.IsEnabledAndPermitted)
				{
					this._availableLanguages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.GetWebPageWebsiteLanguages(viewEntity.EntityReference, current).Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
					this._languages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.ActiveWebsiteLanguages.Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
				}
				else
				{
					this._availableLanguages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.GetWebPageWebsiteLanguages(viewEntity.EntityReference, current).Where(lang => lang.IsPublished).Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
					this._languages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.ActiveWebsiteLanguages.Where(lang => lang.IsPublished).Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
				}
			}

			this._isPageless = new Lazy<bool>(() => CrmSiteMapProvider.IsPageless(current));
		}

		public IEnumerable<SiteMapNodeDrop> Breadcrumbs
		{
			get { return SiteMapNode.Breadcrumbs; }
		}

		public IEnumerable<SiteMapNodeDrop> Children
		{
			get { return SiteMapNode.Children; }
		}

		public string Description
		{
			get { return SiteMapNode.Description; }
		}

		public SiteMapNodeDrop Parent
		{
			get
			{
				// If the current page is "pageless" then return the current sitemap node.
				// This is to support legacy liquid template code that uses "page.parent == null" to identify whether
				// the current page is the Home page.
				if (this._isPageless.Value && SiteMapNode.Parent == null)
				{
					return SiteMapNode;
				}
				else
				{
					return SiteMapNode.Parent;
				}
			}
		}

		public string Title
		{
			get { return SiteMapNode.Title; }
		}

		public override string Url
		{
			get { return SiteMapNode.Url; }
		}

		/// <summary>
		/// list of available languages
		/// </summary>
		public IEnumerable<LanguageDrop> AvailableLanguages
		{
			get { return this._availableLanguages.Value.AsEnumerable(); }
		}
		/// <summary>
		/// list of available languages
		/// </summary>
		public IEnumerable<LanguageDrop> Languages
		{
			get { return this._languages.Value.AsEnumerable(); }
		}


		protected SiteMapNodeDrop SiteMapNode { get; private set; }
	}
}
