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
	public class EntityIndexInfo
	{
		public EntityIndexInfo(IEnumerable<string> logicalNames)
		{
			if (logicalNames == null)
			{
				throw new ArgumentNullException("logicalNames");
			}

			LogicalNames = logicalNames.ToArray();
		}

		public EntityIndexInfo() : this(new List<string>()) { }

		[DataMember]
		public bool IndexNotFound { get; set; }

		[DataMember]
		public string[] LogicalNames { get; set; }
	}
}
