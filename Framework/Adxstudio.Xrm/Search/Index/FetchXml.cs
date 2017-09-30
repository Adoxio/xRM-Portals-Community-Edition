/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Search.Index
{
	internal class FetchXml
	{
		private readonly XNode _xml;

		public FetchXml(XNode xml)
		{
			if (xml == null)
			{
				throw new ArgumentNullException("xml");
			}

			_xml = xml;

			string logicalName;

			if (TryGetFirstAttribute(_xml, "//fetch/entity[@name!='']", "name", out logicalName))
			{
				LogicalName = logicalName;
			}
			else
			{
				throw new InvalidOperationException("Unable to extract entity logical name from fetchxml.");
			}

			string pageAttributeValue;
			int page;

			if (TryGetFirstAttribute(_xml, "//fetch", "page", out pageAttributeValue) && int.TryParse(pageAttributeValue, out page))
			{
				Page = page;
			}
			else
			{
				Page = 1;
			}
		}

		public string LogicalName { get; private set; }

		public int Page { get; private set; }

		public void AddAttribute(string attributeLogicalName)
		{
			foreach (var element in _xml.XPathSelectElements("//fetch/entity"))
			{
				element.Add(new XElement("attribute", new XAttribute("name", attributeLogicalName)));
			}
		}

		public bool ContainsAttribute(string attributeLogicalName)
		{
			return this._xml.XPathSelectElements("//fetch/entity/attribute").Any(x => x.GetAttributeValue("name") == attributeLogicalName);
		}

		/// <summary>
		/// Will add a Filter attribute to the fetchXML. If a linkEntityAlias is given then it will try to find the link and add the condition there.
		/// </summary>
		/// <param name="type">
		/// The type of filter.
		/// </param>
		/// <param name="attributeLogicalName">
		/// The attribute logical name.
		/// </param>
		/// <param name="op">
		/// The operator of the condition.
		/// </param>
		/// <param name="value">
		/// The value for the condition.
		/// </param>
		/// <param name="linkEntityAlias">
		/// The link entity alias.
		/// </param>
		public void AddConditionalStatement(string type, string attributeLogicalName, string op, string value = null, string linkEntityAlias = null)
		{
			var filter = new XElement("filter");
			filter.SetAttributeValue("type", type);
			var condition = new XElement("condition");
			condition.SetAttributeValue("attribute", attributeLogicalName);
			condition.SetAttributeValue("operator", op);
			if (value != null)
			{
				condition.SetAttributeValue("value", value);
			}
			filter.Add(condition);

			if (linkEntityAlias == null)
			{
				foreach (var element in _xml.XPathSelectElements("//fetch/entity"))
				{

					element.Add(filter);
				}
			}
			else
			{
				var entity = _xml
				.XPathSelectElements("//fetch/entity")
				.FirstOrDefault(e => e.Attributes("name").Any(a => a.Value == LogicalName));

				if (entity == null)
				{
					return;
				}

				var linkEntity = entity
					.XPathSelectElements("link-entity")
					.FirstOrDefault(e => e.Attributes("alias").Any(a => a.Value == linkEntityAlias));

				if (linkEntity == null)
				{
					return;
				}

				linkEntity.Add(filter);
			}

		}

		public void AddLinkEntity(string name, string fromAttribute, string toAttribute, string alias, string type,
			bool visible = false, bool intersect = false)
		{
			foreach (var element in _xml.XPathSelectElements("//fetch/entity"))
			{
				var link = new XElement("link-entity");
				link.SetAttributeValue("name", name);
				link.SetAttributeValue("from", fromAttribute);
				link.SetAttributeValue("to", toAttribute);
				link.SetAttributeValue("alias", alias);
				link.SetAttributeValue("link-type", type);
				link.SetAttributeValue("visible", visible);
				link.SetAttributeValue("intersect", intersect);
				element.Add(link);
			}
		}

		public void AddLinkEntityAttribute(string alias, string attributeLogicalName)
		{
			var entity = _xml
				.XPathSelectElements("//fetch/entity")
				.FirstOrDefault(e => e.Attributes("name").Any(a => a.Value == LogicalName));

			if (entity == null)
			{
				return;
			}

			var linkEntity = entity
				.XPathSelectElements("link-entity")
				.FirstOrDefault(e => e.Attributes("alias").Any(a => a.Value == alias));

			if (linkEntity == null)
			{
				return;
			}

			linkEntity.Add(new XElement("attribute", new XAttribute("name", attributeLogicalName)));
		}

		public bool ContainsLinkEntity(string alias)
		{
			var entity = _xml
			.XPathSelectElements("//fetch/entity")
			.FirstOrDefault(e => e.Attributes("name").Any(a => a.Value == LogicalName));

			if (entity == null)
			{
				return false;
			}

			var linkEntity = entity
				.XPathSelectElements("link-entity")
				.FirstOrDefault(e => e.Attributes("alias").Any(a => a.Value == alias));

			return !(linkEntity == null);
		}

		public void AddLinkEntity(XElement link)
		{
			AddFetchElement(link);
		}

		public void AddFilter(XElement filter)
		{
			AddFetchElement(filter);
		}

		public FetchXml ForNextPage()
		{
			return ForNextPage(null);
		}

		public FetchXml ForNextPage(string pagingCookie)
		{
			var xml = XDocument.Parse(ToString());

			var fetch = xml.XPathSelectElement("//fetch");

			fetch.SetAttributeValue("page", Page + 1);
			fetch.SetAttributeValue("paging-cookie", pagingCookie);

			return new FetchXml(xml);
		}

		public override string ToString()
		{
			return _xml.ToString();
		}

		public bool TryGetLinkAttribute(FetchXmlResultField field, out FetchXmlLinkAttribute linkAttribute)
		{
			linkAttribute = null;

			var nameParts = field.Name.Split('.');

			if (nameParts.Length != 2)
			{
				return false;
			}

			var entity = _xml.XPathSelectElements("//fetch/entity")
				.Where(e => e.Attributes("name").Any(a => a.Value == LogicalName))
				.FirstOrDefault();

			if (entity == null)
			{
				return false;
			}

			var linkEntity = entity.XPathSelectElements("link-entity")
				.Where(e => e.Attributes("alias").Any(a => a.Value == nameParts.First()))
				.FirstOrDefault() 
				?? entity.XPathSelectElements("link-entity").FirstOrDefault().XPathSelectElements("link-entity")
				.FirstOrDefault().XPathSelectElements("link-entity").FirstOrDefault(a => a.Attributes("alias").Any(n => n.Value == nameParts.First()));

			if (linkEntity == null)
			{
				return false;
			}

			var linkEntityAttribute =
				linkEntity.XPathSelectElements("attribute")
					.Where(e => e.Attributes("name").Any(a => a.Value == nameParts.Last()))
					.FirstOrDefault()
				?? linkEntity.XPathSelectElements("link-entity").Where(a => a.Attributes("from").Any(p => p.Value == nameParts.Last())).FirstOrDefault();

			if (linkEntityAttribute == null)
			{
				return false;
			}

			var entityName = linkEntity.Attributes("name").Select(a => a.Value).FirstOrDefault();

			if (string.IsNullOrEmpty(entityName))
			{
				return false;
			}

			var logicalName = linkEntityAttribute.Attributes("to").Select(a => a.Value).FirstOrDefault() ?? linkEntityAttribute.Attributes("name").Select(a => a.Value).FirstOrDefault();

			if (string.IsNullOrEmpty(logicalName))
			{
				return false;
			}

			linkAttribute = new FetchXmlLinkAttribute(logicalName, entityName);

			return true;
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

		private void AddFetchElement(XElement fetchElement)
		{
			foreach (var element in _xml.XPathSelectElements("//fetch/entity"))
			{
				element.Add(fetchElement);
			}
		}
	}

	internal static class XAttributeExtensions
	{
		public static string GetValue(this XAttribute attribute)
		{
			return attribute != null ? attribute.Value : null;
		}
	}

	internal static class XElementExtensions
	{
		public static string GetValue(this XElement element)
		{
			return element != null ? element.Value : null;
		}

		public static string GetAttributeValue(this XElement element, XName name)
		{
			return element != null ? element.Attribute(name).GetValue() : null;
		}
	}
}
