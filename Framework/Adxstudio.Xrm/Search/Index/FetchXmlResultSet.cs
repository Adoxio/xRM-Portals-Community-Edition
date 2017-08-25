/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Adxstudio.Xrm.Search.Index
{
	internal class FetchXmlResultSet : IEnumerable<FetchXmlResult>
	{
		private readonly IEnumerable<FetchXmlResult> _results;
		private readonly XDocument _xml;

		public FetchXmlResultSet(string fetchXmlResultSet)
		{
			_xml = XDocument.Parse(fetchXmlResultSet);

			string moreRecords;

			if (TryGetFirstAttribute(_xml, "//resultset", "morerecords", out moreRecords))
			{
				int intValue;

				if (int.TryParse(moreRecords, out intValue))
				{
					MoreRecords = intValue == 1;
				}

				bool boolValue;

				if (bool.TryParse(moreRecords, out boolValue))
				{
					MoreRecords = boolValue;
				}
			}

			string pagingCookie;

			PagingCookie = TryGetFirstAttribute(_xml, "//resultset", "paging-cookie", out pagingCookie)
				? pagingCookie
				: null;

			_results = _xml.XPathSelectElements("//resultset/result").Select(e => new FetchXmlResult(e)).ToList();
		}

		public bool MoreRecords { get; private set; }

		public string PagingCookie { get; private set; }

		public IEnumerator<FetchXmlResult> GetEnumerator()
		{
			return _results.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return _xml.ToString();
		}

		private static bool TryGetFirstAttribute(XNode xml, string xpath, XName attributeName, out string attributeValue)
		{
			attributeValue = null;

			var element = xml.XPathSelectElements(xpath).FirstOrDefault();

			if (element == null)
			{
				return false;
			}

			var attribute = element.Attribute(attributeName);

			if (attribute == null)
			{
				return false;
			}

			attributeValue = attribute.Value;

			return !string.IsNullOrEmpty(attributeValue);
		}
	}
}
