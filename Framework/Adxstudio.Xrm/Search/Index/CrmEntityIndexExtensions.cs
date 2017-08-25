/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Index
{
	using System.Linq;
	using System.Web;
	using Adxstudio.Xrm.Search.Analysis;
	using Lucene.Net.Analysis;
	using Lucene.Net.Util;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Cms;

	/// <summary>
	/// Contains useful extension methods for working with entity indexes
	/// </summary>
	public static class CrmEntityIndexExtensions
	{
		/// <summary>
		/// Gets a language-specific Lucene <see cref="Analyzer"/> for the current user context
		/// </summary>
		/// <param name="index">Current search index</param>
		/// <param name="isMultiLanguageEnabled">flag for multilanguage enabled portal</param>
		/// <param name="contextLanguage">website language</param>
		/// <returns>Language-specific analyzer if Multi Language is enabled. Default index analyzer otherwise</returns>
		public static Analyzer GetQuerySpecificAnalyzer(this ICrmEntityIndex index, bool isMultiLanguageEnabled, IWebsiteLanguage contextLanguage)
		{
			if (!isMultiLanguageEnabled || contextLanguage == null)
			{
				return index.Analyzer;
			}

			return CreateSpecificAnalyzer(contextLanguage.Lcid, index.Version);
		}

		/// <summary>
		/// Gets a language-specific Lucene <see cref="Analyzer"/> for the given language
		/// </summary>
		/// <param name="index">Current search index</param>
		/// <param name="languageId">Language ID (LCID) of the needed language</param>
		/// <returns>Language-specific analyzer provided by <see cref="LanguageAnalyzerFactory"/></returns>
		public static Analyzer GetLanguageSpecificAnalyzer(this ICrmEntityIndex index, int languageId)
		{
			return CreateSpecificAnalyzer(languageId, index.Version);
		}

		/// <summary>
		/// Gets a language-specific Lucene <see cref="Analyzer"/> for the given language
		/// </summary>
		/// <param name="languageId">Language ID (LCID) of the needed language</param>
		/// <param name="version">Lucene version</param>
		/// <returns>Language-specific analyzer via <see cref="LanguageAnalyzerFactory"/> with default stop-words</returns>
		private static Analyzer CreateSpecificAnalyzer(int languageId, Version version)
		{
			var analyzerFactory = new LanguageAnalyzerFactory(Enumerable.Empty<string>());
			return analyzerFactory.GetAnalyzer(languageId, version);
		}
	}
}
