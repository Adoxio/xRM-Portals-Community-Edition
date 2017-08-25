/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using System.Xml.XPath;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	internal static class XNodeUtility
	{
		public static bool TryGetAttributeValue(XNode startingNode, string elementXPath, string attributeName, out string attributeValue)
		{
			attributeValue = null;

			var element = startingNode.XPathSelectElement(elementXPath);

			if (element == null)
			{
				return false;
			}

			var attribute = element.Attribute(XName.Get(attributeName));

			if (attribute == null)
			{
				return false;
			}

			attributeValue = attribute.Value;

			return true;
		}

		public static bool TryGetBooleanAttributeValue(XNode startingNode, string elementXPath, string attributeName, out bool attributeValue)
		{
			attributeValue = false;

			string textValue;

			return TryGetAttributeValue(startingNode, elementXPath, attributeName, out textValue) && bool.TryParse(textValue, out attributeValue);
		}
	}
}
