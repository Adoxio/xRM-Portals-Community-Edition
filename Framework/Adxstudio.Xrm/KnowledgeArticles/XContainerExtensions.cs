/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Xml.Linq;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	internal static class XContainerExtensions
	{
		public static void AddFetchXmlFilterCondition(this XContainer filter, string attribute, string operatorName,
			string value)
		{
			filter.ThrowOnNull("filter");

			var condition = new XElement("condition");

			condition.SetAttributeValue("attribute", attribute);
			condition.SetAttributeValue("operator", operatorName);
			condition.SetAttributeValue("value", value);

			filter.Add(condition);
		}

		public static void AddFetchXmlFilterCondition(this XContainer filter, string attribute, string operatorName)
		{
			filter.ThrowOnNull("filter");

			var condition = new XElement("condition");

			condition.SetAttributeValue("attribute", attribute);
			condition.SetAttributeValue("operator", operatorName);

			filter.Add(condition);
		}

		public static void AddFetchXmlLinkEntity(
			this XContainer entity,
			string linkEntityLogicalName,
			string linkFromAttributeLogicalName,
			string linkToAttributeLogicalName,
			Action<Action<string, string, string>> addFilterConditions,
			Action<Action<string, string, string, Action<Action<string, string, string>>>> addNestedLinkEntities = null)
		{
			entity.ThrowOnNull("entity");

			var linkEntity = new XElement("link-entity");

			linkEntity.SetAttributeValue("name", linkEntityLogicalName);
			linkEntity.SetAttributeValue("from", linkFromAttributeLogicalName);
			linkEntity.SetAttributeValue("to", linkToAttributeLogicalName);

			var filter = new XElement("filter");

			filter.SetAttributeValue("type", "and");

			addFilterConditions(filter.AddFetchXmlFilterCondition);

			if (addNestedLinkEntities != null)
			{
				addNestedLinkEntities(linkEntity.AddFetchXmlLinkEntity);
			}

			linkEntity.Add(filter);

			entity.Add(linkEntity);
		}

		public static void AddFetchXmlLinkEntity(
			this XContainer entity,
			string linkEntityLogicalName,
			string linkFromAttributeLogicalName,
			string linkToAttributeLogicalName,
			Action<Action<string, string, string>> addFilterConditions)
		{
			entity.ThrowOnNull("entity");

			var linkEntity = new XElement("link-entity");

			linkEntity.SetAttributeValue("name", linkEntityLogicalName);
			linkEntity.SetAttributeValue("from", linkFromAttributeLogicalName);
			linkEntity.SetAttributeValue("to", linkToAttributeLogicalName);

			var filter = new XElement("filter");

			filter.SetAttributeValue("type", "and");

			addFilterConditions(filter.AddFetchXmlFilterCondition);

			linkEntity.Add(filter);

			entity.Add(linkEntity);
		}
	}
}
