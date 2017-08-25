/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search.Index
{
	internal class FetchXmlLinkAttribute
	{
		public FetchXmlLinkAttribute(string logicalName, string entityLogicalName)
		{
			if (string.IsNullOrEmpty(logicalName))
			{
				throw new ArgumentNullException("logicalName");
			}

			if (string.IsNullOrEmpty(entityLogicalName))
			{
				throw new ArgumentNullException("entityLogicalName");
			}

			LogicalName = logicalName;
			EntityLogicalName = entityLogicalName;
		}

		public string EntityLogicalName { get; private set; }

		public string LogicalName { get; private set; }
	}
}
