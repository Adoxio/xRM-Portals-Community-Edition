/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Search.Analysis
{
	using System.Collections.Generic;
	using System.Linq;
	using Lucene.Net.Analysis;
	using Lucene.Net.Analysis.Snowball;
	using Lucene.Net.Util;
	using System.Globalization;
	using Lucene.Net.Analysis.Standard;

	public class SnowballAnalyzerFactory : IAnalyzerFactory
	{
		private readonly string _stemmer;
		private readonly ISet<string> _stopWords;

		public SnowballAnalyzerFactory(string stemmer) : this(stemmer, new string[] { }) { }

		public SnowballAnalyzerFactory(string stemmer, IEnumerable<string> stopWords)
		{
			_stemmer = stemmer ?? CultureInfo.CurrentUICulture.Parent.EnglishName;
			_stopWords = new HashSet<string>(stopWords);
		}

		protected internal SnowballAnalyzerFactory() : this(null, new string[] { }) { }

		public virtual Analyzer GetAnalyzer(Version version)
		{
			if (string.IsNullOrEmpty(this._stemmer))
			{
				return null;
			}
			if (!AnalyzerConstants.SupportedStemmer.Contains(this._stemmer))
			{
				return null;
			}

			if (_stopWords.Any())
			{
				return new SnowballAnalyzer(version, _stemmer, _stopWords);
			}

			ISet<string> defaultStopWords;

			if (AnalyzerConstants.DefaultStopWords.TryGetValue(_stemmer, out defaultStopWords))
			{
				return new SnowballAnalyzer(version, _stemmer, defaultStopWords);
			}

			return new SnowballAnalyzer(version, _stemmer);
		}
	}
}
