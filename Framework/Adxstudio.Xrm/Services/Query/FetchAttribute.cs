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

	public class FetchAttribute
	{
		public static ICollection<FetchAttribute> All = new LinkedList<FetchAttribute>();
		public static ICollection<FetchAttribute> None = new LinkedList<FetchAttribute>();

		public string Name { get; set; }
		public string Build { get; set; }
		public string AddedBy { get; set; }
		public string Alias { get; set; }
		public AggregateType? Aggregate { get; set; }
		public bool? GroupBy { get; set; }
		public DateGroupingType? DateGrouping { get; set; }
		public bool? UserTimeZone { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		public XElement ToXml()
		{
			return new XElement("attribute", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			if (Name != null) yield return new XAttribute("name", Name);
			if (Build != null) yield return new XAttribute("build", Build);
			if (AddedBy != null) yield return new XAttribute("addedby", AddedBy);
			if (Alias != null) yield return new XAttribute("alias", Alias);
			if (Aggregate != null) yield return new XAttribute("aggregate", Lookups.AggregateToText[Aggregate.Value]);
			if (GroupBy != null) yield return new XAttribute("groupby", GroupBy.Value);
			if (DateGrouping != null) yield return new XAttribute("dategrouping", Lookups.DateGroupingToText[DateGrouping.Value]);
			if (UserTimeZone != null) yield return new XAttribute("usertimezone", UserTimeZone.Value);

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public static FetchAttribute Parse(string text)
		{
			return Parse(XElement.Parse(text));
		}

		public static FetchAttribute FromJson(string text)
		{
			return Parse(JObject.Parse(text));
		}

		public static FetchAttribute Parse(XElement element)
		{
			if (element == null) return null;

			return new FetchAttribute
			{
				Name = element.GetAttribute("name"),
				Build = element.GetAttribute("build"),
				AddedBy = element.GetAttribute("addedby"),
				Alias = element.GetAttribute("alias"),
				Aggregate = element.GetAttribute("aggregate", Lookups.AggregateToText),
				GroupBy = element.GetAttribute<bool?>("groupby"),
				DateGrouping = element.GetAttribute("dategrouping", Lookups.DateGroupingToText),
				UserTimeZone = element.GetAttribute<bool?>("usertimezone"),

				Extensions = element.GetExtensions(),
			};
		}

		public static FetchAttribute Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);

			return new FetchAttribute
			{
				Name = element.GetAttribute("name"),
				Build = element.GetAttribute("build"),
				AddedBy = element.GetAttribute("addedby"),
				Alias = element.GetAttribute("alias"),
				Aggregate = element.GetAttribute("aggregate", Lookups.AggregateToText),
				GroupBy = element.GetAttribute<bool?>("groupby"),
				DateGrouping = element.GetAttribute("dategrouping", Lookups.DateGroupingToText),
				UserTimeZone = element.GetAttribute<bool?>("usertimezone"),

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public static ICollection<FetchAttribute> Parse(IEnumerable<XElement> elements)
		{
			return elements.Select(Parse).ToList();
		}

		public static ICollection<FetchAttribute> Parse(IEnumerable<JToken> elements, IEnumerable<XAttribute> xmlns = null)
		{
			return elements == null ? null : elements.Select(element => Parse(element, xmlns)).ToList();
		}

		public FetchAttribute()
		{
		}

		public FetchAttribute(string name)
		{
			Name = name;
		}

		public FetchAttribute(string name, string alias, AggregateType aggregate)
			: this(name)
		{
			Alias = alias;
			Aggregate = aggregate;
		}
	}
}
