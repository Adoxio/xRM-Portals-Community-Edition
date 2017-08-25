/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Search
{
	[DataContract]
	public class CrmEntityIndexInfo
	{
		public CrmEntityIndexInfo(string logicalName, string displayName, string displayCollectionName)
		{
			if (string.IsNullOrEmpty(logicalName))
			{
				throw new ArgumentNullException("logicalName");
			}

			LogicalName = logicalName;
			DisplayName = displayName;
			DisplayCollectionName = displayCollectionName;
		}

		[DataMember]
		public string DisplayCollectionName { get; private set; }

		[DataMember]
		public string DisplayName { get; private set; }

		[DataMember]
		public string LogicalName { get; private set; }
	}
}
