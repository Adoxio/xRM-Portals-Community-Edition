/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Documents;

namespace Adxstudio.Xrm.Search
{
	public class EmptyFragmentProvider : ICrmEntitySearchResultFragmentProvider
	{
		public string GetFragment(Document document)
		{
			return string.Empty;
		}
	}
}
