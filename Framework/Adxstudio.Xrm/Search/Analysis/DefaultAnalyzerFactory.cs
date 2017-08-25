/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Version=Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Analysis
{
	public class DefaultAnalyzerFactory : IAnalyzerFactory
	{
		public Analyzer GetAnalyzer(Version version)
		{
			return new StandardAnalyzer(version);
		}
	}
}
