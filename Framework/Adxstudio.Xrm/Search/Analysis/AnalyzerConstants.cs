/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyzerConstants.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Analysis
{
	using System.Collections.Generic;

	/// <summary>
	/// Analyzer Constants
	/// </summary>
	public static class AnalyzerConstants
	{
		/// <summary>
		/// Analyzer Default Stop Words
		/// </summary>
		internal static readonly IDictionary<string, ISet<string>> DefaultStopWords = new Dictionary<string, ISet<string>>
		{
			{ "English", new HashSet<string> { "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with" } },
		};

		/// <summary>
		/// Lucene Supported Stemmer
		/// </summary>
		internal static readonly HashSet<string> SupportedStemmer = new HashSet<string>
		{
			"Danish", "Dutch", "English", "Finnish", "French", "German", "Hungarian", "Italian", "Norwegian", "Portuguese", "Romanian", "Russian", "Spanish", "Swedish"
		};
	}
}
