/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Query
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Runtime.Serialization;
	using System.Xml.Linq;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json.Linq;

	public class Fetch
	{
		/// <summary>
		/// The maximum number of results a query can retrieve in a single request.
		/// </summary>
		public const int MaximumPageSize = 5000;

		public string Version { get; set; }
		public int? PageSize { get; set; }
		public int? PageNumber { get; set; }
		public string PagingCookie { get; set; }
		public int? UtcOffset { get; set; }
		public bool? Aggregate { get; set; }
		public bool? Distinct { get; set; }
		public MappingType MappingType { get; set; }
		public bool? MinActiveRowVersion { get; set; }
		public OutputFormatType? OutputFormat { get; set; }
		public bool? ReturnTotalRecordCount { get; set; }
		public bool? NoLock { get; set; }

		/// <summary>
		/// SkipCache means:
		/// 1) we will always execute this Fetch without checking if result is already available in cache and 
		/// 2) We will not cache the result.
		/// </summary>
		public bool? SkipCache { get; set; }

		public FetchEntity Entity { get; set; }

		[IgnoreDataMember]
		public ICollection<XAttribute> Extensions { get; set; }

		/// <remarks>
		/// This is for the Reports view only.
		/// </remarks>
		public ICollection<Order> Orders { get; set; }

		public XElement ToXml()
		{
			return new XElement("fetch", GetContent());
		}

		private IEnumerable<XObject> GetContent()
		{
			yield return new XAttribute("mapping", Lookups.MappingTypeToText[MappingType]);
			if (Version != null) yield return new XAttribute("version", Version);
			if (PageSize != null) yield return new XAttribute("count", PageSize.Value);
			if (PageNumber != null) yield return new XAttribute("page", PageNumber.Value);
			if (PagingCookie != null) yield return new XAttribute("paging-cookie", PagingCookie);
			if (UtcOffset != null) yield return new XAttribute("utc-offset", UtcOffset.Value);

			if (Aggregate != null) yield return new XAttribute("aggregate", Aggregate.Value);
			if (Distinct != null) yield return new XAttribute("distinct", Distinct.Value);
			if (MinActiveRowVersion != null) yield return new XAttribute("min-active-row-version", MinActiveRowVersion.Value);
			if (OutputFormat != null) yield return new XAttribute("output-format", Lookups.OutputFormatTypeToText[OutputFormat.Value]);
			if (ReturnTotalRecordCount != null) yield return new XAttribute("returntotalrecordcount", ReturnTotalRecordCount.Value);
			if (NoLock != null) yield return new XAttribute("no-lock", NoLock.Value);

			if (Orders != null)
			{
				foreach (var order in Orders) yield return order.ToXml();
			}

			if (Entity != null) yield return Entity.ToXml();

			if (Extensions != null)
			{
				foreach (var extension in Extensions) yield return extension;
			}
		}

		public static Fetch Parse(string text)
		{
			return text == null ? null : Parse(XElement.Parse(text));
		}

		public static Fetch FromJson(string text)
		{
			return text == null ? null : Parse(JObject.Parse(text));
		}

		public static Fetch Parse(XElement element)
		{
			if (element == null) return null;

			return new Fetch
			{
				MappingType = element.GetAttribute("mapping", Lookups.MappingTypeToText).GetValueOrDefault(),
				Version = element.GetAttribute("version"),
				PageSize = element.GetAttribute<int?>("count"),
				PageNumber = element.GetAttribute<int?>("page"),
				PagingCookie = element.GetAttribute("paging-cookie"),
				UtcOffset = element.GetAttribute<int?>("utc-offset"),

				Aggregate = element.GetAttribute<bool?>("aggregate"),
				Distinct = element.GetAttribute<bool?>("distinct"),
				MinActiveRowVersion = element.GetAttribute<bool?>("min-active-row-version"),
				OutputFormat = element.GetAttribute("output-format", Lookups.OutputFormatTypeToText),
				ReturnTotalRecordCount = element.GetAttribute<bool?>("returntotalrecordcount"),
				NoLock = element.GetAttribute<bool?>("no-lock"),

				Orders = Order.Parse(element.Elements("order")),
				Entity = FetchEntity.Parse(element.Element("entity")),

				Extensions = element.GetExtensions(),
			};
		}

		public static Fetch Parse(JToken element, IEnumerable<XAttribute> xmlns = null)
		{
			if (element == null) return null;

			var namespaces = element.ToNamespaces(xmlns);

			return new Fetch
			{
				MappingType = element.GetAttribute("mapping", Lookups.MappingTypeToText).GetValueOrDefault(),
				Version = element.GetAttribute("version"),
				PageSize = element.GetAttribute<int?>("count"),
				PageNumber = element.GetAttribute<int?>("page"),
				PagingCookie = element.GetAttribute("paging-cookie"),
				UtcOffset = element.GetAttribute<int?>("utc-offset"),

				Aggregate = element.GetAttribute<bool?>("aggregate"),
				Distinct = element.GetAttribute<bool?>("distinct"),
				MinActiveRowVersion = element.GetAttribute<bool?>("min-active-row-version"),
				OutputFormat = element.GetAttribute("output-format", Lookups.OutputFormatTypeToText),
				ReturnTotalRecordCount = element.GetAttribute<bool?>("returntotalrecordcount"),
				NoLock = element.GetAttribute<bool?>("no-lock"),

				Orders = Order.Parse(element.Elements("orders"), namespaces),
				Entity = FetchEntity.Parse(element.Element("entity"), namespaces),

				Extensions = element.GetExtensions(namespaces),
			};
		}

		public FetchExpression ToFetchExpression()
		{
			return new FetchExpression(ToXml().ToString());
		}

		public RetrieveMultipleRequest ToRetrieveMultipleRequest()
		{
			return new RetrieveMultipleRequest { Query = ToFetchExpression() };
		}

		internal RetrieveSingleRequest ToRetrieveSingleRequest()
		{
			return new RetrieveSingleRequest(ToFetchExpression());
		}

		public EntityCollection Execute(
			IOrganizationService service,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return service.RetrieveMultiple(this, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}
	}
}
