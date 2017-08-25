/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search
{
	using System;
	using System.Collections.Generic;
	using Adxstudio.Xrm.Search.Index;
	using Fetch = Adxstudio.Xrm.Services.Query;
	using Lucene.Net.Analysis;
	using Lucene.Net.Search;
	using Lucene.Net.Store;
	using Microsoft.Xrm.Client.Configuration;
	using Microsoft.Xrm.Sdk.Client;
	using Version = Lucene.Net.Util.Version;

	public class WebCrmEntityIndex : ICrmEntityIndex
	{
		private const string _contentFieldName = "_content";
		private const string _isUrlDefinedFieldName = "_isurldefined";
		private const string _languageLocaleCodeFieldName = "_language";
		private const string _languageLocaleLCIDFieldName = "_language_lcid";
		private const string _languageLocaleCodeDefaultValue = " ";
		private const string _logicalNameFieldName = "_logicalname";
		private const string _primaryKeyFieldName = "_primarykey";
		private const string _primaryKeyLogicalNameFieldName = "_primarykeylogicalname";
		private const string _scopeDataSourceFieldName = "adx_websiteid";
		private const string _scopeFieldName = "_websitescope";
		private const string _titleFieldName = "_title";

		/// <summary>
		/// The web role field name.
		/// </summary>
		private const string _webRoleFieldName = "_webRole";

		private readonly string _dataContextName;
		private readonly Analyzer _defaultAnalyzer;

		public WebCrmEntityIndex(Directory directory, Analyzer analyzer, Version version, string indexQueryName)
		{
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}

			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}

			if (indexQueryName == null)
			{
				throw new ArgumentNullException("indexQueryName");
			}

			Directory = directory;
			Version = version;
			IndexQueryName = indexQueryName;

			Name = string.IsNullOrEmpty(_dataContextName)
				? CrmConfigurationManager.GetCrmSection().Contexts.Current.Name
				: _dataContextName;

			_defaultAnalyzer = analyzer;

			Analyzer = GetAnalyzer();
		}

		public WebCrmEntityIndex(Directory directory, Analyzer analyzer, Version version, string indexQueryName, string dataContextName) : this(directory, analyzer, version, indexQueryName)
		{
			_dataContextName = dataContextName;
		}

		public virtual bool AddScopeField
		{
			get { return true; }
		}

		public virtual Analyzer Analyzer { get; private set; }

		public virtual string ContentFieldName
		{
			get { return _contentFieldName; }
		}

		public virtual OrganizationServiceContext DataContext
		{
			get
			{
				var context = CrmConfigurationManager.CreateContext(_dataContextName);

				context.MergeOption = MergeOption.NoTracking;

				return context;
			}
		}

		public Directory Directory { get; private set; }

		public virtual string IsUrlDefinedFieldName
		{
			get
			{
				return _isUrlDefinedFieldName;
			}
		}

		public virtual string LanguageLocaleCode { get; set; }

		public virtual string LanguageLocaleCodeDefaultValue
		{
			get { return _languageLocaleCodeDefaultValue; }
		}

		public virtual string LanguageLocaleCodeFieldName
		{
			get { return _languageLocaleCodeFieldName; }
		}

		public string LanguageLocaleLCIDFieldName
		{
			get { return _languageLocaleLCIDFieldName; }
		}

		public virtual string LogicalNameFieldName
		{
			get { return _logicalNameFieldName; }
		}

		public virtual string Name { get; private set; }

		public virtual string PrimaryKeyFieldName
		{
			get { return _primaryKeyFieldName; }
		}

		public virtual string PrimaryKeyLogicalNameFieldName
		{
			get { return _primaryKeyLogicalNameFieldName; }
		}

		public virtual string ProductAccessDefaultValue
		{
			get
			{
				return "_noassociatedproducts";
			}
		}

		public virtual string ProductAccessNonKnowledgeArticleDefaultValue
		{
			get
			{
				return "_notknowledgearticleentity";
			}
		}

		public virtual string ScopeDefaultValue
		{
			get { return Guid.Empty.ToString(); }
		}

		public virtual string ScopeFieldName
		{
			get { return _scopeFieldName; }
		}

		public virtual string ScopeValueSourceFieldName
		{
			get { return _scopeDataSourceFieldName; }
		}

		public virtual bool DisplayNotes { get; }

		public virtual string NotesFilter { get; }

		public virtual bool StoreContentField
		{
			get { return true; }
		}

		public virtual string TitleFieldName
		{
			get { return _titleFieldName; }
		}

		/// <summary>
		/// Gets the web role field name.
		/// </summary>
		public string WebRoleFieldName
		{
			get { return _webRoleFieldName; }
		}

		public string WebRoleDefaultValue
		{
			get
			{
				return "F1158253-71CB-4063-BBC5-B3CFE27CA3EB";
			}
		}

		public Version Version { get; private set; }

		protected virtual string IndexQueryName { get; private set; }

		public virtual IEnumerable<ICrmEntityIndexer> GetIndexers()
		{
			return new ICrmEntityIndexer[]
			{
				new SavedQueryIndexer(this, IndexQueryName),
				new UserQueryIndexer(this, IndexQueryName),
			};
		}

		public virtual IEnumerable<ICrmEntityIndexer> GetIndexers(string entityLogicalName)
		{
			return new ICrmEntityIndexer[]
			{
				new EntitySetSavedQueryIndexer(this, IndexQueryName, entityLogicalName),
				new EntitySetUserQueryIndexer(this, IndexQueryName, entityLogicalName),
			};
		}

        public virtual IEnumerable<ICrmEntityIndexer> GetIndexers(string entityLogicalName, IEnumerable<Fetch.Filter> filters = null, IEnumerable<Fetch.Link> links = null)
        {
            return new ICrmEntityIndexer[]
            {
                new EntitySetSavedQueryIndexer(this, IndexQueryName, entityLogicalName, filters, links),
                new EntitySetUserQueryIndexer(this, IndexQueryName, entityLogicalName, filters, links),
            };
        }

        public virtual IEnumerable<ICrmEntityIndexer> GetIndexers(string entityLogicalName, Guid id)
		{
			return new ICrmEntityIndexer[]
			{
				new SingleEntitySavedQueryIndexer(this, IndexQueryName, entityLogicalName, id),
				new SingleEntityUserQueryIndexer(this, IndexQueryName, entityLogicalName, id),
			};
		}

		public virtual ICrmEntitySearchResultFactory GetSearchResultFactory(Query query)
		{
			return new CrmEntitySearchResultFactory(this, new SimpleHtmlHighlightedFragmentProvider(this, query));
		}

		private Analyzer GetAnalyzer()
		{
			var analyzer = new PerFieldAnalyzerWrapper(_defaultAnalyzer);

			analyzer.AddAnalyzer(LogicalNameFieldName, new KeywordAnalyzer());
			analyzer.AddAnalyzer(PrimaryKeyFieldName, new KeywordAnalyzer());
			analyzer.AddAnalyzer(PrimaryKeyLogicalNameFieldName, new KeywordAnalyzer());

			return analyzer;
		}
	}
}
