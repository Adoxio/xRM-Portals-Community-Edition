/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Data
{
	internal static class XContainerExtensions
	{
		public static void AddFetchXmlFilterCondition(this XContainer filter, string attribute, string operatorName, string value)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}

			var condition = new XElement("condition");

			condition.SetAttributeValue("attribute", attribute);
			condition.SetAttributeValue("operator", operatorName);
			condition.SetAttributeValue("value", value);

			filter.Add(condition);
		}

		public static void AddFetchXmlFilterInCondition(this XContainer filter, string attribute, IEnumerable<string> values)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
		
			if (!values.Any())
			{
				throw new ArgumentException("Value can't be null or empty.", "values");
			}

			var valueArray = values as string[] ?? values.ToArray();

			var condition = new XElement("condition");

			condition.SetAttributeValue("attribute", attribute);
			condition.SetAttributeValue("operator", "in");

			foreach (var value in valueArray)
			{
				var valueElement = new XElement("value");

				valueElement.SetValue(value);

				condition.Add(valueElement);
			}

			filter.Add(condition);
		}
	}
}
