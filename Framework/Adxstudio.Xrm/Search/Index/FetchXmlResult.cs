/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Adxstudio.Xrm.Search.Index
{
	internal class FetchXmlResult : IEnumerable<FetchXmlResultField>
	{
		private readonly IEnumerable<FetchXmlResultField> _fields;
		private readonly XElement _xml;

		public FetchXmlResult(XElement fetchXmlResult)
		{
			if (fetchXmlResult == null)
			{
				throw new ArgumentNullException("fetchXmlResult");
			}

			_xml = fetchXmlResult;

			_fields = _xml.Descendants().Select(e => new FetchXmlResultField(e)).ToList();
		}

		public IEnumerator<FetchXmlResultField> GetEnumerator()
		{
			return _fields.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return _xml.ToString();
		}
	}
}
