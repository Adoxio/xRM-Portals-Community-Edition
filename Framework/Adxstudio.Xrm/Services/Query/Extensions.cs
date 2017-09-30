/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Services.Query
{
	internal static class Extensions
	{
		public static readonly string FetchNamespace = "http://schemas.adxstudio.com/fetch/";

		public static string GetAttribute(this XElement element, XName name)
		{
			var attribute = element.Attribute(name);
			return attribute != null ? attribute.Value : null;
		}

		public static string GetAttribute(this JToken element, string name)
		{
			var attribute = element[name];
			return attribute != null ? attribute.Value<string>() : null;
		}

		public static T GetAttribute<T>(this XElement element, XName name)
		{
			var attribute = element.Attribute(name);
			var value = GetAttribute<T>(attribute);
			return value != null ? (T)value : default(T);
		}

		public static T GetAttribute<T>(this JToken element, string name)
		{
			var attribute = element[name];
			var value = GetAttribute<T>(attribute);
			return value != null ? (T)value : default(T);
		}

		public static object GetAttribute<T>(this XAttribute attribute)
		{
			if (attribute == null) return null;
			if (typeof(T).GetUnderlyingType() == typeof(bool)) return bool.Parse(attribute.Value);
			if (typeof(T).GetUnderlyingType() == typeof(int)) return int.Parse(attribute.Value);
			if (typeof(T) == typeof(object)) return attribute.Value;

			return null;
		}

		public static object GetAttribute<T>(this JToken attribute)
		{
			if (attribute == null) return null;
			if (typeof(T).GetUnderlyingType() == typeof(bool)) return bool.Parse(attribute.Value<string>());
			if (typeof(T).GetUnderlyingType() == typeof(int)) return int.Parse(attribute.Value<string>());
			if (typeof(T) == typeof(object)) return attribute.Value<JValue>().Value;

			return null;
		}

		public static T? GetAttribute<T>(this XElement element, XName name, IDictionary<T, string> lookup) where T : struct
		{
			var attribute = element.Attribute(name);

			if (attribute != null)
			{
				var value = lookup.First(pair => pair.Value == attribute.Value);

				return value.Key;
			}

			return null;
		}

        public static T? GetAttribute<T>(this XElement element, XName name, Func<string, T> GetKeyByValue) where T : struct
        {
            var attribute = element.Attribute(name);

            if (attribute != null)
            {
                return GetKeyByValue(attribute.Value);
            }

            return null;
        }

        public static T? GetAttribute<T>(this JToken element, string name, IDictionary<T, string> lookup) where T : struct
		{
			var attribute = element[name];

			if (attribute != null)
			{
				var value = lookup.First(pair => pair.Value == attribute.Value<string>());

				return value.Key;
			}

			return null;
		}

        public static T? GetAttribute<T>(this JToken element, string name, Func<string, T> GetKeyByValue) where T : struct
        {
            var attribute = element[name];

            if (attribute != null)
            {
                return GetKeyByValue(attribute.Value<string>());
            }

            return null;
        }

        public static ICollection<FetchAttribute> GetAttributes(this XElement element)
		{
			if (element.Element("all-attributes") != null) return FetchAttribute.All;
			if (element.Element("no-attrs") != null) return FetchAttribute.None;
			return FetchAttribute.Parse(element.Elements("attribute"));
		}

		public static ICollection<FetchAttribute> GetAttributes(this JToken element)
		{
			if (element["all-attributes"] != null && element["all-attributes"].Value<bool>()) return FetchAttribute.All;
			if (element["no-attrs"] != null && element["no-attrs"].Value<bool>()) return FetchAttribute.None;

			var attributes = element["attributes"] as JArray;

			if (attributes != null)
			{
				return attributes.Select(attribute => FetchAttribute.Parse(attribute)).ToList();
			}

			return new List<FetchAttribute>();
		}

		public static JToken Element(this JToken element, string name)
		{
			return element[name];
		}

		public static IEnumerable<JToken> Elements(this JToken element, string name)
		{
			var items = element[name] as JArray;

			if (items != null)
			{
				foreach (var item in items)
				{
					yield return item;
				}
			}
		}

		public static XAttribute GetExtension(this IEnumerable<XAttribute> attributes, string name)
		{
			return attributes.FirstOrDefault(a => a.Name == XName.Get(name, FetchNamespace));
		}

		public static string GetExtensionValue(this IEnumerable<XAttribute> attributes, string name)
		{
			var attribute = GetExtension(attributes, name);
			return attribute != null ? attribute.Value : null;
		}

		public static ICollection<XAttribute> GetExtensions(this XElement element)
		{
			return element.Attributes().Where(a => !string.IsNullOrEmpty(a.Name.NamespaceName)).ToList();
		}

		public static ICollection<XAttribute> GetExtensions(this JToken element, IEnumerable<XAttribute> xmlns)
		{
			return element
				.Children()
				.OfType<JProperty>()
				.Where(p => p.Name.Contains("."))
				.Select(p => p.ToExtension(xmlns))
				.Where(a => a != null)
				.ToList();
		}

		private static XAttribute ToExtension(this JProperty property, IEnumerable<XAttribute> xmlns)
		{
			if (IsXmlns(property))
			{
				return ToNamespace(property);
			}

			var index = property.Name.IndexOf('.');
			var prefix = property.Name.Substring(0, index);
			var localName = property.Name.Substring(index + 1);

			var attribute = xmlns.FirstOrDefault(a => string.Equals(a.Name.LocalName, prefix));

			if (attribute != null)
			{
				return new XAttribute(XNamespace.Get(attribute.Value) + localName, property.Value);
			}

			return null;
		}

		public static ICollection<XAttribute> ToNamespaces(this JToken element, IEnumerable<XAttribute> xmlns)
		{
			return element
				.Children()
				.OfType<JProperty>()
				.Where(IsXmlns)
				.Select(ToNamespace)
				.Concat(xmlns ?? new XAttribute[] { })
				.ToList();
		}

		private static XAttribute ToNamespace(this JProperty property)
		{
			var value = property.Value.Value<string>();
			var local = property.Name.Substring(6);
			var xmlns = XNamespace.Xmlns + local;
			return new XAttribute(xmlns, value);
		}

		private static bool IsXmlns(this JProperty property)
		{
			return property.Value != null && property.Value.Type == JTokenType.String && property.Name.StartsWith("xmlns.", StringComparison.OrdinalIgnoreCase);
		}

		public static ICollection<Link> Clone(this ICollection<Link> links)
		{
			return links.Select(link => link.Clone()).ToArray();
		}
	}
}
