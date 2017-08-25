/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using Adxstudio.Xrm.Resources;
using Lucene.Net.Store;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;
using Directory=Lucene.Net.Store.Directory;
using Version=Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Store
{
	public class WebsiteSettingDirectoryFactory : IDirectoryFactory
	{
		private readonly PortalContext _portal;

		public WebsiteSettingDirectoryFactory(PortalContext portal)
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

		public Directory GetDirectory(Version version)
		{
			var indexPathSetting = _portal.ServiceContext.GetSiteSettingValueByName(_portal.Website, "Adxstudio.Xrm.Search.Index.DirectoryPath");

			return string.IsNullOrEmpty(indexPathSetting) ? null : FSDirectory.Open(new DirectoryInfo(indexPathSetting));
		}
	}
}
