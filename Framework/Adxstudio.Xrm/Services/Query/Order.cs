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

	public class Order
	{
		public string Attribute { get; set; }
		public string Alias { get; set; }
		public OrderType? Direction { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("order", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Attribute != null) yield return new XAttribute("attribute", Attribute);
			if (Alias != null) yield return new XAttribute("alias", Alias);
			if (Direction == OrderType.Descending) yield return new XAttribute("descending", "true");
			if (Direction == OrderType.Ascending) yield return new XAttribute("descending", "false");

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public static Order Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static Order FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static Order Parse(XElement element)
		{
			if (element == null) return null;

			var attribute = element.Attribute("descending");

			bool descending;

			var direction = attribute != null && bool.TryParse(attribute.Value, out descending)
				? descending ? OrderType.Descending : OrderType.Ascending as OrderType?
				: null;

			return new Order
			{
				Attribute = element.GetAttribute("attribute"),
				Alias = element.GetAttribute("alias"),
				Direction = direction,

				Extensions = element.GetExtensions(),
			};
		}

		public static Order Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);
			var attribute = element["descending"];

			bool descending;

			var direction = attribute != null && bool.TryParse(attribute.Value<string>(), out descending)
				? descending ? OrderType.Descending : OrderType.Ascending as OrderType?
				: null;

			return new Order
			{
				Attribute = element.GetAttribute("attribute"),
				Alias = element.GetAttribute("alias"),
				Direction = direction,

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public static ICollection<Order> Parse(IEnumerable<XElement> elements)
		{
			return elements.Select(Parse).ToList();
		}

		public static ICollection<Order> Parse(IEnumerable<JToken> elements, IEnumerable<XAttribute> xmlns = null)
		{
			return elements == null ? null : elements.Select(element => Parse(element, xmlns)).ToList();
		}

		public Order()
		{
		}

		public Order(string attribute)
		{
			Attribute = attribute;
		}

		public Order(string attribute, OrderType direction)
			: this(attribute)
		{
			Direction = direction;
		}
	}
}
