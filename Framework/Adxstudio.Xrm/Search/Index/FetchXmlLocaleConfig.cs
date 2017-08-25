/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Index
{
	/// <summary>
	/// Used to propagate locale schema information about currently indexed entity
	/// </summary>
	internal class FetchXmlLocaleConfig
	{
		/// <summary>
		/// Stores field name for language code
		/// </summary>
		private readonly string localeCodeLogicalName;

		/// <summary>
		/// Stores field name for language LCID
		/// </summary>
		private readonly string localeLCIDLogicalName;

		/// <summary>
		/// Stores legacy (i.e. KB article) flag
		/// </summary>
		private readonly bool isKnowledgeArticle;

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchXmlLocaleConfig" /> class.
		/// </summary>
		/// <param name="localeCodeLogicalName">Fetch XML field name for language code</param>
		/// <param name="localeLCIDLogicalName">Fetch XML field name for language LCID</param>
		/// <param name="isKnowledgeArticle">Is this a knowledge article?</param>
		private FetchXmlLocaleConfig(string localeCodeLogicalName, string localeLCIDLogicalName, bool isKnowledgeArticle)
		{
			this.localeCodeLogicalName = localeCodeLogicalName;
			this.localeLCIDLogicalName = localeLCIDLogicalName;
			this.isKnowledgeArticle = isKnowledgeArticle;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="FetchXmlLocaleConfig" /> class for Knowledge articles.
		/// </summary>
		/// <returns>A new instance of the <see cref="FetchXmlLocaleConfig" /> class</returns>
		public static FetchXmlLocaleConfig CreateKnowledgeArticleConfig()
		{
			return new FetchXmlLocaleConfig("language_localeid.code", null, true);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="FetchXmlLocaleConfig" /> class for other entities.
		/// </summary>
		/// <returns>A new instance of the <see cref="FetchXmlLocaleConfig" /> class</returns>
		public static FetchXmlLocaleConfig CreatePortalLanguageConfig()
		{
			return new FetchXmlLocaleConfig("portallang.adx_languagecode", "portallang.adx_lcid", false);
		}

		/// <summary>
		/// Determines if currently processed field represents language code
		/// </summary>
		/// <param name="fieldName">Processed field name</param>
		/// <returns>True if currently processed field represents language code</returns>
		public bool IsLanguageCodeLogicalName(string fieldName)
		{
			return !string.IsNullOrEmpty(this.localeCodeLogicalName) && this.localeCodeLogicalName == fieldName;
		}

		/// <summary>
		/// Determines if currently processed field represents language LCID
		/// </summary>
		/// <param name="fieldName">Processed field name</param>
		/// <returns>True if currently processed field represents language LCID</returns>
		public bool IsLCIDLogicalName(string fieldName)
		{
			return !string.IsNullOrEmpty(this.localeLCIDLogicalName) && this.localeLCIDLogicalName == fieldName;
		}

		/// <summary>
		/// Determines if parsing field metadata should be skipped
		/// </summary>
		/// <param name="fieldName">Processed field name</param>
		/// <returns>True if parsing field metadata should be skipped</returns>
		public bool CanSkipMetadata(string fieldName)
		{
			return (this.IsLanguageCodeLogicalName(fieldName) || this.IsLCIDLogicalName(fieldName)) && !this.isKnowledgeArticle;
		}
	}
}
