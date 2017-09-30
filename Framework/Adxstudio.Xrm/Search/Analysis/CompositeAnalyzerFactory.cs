/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Version=Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Analysis
{
	public class CompositeAnalyzerFactory : IAnalyzerFactory
	{
		private readonly IEnumerable<IAnalyzerFactory> _factories;

		public CompositeAnalyzerFactory(IEnumerable<IAnalyzerFactory> factories)
		{
			if (factories == null)
			{
				throw new ArgumentNullException("factories");
			}

			_factories = factories;
		}

		public CompositeAnalyzerFactory(params IAnalyzerFactory[] factories) : this(factories as IEnumerable<IAnalyzerFactory>) { }

		public Analyzer GetAnalyzer(Version version)
		{
			foreach (var factory in _factories)
			{
				var analyzer = factory.GetAnalyzer(version);

				if (analyzer != null)
				{
					return analyzer;
				}
			}

			return null;
		}
	}
}
