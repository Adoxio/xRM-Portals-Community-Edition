/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Search.WindowsAzure
{
	[DataContract]
	public class EntitySearchResultPage
	{
		public EntitySearchResultPage(IEnumerable<EntitySearchResult> results, int approximateTotalHits, int pageNumber, int pageSize)
		{
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}

			Results = results.ToArray();

			ApproximateTotalHits = approximateTotalHits;
			PageNumber = pageNumber;
			PageSize = pageSize;
		}

		[DataMember]
		public int ApproximateTotalHits { get; set; }

		[DataMember]
		public bool IndexNotFound { get; set; }

		[DataMember]
		public int PageNumber { get; set; }

		[DataMember]
		public int PageSize { get; set; }

		[DataMember]
		public EntitySearchResult[] Results { get; set; }
	}
}
