/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;

namespace Adxstudio.Xrm.Search.Index
{
	public class CrmEntityIndexDocument
	{
		public CrmEntityIndexDocument(Document document, Analyzer analyzer, Guid primaryKey)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}

			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}

			Document = document;
			Analyzer = analyzer;
            PrimaryKey = primaryKey;
		}

		public Analyzer Analyzer { get; private set; }

		public Document Document { get; private set; }

        public Guid PrimaryKey { get; private set; }
	}
}
