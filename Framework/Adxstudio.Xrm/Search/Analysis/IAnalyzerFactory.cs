/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace Adxstudio.Xrm.Search.Analysis
{
	public interface IAnalyzerFactory
	{
		Analyzer GetAnalyzer(Version version);
	}
}
