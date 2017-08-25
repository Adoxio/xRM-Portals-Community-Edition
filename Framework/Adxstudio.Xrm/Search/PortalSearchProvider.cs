/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Adxstudio.Xrm.Search.Facets;

namespace Adxstudio.Xrm.Search
{
	public class PortalSearchProvider : FSDirectorySearchProvider
	{
		protected string PortalName { get; private set; }

		protected string ArticlesLanguageCode { get; private set; }

		protected bool DisplayNotes { get; private set; }

		protected string NotesFilter { get; private set; }

		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			bool displayNotes;

			PortalName = config["portalName"];
			NotesFilter = config["notesFilter"];
			ArticlesLanguageCode = config["articlesLanguageCode"];
			DisplayNotes = bool.TryParse(config["displayNotes"], out displayNotes) && displayNotes;

			var recognizedAttributes = new List<string>
			{
				"portalName",
				"notesFilter",
				"displayNotes",
				"articlesLanguageCode"
			};

			recognizedAttributes.ForEach(config.Remove);

			base.Initialize(name, config);
		}

		protected override ICrmEntityIndexSearcher CreateIndexSearcher(ICrmEntityIndex index)
		{
			var websiteId = GetWebsiteId();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.PortalFacetedNavigation))
			{
				return new PortalFacetedIndexSearcher(index, websiteId);
			}

			return new PortalIndexSearcher(index, websiteId);
		}

		protected override ICrmEntityIndex GetIndex(string dataContextName)
		{

			return new PortalIndex(
				PortalName,
				ArticlesLanguageCode,
				NotesFilter,
				DisplayNotes,
				GetIndexDirectoryFactory().GetDirectory(Version),
				GetIndexAnalyzerFactory().GetAnalyzer(Version),
				Version,
				IndexQueryName,
				dataContextName);
		}

		protected override string GetIndexSearcherName(ICrmEntityIndex index)
		{
			var websiteId = GetWebsiteId();

			return "{0}:{1}:{2}".FormatWith(index.Name, index.Directory, websiteId);
		}

		private Guid GetWebsiteId()
		{
			if (this.WebsiteId != null)
			{
				return this.WebsiteId.Value;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			return portalContext.Website.Id;
		}
	}
}
