/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LanguageAnalyzerFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Analysis
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Globalization;
	using Lucene.Net.Analysis;
	using Lucene.Net.Analysis.Snowball;
	using Lucene.Net.Analysis.Standard;
	using Version = Lucene.Net.Util.Version;

	/// <summary>
	/// Language Analyzer Factory
	/// </summary>
	/// <seealso cref="Adxstudio.Xrm.Search.Analysis.ILanguageAnalyzerFactory" />
	public class LanguageAnalyzerFactory : ILanguageAnalyzerFactory
	{
		/// <summary>
		/// Analyzer Stop Words
		/// </summary>
		private readonly ISet<string> stopWords;

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageAnalyzerFactory"/> class.
		/// </summary>
		/// <param name="stopWords">Analyzer Stop Words</param>
		public LanguageAnalyzerFactory(IEnumerable<string> stopWords)
		{
			this.stopWords = new HashSet<string>(stopWords);
		}

		/// <summary>
		/// Get Analyzer
		/// </summary>
		/// <param name="lcid">Language code ID</param>
		/// <param name="version">Lucene Version</param>
		/// <returns>Lucene Analyzer</returns>
		public Analyzer GetAnalyzer(int lcid, Version version)
		{
			CultureInfo culture;
			try
			{
				culture = new CultureInfo(lcid);
			}
			catch (CultureNotFoundException)
			{
				return new StandardAnalyzer(version);
			}

			var stemmer = culture.Parent.EnglishName;
			if (!AnalyzerConstants.SupportedStemmer.Contains(stemmer))
			{
				return new StandardAnalyzer(version);
			}

			if (this.stopWords.Any())
			{
				return new SnowballAnalyzer(version, stemmer, this.stopWords);
			}

			ISet<string> defaultStopWords;

			if (AnalyzerConstants.DefaultStopWords.TryGetValue(stemmer, out defaultStopWords))
			{
				return new SnowballAnalyzer(version, stemmer, defaultStopWords);
			}

			return new SnowballAnalyzer(version, stemmer);
		}
	}
}
