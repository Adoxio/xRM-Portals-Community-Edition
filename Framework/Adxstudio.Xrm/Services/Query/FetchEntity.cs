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
	using Newtonsoft.Json.Linq;

	public class FetchEntity
	{
		public string Name { get; set; }
		public ICollection<FetchAttribute> Attributes { get; set; }
		public ICollection<Order> Orders { get; set; }
		public ICollection<Filter> Filters { get; set; }
		public ICollection<Link> Links { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("entity", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Name != null) yield return new XAttribute("name", Name);

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

		public static FetchEntity Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static FetchEntity FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static FetchEntity Parse(XElement element)
		{
			if (element == null) return null;

			return new FetchEntity
			{
				Name = element.GetAttribute("name"),

				Attributes = element.GetAttributes(),

				Orders = Order.Parse(element.Elements("order")),
				Filters = Filter.Parse(element.Elements("filter")),
				Links = Link.Parse(element.Elements("link-entity")),

				Extensions = element.GetExtensions(),
			};
		}

		public static FetchEntity Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);

			return new FetchEntity
			{
				Name = element.GetAttribute("name"),

				Attributes = element.GetAttributes(),

				Orders = Order.Parse(element.Elements("orders"), namespaces),
				Filters = Filter.Parse(element.Elements("filters"), namespaces),
				Links = Link.Parse(element.Elements("links"), namespaces),

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public FetchEntity()
		{
		}

		public FetchEntity(string name)
		{
			Name = name;
		}

		public FetchEntity(string name, IEnumerable<string> attributes)
			: this(name)
		{
			Attributes = attributes.Select(attribute => new FetchAttribute(attribute)).ToArray();
		}
	}
}
