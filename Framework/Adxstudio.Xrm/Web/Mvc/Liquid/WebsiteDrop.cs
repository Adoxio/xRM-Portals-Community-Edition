/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Forums;
using IDataAdapterDependencies = Adxstudio.Xrm.Forums.IDataAdapterDependencies;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Blogs.PortalConfigurationDataAdapterDependencies;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Configuration;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.Security;
using DevTrends.MvcDonutCaching;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class WebsiteDrop : PortalViewEntityDrop
	{
		private readonly Lazy<EventDrop[]> _events;

		private readonly Lazy<BlogDrop[]> _blogs;

		private readonly Lazy<LanguageDrop[]> _languages;

		private IEventAggregationDataAdapter _eventAggregationDataAdapter;

		private IBlogAggregationDataAdapter _blogAggregationDataAdapter;

		private IDataAdapterDependencies _dependencies;
	   
		public WebsiteDrop(IPortalLiquidContext portalLiquidContext, IPortalViewEntity viewEntity)
			: base(portalLiquidContext, viewEntity)
		{
		
		}

		public WebsiteDrop(IPortalLiquidContext portalLiquidContext, IPortalViewEntity viewEntity, IDataAdapterDependencies dependencies) : base(portalLiquidContext, viewEntity)
		{
			_dependencies = dependencies;

			_eventAggregationDataAdapter = new WebsiteEventDataAdapter(_dependencies);

			_events = new Lazy<EventDrop[]>(() => _eventAggregationDataAdapter.SelectEvents().Select(e => new EventDrop(this, _dependencies, new Event(e))).ToArray(), LazyThreadSafetyMode.None);

			var blogDependencies = new PortalConfigurationDataAdapterDependencies();
			
			_blogAggregationDataAdapter = new WebsiteBlogAggregationDataAdapter(blogDependencies);

			var urlProvider = blogDependencies.GetUrlProvider();
			var serviceContext = dependencies.GetServiceContext();

			_blogs = new Lazy<BlogDrop[]>(() => _blogAggregationDataAdapter.SelectBlogs()
				.Select(e => new BlogDrop(this, blogDependencies, new Blog(e.Entity, urlProvider.GetApplicationPath(serviceContext, e.Entity))))
				.ToArray(), LazyThreadSafetyMode.None);

			var contextLanguageInfo = portalLiquidContext.ContextLanguageInfo;

			if (!contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				this._languages = new Lazy<LanguageDrop[]>(() => new LanguageDrop[0]);  // Initialize _languages as an empty collection.
			}
			else
			{
				var previewPermission = new PreviewPermission(PortalContext.Current.ServiceContext, PortalContext.Current.Website);
				if (previewPermission.IsEnabledAndPermitted)
				{
					this._languages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.ActiveWebsiteLanguages.Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
				}
				else
				{
					this._languages = new Lazy<LanguageDrop[]>(() => contextLanguageInfo.ActiveWebsiteLanguages.Where(lang => lang.IsPublished).Select(websiteLanguage => new LanguageDrop(this, websiteLanguage)).ToArray());
				}
				this.SelectedLanguage = new LanguageDrop(this, contextLanguageInfo.ContextLanguage);
			}
		}

		public IEnumerable<ForumDrop> Forums
		{
			get 
			{ 
				return new ForumsDrop(this, _dependencies).All;
			}
		}

		public IEnumerable<EventDrop> Events
		{
			get { return EventsEnabled ? _events.Value.AsEnumerable() : null; }
		}

		public IEnumerable<BlogDrop> Blogs
		{
			get { return _blogs.Value.AsEnumerable();  }
		}

		/// <summary>
		/// Selected Language
		/// </summary>
		public LanguageDrop SelectedLanguage { get; private set; }

		/// <summary>
		/// list of languages
		/// </summary>
		public IEnumerable<LanguageDrop> Languages
		{
			get { return this._languages.Value.AsEnumerable(); }
		}

		/// <summary>
		/// Gets the sign-in URL for the website, with return URL for the current page.
		/// </summary>
		public string SignInUrl
		{
			get { return LayoutChildAction("SignInUrl"); }
		}

		/// <summary>
		/// Gets the sign-in URL for the website, with return URL for the current page. 
		/// Returned as a donut cache substitution string if the parent view supports donut holes.
		/// </summary>
		public string SignInUrlSubstitution
		{
			get { return LayoutChildAction("SignInUrl", ViewSupportsDonuts); }
		}

		/// <summary>
		/// Gets the sign-in anchor link for the website.
		/// </summary>
		public string SignInLink
		{
			get { return LayoutChildAction("SignInLink"); }
		}

		/// <summary>
		/// Gets the sign-in anchor link for the website. 
		/// Returned as a donut cache substitution string if the parent view supports donut holes.
		/// </summary>
		public string SignInLinkSubstitution
		{
			get { return LayoutChildAction("SignInLink", ViewSupportsDonuts); }
		}

		/// <summary>
		/// Gets the sign-out URL for the website, with return URL for the current page. 
		/// </summary>
		public string SignOutUrl
		{
			get { return LayoutChildAction("SignOutUrl"); }
		}

		/// <summary>
		/// Gets the sign-out URL for the website, with return URL for the current page. 
		/// Returned as a donut cache substitution string if the parent view supports donut holes.
		/// </summary>
		public string SignOutUrlSubstitution
		{
			get { return LayoutChildAction("SignOutUrl", ViewSupportsDonuts); }
		}

		private string LayoutChildAction(string actionName, bool excludeFromParentCache = false)
		{
			var html = Html.Action(actionName, "Layout", new { area = "Portal" }, excludeFromParentCache);
			return html == null ? null : html.ToString();
		}
	}
}
