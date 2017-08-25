/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;

namespace Adxstudio.Xrm.Search.Analysis
{
	public class WebsiteAndAppSettingSnowballAnalyzerFactory : AppSettingSnowballAnalyzerFactory
	{
		private readonly PortalContext _portal;

		public WebsiteAndAppSettingSnowballAnalyzerFactory(PortalContext portal)
		{
			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (portal.Website == null)
			{
				throw new NullReferenceException("The portal's Website property is null.");
			}

			portal.Website.AssertEntityName("adx_website");

			_portal = portal;
		}

		protected override bool TryGetSettingValue(string name, out string value)
		{
			value = _portal.ServiceContext.GetSiteSettingValueByName(_portal.Website, name);

			return !string.IsNullOrEmpty(value) || base.TryGetSettingValue(name, out value);
		}
	}
}
