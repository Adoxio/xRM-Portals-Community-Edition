/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Metadata;
using DotLiquid;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public abstract class PortalDrop : Drop, IPortalLiquidContext
	{
		private readonly Lazy<bool> _eventsEnabled;
		private readonly IPortalLiquidContext _portalLiquidContext;

		protected PortalDrop(IPortalLiquidContext portalLiquidContext)
		{
			if (portalLiquidContext == null) throw new ArgumentNullException("portalLiquidContext");

			_portalLiquidContext = portalLiquidContext;
			_eventsEnabled = new Lazy<bool>(GetEventsEnabled, LazyThreadSafetyMode.None);
		}

		public HtmlHelper Html
		{
			get
			{
                // If a custom HtmlHelper was saved earlier to the Liquid Context, then use it.
                // Otherwise return default PortalLiquidContext's HtmlHelper.
                return Context != null
					? (Context.Registers["htmlHelper"] as HtmlHelper) ?? _portalLiquidContext.Html
					: _portalLiquidContext.Html;
			}
		}

		public IOrganizationMoneyFormatInfo OrganizationMoneyFormatInfo
		{
			get { return _portalLiquidContext.OrganizationMoneyFormatInfo; }
		}

		public IPortalViewContext PortalViewContext
		{
			get { return _portalLiquidContext.PortalViewContext; }
		}

		public Random Random
		{
			get { return _portalLiquidContext.Random; }
		}

		public UrlHelper UrlHelper
		{
			get { return _portalLiquidContext.UrlHelper; }
		}

		protected string GetUserImageUrl(string email, int? size = null)
		{
			return VirtualPathUtility.ToAbsolute("~/xrm-adx/images/contact_photo.png");
		}

		protected internal bool EventsEnabled
		{
			get { return _eventsEnabled.Value; }
		}

		public ContextLanguageInfo ContextLanguageInfo
		{
			get { return _portalLiquidContext.ContextLanguageInfo; }
		}

		public IOrganizationService PortalOrganizationService
		{
			get { return _portalLiquidContext.PortalOrganizationService; }
		}

        /// <summary>
        /// Gets whether the View that this drop is in supports donuts for donut-hole-caching purposes.
        /// </summary>
		protected bool ViewSupportsDonuts
		{
			get { return Context != null && Context.ViewSupportsDonuts(); }
		}

		private bool GetEventsEnabled()
		{
			return SolutionDependenciesAvailable(new[]
			{
				"AdxstudioEventManagement"
			});
		}

		private bool SolutionDependenciesAvailable(string[] requiredSolutions)
		{
			if (requiredSolutions == null || !requiredSolutions.Any())
			{
				return true;
			}

			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(this.PortalViewContext.PortalName);

			if (contentMapProvider == null)
			{
				return true;
			}

			var availableSolutions = contentMapProvider.Using(map => map.Solution.Solutions);

			return requiredSolutions.Intersect(availableSolutions, StringComparer.OrdinalIgnoreCase).Count() == requiredSolutions.Length;
		}
	}
}
