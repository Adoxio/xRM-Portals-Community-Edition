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

	public class Filter
	{
		public static class Hints
		{
			public const string Union = "union";
		}

		public string Hint { get; set; }
		public bool? IsQuickFindFields { get; set; }
		public LogicalOperator? Type { get; set; }
		public ICollection<Condition> Conditions { get; set; }
		public ICollection<Filter> Filters { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("filter", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Type != null) yield return new XAttribute("type", Lookups.LogicalOperatorToText[Type.Value]);
			if (Hint != null) yield return new XAttribute("hint", Hint);
			if (IsQuickFindFields != null) yield return new XAttribute("isquickfindfields", IsQuickFindFields.Value);

			if (Conditions != null)
			{
				foreach (var condition in Conditions) yield return condition.ToXml();
			}

			if (Filters != null)
			{
				foreach (var filter in Filters) yield return filter.ToXml();
			}

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public static Filter Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static Filter FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static Filter Parse(XElement element)
		{
			if (element == null) return null;

			return new Filter
			{
				Type = element.GetAttribute("type", Lookups.LogicalOperatorToText),
				Hint = element.GetAttribute<string>("hint"),
				IsQuickFindFields = element.GetAttribute<bool?>("isquickfindfields"),

				Conditions = Condition.Parse(element.Elements("condition")),
				Filters = Parse(element.Elements("filter")),

				Extensions = element.GetExtensions(),
			};
		}

		public static Filter Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);

			return new Filter
			{
				Type = element.GetAttribute("type", Lookups.LogicalOperatorToText),
				IsQuickFindFields = element.GetAttribute<bool?>("isquickfindfields"),

				Conditions = Condition.Parse(element.Elements("conditions"), namespaces),
				Filters = Parse(element.Elements("filters"), namespaces),

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public static ICollection<Filter> Parse(IEnumerable<XElement> elements)
		{
			return elements == null ? null : elements.Select(Parse).ToList();
		}

		public static ICollection<Filter> Parse(IEnumerable<JToken> elements, IEnumerable<XAttribute> xmlns = null)
		{
			return elements == null ? null : elements.Select(element => Parse(element, xmlns)).ToList();
		}
	}
}
