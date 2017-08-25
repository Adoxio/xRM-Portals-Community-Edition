/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Search.Index
{
	internal class SavedQuery
	{
		private readonly FetchXml _fetchxml;
		private readonly XDocument _layoutxml;

		public SavedQuery(Entity savedQuery)
		{
			if (savedQuery == null)
			{
				throw new ArgumentNullException("savedQuery");
			}

			_fetchxml = new FetchXml(XDocument.Parse(savedQuery.GetAttributeValue<string>("fetchxml")));
			_layoutxml = XDocument.Parse(savedQuery.GetAttributeValue<string>("layoutxml"));

			string titleAttributeLogicalName;

			if (TryGetFirstAttribute(_layoutxml, "//row/cell", "name", out titleAttributeLogicalName))
			{
				TitleAttributeLogicalName = titleAttributeLogicalName;
			}
			else
			{
				throw new InvalidOperationException("Unable to extract title attribute logical name from layoutxml.");
			}
		}

		public FetchXml FetchXml
		{
			get { return _fetchxml; }
		}

		public string LogicalName
		{
			get { return FetchXml.LogicalName; }
		}

		public string TitleAttributeLogicalName { get; private set; }

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
