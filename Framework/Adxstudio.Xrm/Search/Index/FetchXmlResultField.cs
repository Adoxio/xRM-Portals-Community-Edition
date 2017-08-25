/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Adxstudio.Xrm.Search.Index
{
	internal class FetchXmlResultField
	{
		public FetchXmlResultField(XElement fetchXmlResultField)
		{
			if (fetchXmlResultField == null)
			{
				throw new ArgumentNullException("fetchXmlResultField");
			}

			Attributes = fetchXmlResultField.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);
			Name = fetchXmlResultField.Name.LocalName;
			Value = fetchXmlResultField.Value;
		}

		public IDictionary<string, string> Attributes { get; private set; }

		public string Name { get; private set; }

		public string Value { get; set; }
	}
}
