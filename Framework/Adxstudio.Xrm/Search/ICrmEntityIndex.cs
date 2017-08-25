/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Fetch = Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Search.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Xrm.Sdk.Client;
using Version = Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search
{
	public interface ICrmEntityIndex
	{
		bool AddScopeField { get; }

		Analyzer Analyzer { get; }

		string ContentFieldName { get; }

		OrganizationServiceContext DataContext { get; }

		Directory Directory { get; }

		string IsUrlDefinedFieldName { get; }

		[Obsolete("Use the new Multi Language implementation instead", false)]
		string LanguageLocaleCode { get; }

		string LanguageLocaleCodeDefaultValue { get; }

		string LanguageLocaleCodeFieldName { get; }

		string LanguageLocaleLCIDFieldName { get; }

		string LogicalNameFieldName { get; }

		string Name { get; }

		string PrimaryKeyFieldName { get; }

		string PrimaryKeyLogicalNameFieldName { get; }

		string ProductAccessDefaultValue { get; }

		string ProductAccessNonKnowledgeArticleDefaultValue { get; }

		string ScopeDefaultValue { get; }

		string ScopeFieldName { get; }

		string ScopeValueSourceFieldName { get; }

		/// <summary>
		/// Gets site setting value for KnowledgeManagement/DisplayNotes
		/// </summary>
		bool DisplayNotes { get; }

		/// <summary>
		/// Gets site setting value for KnowledgeManagement/NotesFilter
		/// </summary>
		string NotesFilter { get; }

		bool StoreContentField { get; }

		string TitleFieldName { get; }

		/// <summary>
		/// Gets the web role field name.
		/// </summary>
		string WebRoleFieldName { get; }

		/// <summary>
		/// Returns "F1158253-71CB-4063-BBC5-B3CFE27CA3EB"
		/// </summary>
		string WebRoleDefaultValue { get; }

		Version Version { get; }

		IEnumerable<ICrmEntityIndexer> GetIndexers();

		IEnumerable<ICrmEntityIndexer> GetIndexers(string entityLogicalName, IEnumerable<Fetch.Filter> filters = null, IEnumerable<Fetch.Link> links = null);

		IEnumerable<ICrmEntityIndexer> GetIndexers(string entityLogicalName, Guid id);

		ICrmEntitySearchResultFactory GetSearchResultFactory(Query query);
	}
}
