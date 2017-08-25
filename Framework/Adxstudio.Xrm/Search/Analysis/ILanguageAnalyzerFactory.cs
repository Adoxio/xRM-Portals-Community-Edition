/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILanguageAnalyzerFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Analysis
{
	using Lucene.Net.Analysis;
	using Lucene.Net.Util;

	/// <summary>
	/// Language Analyzer Factory
	/// </summary>
	public interface ILanguageAnalyzerFactory
	{
		/// <summary>
		/// Get Analyzer
		/// </summary>
		/// <param name="lcid">Language code ID</param>
		/// <param name="version">Lucene Version</param>
		/// <returns>Lucene Analyzer</returns>
		Analyzer GetAnalyzer(int lcid, Version version);
	}
}
