/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	internal sealed class TimeBasedChangedData
	{
		/// <summary>
		/// List of updated records
		/// </summary>
		internal List<IChangedItem> UpdatedEntityRecords { get; set; }

		/// <summary>
		/// Dictionyry with entity name and last updated timestamp
		/// </summary>
		internal Dictionary<string, string> UpdatedEntitiesWithLastTimestamp { get; set; }
	}
}
