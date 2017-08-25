/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Search.Index
{
	public interface ICrmEntityIndexer
	{
		IEnumerable<CrmEntityIndexDocument> GetDocuments();

		bool Indexes(string entityLogicalName);
	}
}
