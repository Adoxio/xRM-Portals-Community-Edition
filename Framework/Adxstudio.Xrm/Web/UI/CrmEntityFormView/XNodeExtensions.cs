/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Adxstudio.Xrm.AspNet.Cms;

	internal static class XNodeExtensions
	{
		public static bool TryGetAttributeValue(this XNode startingNode, string elementXPath, string attributeName, out string attributeValue)
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

		public static bool TryGetBooleanAttributeValue(this XNode startingNode, string elementXPath, string attributeName, out bool attributeValue)
		{
			attributeValue = false;

			string textValue;

			return TryGetAttributeValue(startingNode, elementXPath, attributeName, out textValue) && bool.TryParse(textValue, out attributeValue);
		}

		public static bool TryGetIntegerAttributeValue(this XNode startingNode, string elementXPath, string attributeName, out int? attributeValue)
		{
			attributeValue = null;

			string textValue;

			if (!TryGetAttributeValue(startingNode, elementXPath, attributeName, out textValue))
			{
				return false;
			}

			int integerValue;

			if (!int.TryParse(textValue, out integerValue))
			{
				return false;
			}

			attributeValue = integerValue;

			return true;
		}

		public static bool TryGetElementValue(this XNode startingNode, string elementXPath, out string elementValue)
		{
			elementValue = null;

			var element = startingNode.XPathSelectElement(elementXPath);

			if (element == null)
			{
				return false;
			}

			elementValue = element.Value;

			return true;
		}

		public static bool TryGetBooleanElementValue(this XNode startingNode, string elementXPath, out bool elementValue)
		{
			elementValue = false;

			string textValue;

			return startingNode.TryGetElementValue(elementXPath, out textValue) && bool.TryParse(textValue, out elementValue);
		}

		public static bool TryGetIntegerElementValue(this XNode startingNode, string elementXPath, out int? elementValue)
		{
			elementValue = null;

			string textValue;

			if (!startingNode.TryGetElementValue(elementXPath, out textValue))
			{
				return false;
			}
			
			int integerValue;

			if (!int.TryParse(textValue, out integerValue))
			{
				return false;
			}
			
			elementValue = integerValue;
			return true;
		}

		/// <summary> The try get language specific attribute value. </summary>
		/// <param name="startingNode"> The starting node. </param>
		/// <param name="languageCode"> The language code. </param>
		/// <param name="elementValue"> The element value. </param>
		/// <param name="baseOrgLanguageCode"> The base org language code. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		public static bool TryGetLanguageSpecificLabelValue(this XNode startingNode, int languageCode, out string elementValue, int baseOrgLanguageCode = 0)
		{
			var lcid = ContextLanguageInfo.ResolveCultureLcid(languageCode);
			if (TryGetAttributeValue(startingNode, string.Format("labels/label[@languagecode='{0}']", lcid), "description", out elementValue))
			{
				return true;
			}

			return TryGetAttributeValue(startingNode, string.Format("labels/label[@languagecode='{0}']", baseOrgLanguageCode), "description", out elementValue);
		}
	}
}
