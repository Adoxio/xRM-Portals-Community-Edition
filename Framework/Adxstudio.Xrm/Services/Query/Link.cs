/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Query
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Xml.Linq;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json.Linq;

	public class Link
	{
		public string Name { get; set; }
		public string FromAttribute { get; set; }
		public string ToAttribute { get; set; }
		public string Alias { get; set; }
		public JoinOperator? Type { get; set; }
		public bool? Visible { get; set; }
		public bool? Intersect { get; set; }
		public bool? IsUnique { get; set; }

		public IEnumerable<FetchAttribute> Attributes { get; set; }
		public IEnumerable<Order> Orders { get; set; }
		public IEnumerable<Filter> Filters { get; set; }
		public IEnumerable<Link> Links { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("link-entity", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Name != null) yield return new XAttribute("name", Name);
			if (FromAttribute != null) yield return new XAttribute("from", FromAttribute);
			if (ToAttribute != null) yield return new XAttribute("to", ToAttribute);
			if (Alias != null) yield return new XAttribute("alias", Alias);
			if (Type != null) yield return new XAttribute("link-type", Lookups.JoinOperatorToText[Type.Value]);
			if (Visible != null) yield return new XAttribute("visible", Visible.Value);
			if (Intersect != null) yield return new XAttribute("intersect", Intersect.Value);

			if (Equals(Attributes, FetchAttribute.All)) yield return new XElement("all-attributes");
			else if (Equals(Attributes, FetchAttribute.None)) yield return new XElement("no-attrs");
			else if (Attributes != null) foreach (var attribute in Attributes) yield return attribute.ToXml();

			if (Orders != null)
			{
				foreach (var order in Orders) yield return order.ToXml();
			}

			if (Filters != null)
			{
				foreach (var filter in Filters) yield return filter.ToXml();
			}

			if (Links != null)
			{
				foreach (var link in Links) yield return link.ToXml();
			}

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public Link Clone()
		{
			return Parse(ToXml());
		}

		public static Link Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static Link FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static Link Parse(XElement element)
		{
			if (element == null) return null;

			return new Link
			{
				Name = element.GetAttribute("name"),
				FromAttribute = element.GetAttribute("from"),
				ToAttribute = element.GetAttribute("to"),
				Alias = element.GetAttribute("alias"),
				Type = element.GetAttribute("link-type", Lookups.JoinOperatorToText),
				Visible = element.GetAttribute<bool?>("visible"),
				Intersect = element.GetAttribute<bool?>("intersect"),

				Attributes = element.GetAttributes(),

				Orders = Order.Parse(element.Elements("order")),
				Filters = Filter.Parse(element.Elements("filter")),
				Links = Parse(element.Elements("link-entity")),

				Extensions = element.GetExtensions(),
			};
		}

		public static Link Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);

			return new Link
			{
				Name = element.GetAttribute("name"),
				FromAttribute = element.GetAttribute("from"),
				ToAttribute = element.GetAttribute("to"),
				Alias = element.GetAttribute("alias"),
				Type = element.GetAttribute("link-type", Lookups.JoinOperatorToText),
				Visible = element.GetAttribute<bool?>("visible"),
				Intersect = element.GetAttribute<bool?>("intersect"),

				Attributes = element.GetAttributes(),

				Orders = Order.Parse(element.Elements("orders"), namespaces),
				Filters = Filter.Parse(element.Elements("filters"), namespaces),
				Links = Parse(element.Elements("links"), namespaces),

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public static ICollection<Link> Parse(IEnumerable<XElement> elements)
		{
			return elements.Select(Parse).ToList();
		}

		public static ICollection<Link> Parse(IEnumerable<JToken> elements, IEnumerable<XAttribute> xmlns = null)
		{
			return elements == null ? null : elements.Select(element => Parse(element, xmlns)).ToList();
		}
	}
}
