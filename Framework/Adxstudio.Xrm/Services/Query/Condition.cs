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

	public class Condition
	{
		public string Column { get; set; }
		public string Attribute { get; set; }
		public string EntityName { get; set; }
		public ConditionOperator? Operator { get; set; }
		public AggregateType? Aggregate { get; set; }
		public string Alias { get; set; }
		public string UiName { get; set; }
		public string UiType { get; set; }
		public bool? UiHidden { get; set; }
		public object Value { get; set; }
		public ICollection<object> Values { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("condition", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Column != null) yield return new XAttribute("column", Column);
			if (EntityName != null) yield return new XAttribute("entityname", EntityName);
			if (Attribute != null) yield return new XAttribute("attribute", Attribute);
			if (Operator != null) yield return new XAttribute("operator", FetchXMLConditionalOperators.GetValueByKey(Operator.Value));
			if (Aggregate != null) yield return new XAttribute("aggregate", Lookups.AggregateToText[Aggregate.Value]);
			if (Alias != null) yield return new XAttribute("alias", Alias);
			if (UiName != null) yield return new XAttribute("uiname", UiName);
			if (UiType != null) yield return new XAttribute("uitype", UiType);
			if (UiHidden != null) yield return new XAttribute("uihidden", UiHidden.Value ? "1" : "0");

			if (Value != null) yield return new XAttribute("value", Value);

			if (Values != null)
			{
				foreach (var value in Values) yield return new XElement("value", value);
			}

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public static Condition Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static Condition FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static Condition Parse(XElement element)
		{
			if (element == null) return null;

			var elements = element.Elements("value").Select(e => e.Value as object).ToList();

			return new Condition
			{
				Column = element.GetAttribute("column"),
				EntityName = element.GetAttribute("entityname"),
				Attribute = element.GetAttribute("attribute"),
				Operator = element.GetAttribute<ConditionOperator>("operator", FetchXMLConditionalOperators.GetKeyByValue),
				Aggregate = element.GetAttribute("aggregate", Lookups.AggregateToText),
				Alias = element.GetAttribute("alias"),
				UiName = element.GetAttribute("uiname"),
				UiType = element.GetAttribute("uitype"),
				UiHidden = element.GetAttribute<bool?>("uihidden"),

				Value = element.GetAttribute("value"),

				Values = elements,

				Extensions = element.GetExtensions(),
			};
		}

		public static Condition Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);
			var elements = element.Elements("value").Select(e => e.Value<object>()).ToList();

			return new Condition
			{
				Column = element.GetAttribute("column"),
				Attribute = element.GetAttribute("attribute"),
				Operator = element.GetAttribute<ConditionOperator>("operator", FetchXMLConditionalOperators.GetKeyByValue),
				Aggregate = element.GetAttribute("aggregate", Lookups.AggregateToText),
				Alias = element.GetAttribute("alias"),
				UiName = element.GetAttribute("uiname"),
				UiType = element.GetAttribute("uitype"),
				UiHidden = element.GetAttribute<bool?>("uihidden"),

				Value = element.GetAttribute<object>("value"),

				Values = elements,

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public static ICollection<Condition> Parse(IEnumerable<XElement> elements)
		{
			return elements.Select(Parse).ToList();
		}

		public static ICollection<Condition> Parse(IEnumerable<JToken> elements, IEnumerable<XAttribute> xmlns = null)
		{
			return elements == null ? null : elements.Select(element => Parse(element, xmlns)).ToList();
		}

		public Condition()
		{
		}

		public Condition(string attribute, ConditionOperator @operator)
		{
			Attribute = attribute;
			Operator = @operator;
		}

		public Condition(string attribute, ConditionOperator @operator, object value)
			: this(attribute, @operator)
		{
			Value = value;
		}

		public Condition(string attribute, ConditionOperator @operator, ICollection<object> values)
			: this(attribute, @operator)
		{
			Values = values;
		}

		public Condition(string entityName, string attribute, ConditionOperator @operator, object value)
		{
			EntityName = entityName;
			Attribute = attribute;
			Operator = @operator;
			Value = value;
		}
	}
}
