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
	using Version = Lucene.Net.Util.Version;
	using System.Globalization;

	public abstract class SettingSnowballAnalyzerFactory : SnowballAnalyzerFactory
	{
		public const string StemmerSettingName = "Adxstudio.Xrm.Search.Analysis.Stemmer";
		public const string StopWordsSettingName = "Adxstudio.Xrm.Search.Analysis.StopWords";

		public override Analyzer GetAnalyzer(Version version)
		{
			string stemmer;

			if (!TryGetSettingValue(StemmerSettingName, out stemmer))
			{
				stemmer = CultureInfo.CurrentUICulture.Parent.EnglishName;
			}

			ISet<string> stopWords;

			return TryGetStopWords(stemmer, out stopWords)
				? new SnowballAnalyzer(version, stemmer, stopWords)
				: new SnowballAnalyzer(version, stemmer);
		}

		protected abstract bool TryGetSettingValue(string name, out string value);

		private bool TryGetStopWords(string stemmer, out ISet<string> stopWords)
		{
			string stopWordsSetting;

			if (!TryGetSettingValue(StopWordsSettingName, out stopWordsSetting))
			{
				ISet<string> defaultStopWords;

				if (AnalyzerConstants.DefaultStopWords.TryGetValue(stemmer, out defaultStopWords))
				{
					stopWords = defaultStopWords;

					return true;
				}

				stopWords = null;

				return false;
			}

			stopWords = new HashSet<string>(
				stopWordsSetting
					.Split(',')
					.Select(word => word.Trim())
					.Where(word => !string.IsNullOrEmpty(word)));

			return true;
		}
	}
}
