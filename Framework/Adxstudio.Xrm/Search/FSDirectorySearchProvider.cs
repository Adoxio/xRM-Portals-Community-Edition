/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Search
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration.Provider;
	using System.IO;
	using System.Linq;
	using System.Web.Hosting;
	using Adxstudio.Xrm.Globalization;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Search.Analysis;
	using Adxstudio.Xrm.Search.Index;
	using Adxstudio.Xrm.Search.Store;
	using Lucene.Net.Index;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Version = Lucene.Net.Util.Version;
	using System.Globalization;

	public class FSDirectorySearchProvider : SearchProvider
	{
		private enum IndexSearcherCacheMode
		{
			Shared,
			SingleUse
		}

		private const string _defaultIndexQueryName = "Portal Search";

		private static readonly IIndexSearcherPool _sharedSearcherPool = new SharedIndexSearcherCache();

		private IIndexSearcherPool _searcherPool;

		protected virtual DirectoryInfo IndexDirectory { get; private set; }

		protected string IndexQueryName { get; private set; }

		protected bool UseEncryptedDirectory { get; private set; }

		protected bool IsOnlinePortal { get; private set; }

		protected string SearchDataContextName { get; private set; }

		protected string Stemmer { get; private set; }

		protected IEnumerable<string> StopWords { get; private set; }

		protected string UpdateDataContextName { get; private set; }

		protected virtual Version Version { get; private set; }

		/// <summary>
		/// The website Id.
		/// </summary>
		protected Guid? WebsiteId { get; private set; }

		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (string.IsNullOrEmpty(name))
			{
				name = GetType().Name;
			}

			base.Initialize(name, config);

			var dataContextName = config["dataContextName"];

			SearchDataContextName = config["searchDataContextName"] ?? dataContextName;
			UpdateDataContextName = config["updateDataContextName"] ?? dataContextName;

			var indexPath = config["indexPath"];

			if (string.IsNullOrEmpty(indexPath))
			{
				throw new ProviderException("The search provider {0} requires the attribute indexPath to be set.".FormatWith(name));
			}

			IndexDirectory = new DirectoryInfo(
				(indexPath.StartsWith(@"~/", StringComparison.Ordinal) || indexPath.StartsWith(@"~\", StringComparison.Ordinal))
					? HostingEnvironment.MapPath(indexPath) ?? indexPath
					: indexPath);

			var useEncryptedDirectory = false;
			bool.TryParse(config["useEncryptedDirectory"], out useEncryptedDirectory);
			UseEncryptedDirectory = useEncryptedDirectory;

			var isOnlinePortal = false;
			bool.TryParse(config["isOnlinePortal"], out isOnlinePortal);
			IsOnlinePortal = isOnlinePortal;

			IndexQueryName = config["indexQueryName"] ?? _defaultIndexQueryName;
			Stemmer = CultureInfo.CurrentUICulture.Parent.EnglishName;

			var stopWords = config["stopWords"];

			StopWords = string.IsNullOrEmpty(stopWords)
				? new string[] { }
				: stopWords.Split(',').Select(word => word.Trim()).Where(word => !string.IsNullOrEmpty(word)).ToArray();

			var searcherCacheMode = config["indexSearcherCacheMode"];

			IndexSearcherCacheMode mode;

			_searcherPool = !string.IsNullOrEmpty(searcherCacheMode) && Enum.TryParse(searcherCacheMode, out mode) && mode == IndexSearcherCacheMode.SingleUse
				? new SingleUseIndexSearcherPool()
				: _sharedSearcherPool;

			var indexVersion = config["indexVersion"];

			Version version;

			Version = !string.IsNullOrEmpty(indexVersion) && Enum.TryParse(indexVersion, out version)
				? version
				: Version.LUCENE_23;

			Guid websiteId;
			WebsiteId = Guid.TryParse(config["websiteId"], out websiteId)
				? websiteId
				: (Guid?)null;

			var recognizedAttributes = new List<string>
			{
				"name",
				"description",
				"dataContextName",
				"indexPath",
				"indexQueryName",
				"indexSearcherCacheMode",
				"indexVersion",
				"searchDataContextName",
				"stemmer",
				"stopWords",
				"updateDataContextName",
				"isOnlinePortal",
				"useEncryptedDirectory",
				"websiteId",
			};

			// Remove all of the known configuration values. If there are any left over, they are unrecognized.
			recognizedAttributes.ForEach(config.Remove);

			if (config.Count > 0)
			{
				var unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ProviderException("The search provider {0} does not currently recognize or support the attribute {1}.".FormatWith(name, unrecognizedAttribute));
				}
			}
		}

		public override ICrmEntityIndexBuilder GetIndexBuilder()
		{
			return new EventedIndexBuilder(new CrmEntityIndexBuilder(GetUpdateIndex()), () => RefreshSearcher(GetSearchIndex()));
		}

		public override ICrmEntityIndexUpdater GetIndexUpdater()
		{
			return new EventedIndexUpdater(new CrmEntityIndexBuilder(GetUpdateIndex()), () => RefreshSearcher(GetSearchIndex()));
		}

		public override ICrmEntityIndexSearcher GetIndexSearcher()
		{
			var index = GetSearchIndex();

			return _searcherPool.Get(GetIndexSearcherName(index), () => CreateIndexSearcher(index));
		}

		public override IRawLuceneIndexSearcher GetRawLuceneIndexSearcher()
		{
			var index = GetSearchIndex();

			return new CrmEntityIndexSearcher(index);
		}


        public override IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo()
		{
			return GetIndexedEntityInfo(null);
		}

		public override IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode)
		{
			return GetIndexedEntityInfo(languageCode);
		}

        protected virtual ICrmEntityIndexSearcher CreateIndexSearcher(ICrmEntityIndex index)
		{
			return new CrmEntityIndexSearcher(index);
		}

		protected virtual ICrmEntityIndex GetIndex(string dataContextName)
		{
			return new WebCrmEntityIndex(
				GetIndexDirectoryFactory().GetDirectory(Version),
				GetIndexAnalyzerFactory().GetAnalyzer(Version),
				Version,
				IndexQueryName,
				dataContextName);
		}

		protected virtual ICrmEntityIndex GetSearchIndex()
		{
			return GetIndex(SearchDataContextName);
		}

		protected virtual ICrmEntityIndex GetUpdateIndex()
		{
			return GetIndex(UpdateDataContextName);
		}

		protected IAnalyzerFactory GetIndexAnalyzerFactory()
		{
			return new CompositeAnalyzerFactory(
				new SnowballAnalyzerFactory(Stemmer, StopWords),
				new DefaultAnalyzerFactory());
		}

		protected IDirectoryFactory GetIndexDirectoryFactory()
		{
			return new CompositeDirectoryFactory(
				new DirectoryInfoDirectoryFactory(IndexDirectory, UseEncryptedDirectory, IsOnlinePortal),
				new DefaultDirectoryFactory());
		}

		protected virtual string GetIndexSearcherName(ICrmEntityIndex index)
		{
			return "{0}:{1}".FormatWith(index.Name, index.Directory);
		}

		protected void RefreshSearcher(ICrmEntityIndex index)
		{
			_searcherPool.Refresh(GetIndexSearcherName(index));
		}

		private IEnumerable<CrmEntityIndexInfo> GetIndexedEntityInfo(int? languageCode)
		{
			var index = GetUpdateIndex();

			var logicalNames = GetIndexedEntityLogicalNames(index);

			var response = (RetrieveAllEntitiesResponse)index.DataContext.Execute(new RetrieveAllEntitiesRequest { EntityFilters = EntityFilters.Entity });

			return response.EntityMetadata
				.Where(m => logicalNames.Contains(m.LogicalName))
				.Select(m => new CrmEntityIndexInfo(m.LogicalName, GetEntityDisplayName(m, languageCode), GetEntityDisplayCollectionName(m, languageCode)))
				.ToArray();
		}

		private static string GetEntityDisplayCollectionName(EntityMetadata metadata, int? languageCode)
		{
			if (languageCode == null)
			{
				return metadata.DisplayCollectionName.GetLocalizedLabelString();
			}

			var label = metadata.DisplayCollectionName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode.Value);

			return label != null ? label.Label : null;
		}

		private static string GetEntityDisplayName(EntityMetadata metadata, int? languageCode)
		{
			if (languageCode == null)
			{
				return metadata.DisplayName.GetLocalizedLabelString();
			}

			var label = metadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode.Value);

			return label != null ? label.Label : null;
		}

		private static List<string> GetIndexedEntityLogicalNames(ICrmEntityIndex index)
		{
			var logicalNames = new List<string>();
			
			try
			{
				using (var reader = IndexReader.Open(index.Directory, true))
				using (var terms = reader.Terms(new Term(index.LogicalNameFieldName)))
				{
					while (terms.Term.Field == index.LogicalNameFieldName)
					{
						logicalNames.Add(terms.Term.Text);

						if (!terms.Next())
						{
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				SearchEventSource.Log.ReadError(e);

				return logicalNames;
			}

			return logicalNames;
		}
	}
}
