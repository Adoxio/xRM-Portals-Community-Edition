/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Services.Query
{
	// http://msdn.microsoft.com/en-us/library/gg309405.aspx

	public enum AggregateType
	{
		Count,
		CountColumn,
		Sum,
		Avg,
		Min,
		Max,
	}

	public enum DateGroupingType
	{
		Day,
		Week,
		Month,
		Quarter,
		Year,
		FiscalPeriod,
		FiscalYear,
	}

	public enum MappingType
	{
		Logical,
		Internal,
		Physical,
	}

	public enum OutputFormatType
	{
		Auto,
		Ado,
		Elements,
		Raw,
		Platform,
	}

	internal static class Lookups
	{
		public static readonly IDictionary<DateGroupingType, string> DateGroupingToText = new Dictionary<DateGroupingType, string>
		{
			{ DateGroupingType.Day, "day" },
			{ DateGroupingType.Week, "week" },
			{ DateGroupingType.Month, "month" },
			{ DateGroupingType.Quarter, "quarter" },
			{ DateGroupingType.Year, "year" },
			{ DateGroupingType.FiscalPeriod, "fiscal-period" },
			{ DateGroupingType.FiscalYear, "fiscal-year" },
		};

		public static readonly IDictionary<AggregateType, string> AggregateToText = new Dictionary<AggregateType, string>
		{
			{ AggregateType.Count, "count" },
			{ AggregateType.CountColumn, "countcolumn" },
			{ AggregateType.Sum, "sum" },
			{ AggregateType.Avg, "avg" },
			{ AggregateType.Min, "min" },
			{ AggregateType.Max, "max" },
		};

		public static readonly IDictionary<JoinOperator, string> JoinOperatorToText = new Dictionary<JoinOperator, string>
		{
			{ JoinOperator.Natural, "natural" },
			{ JoinOperator.Inner, "inner" },
			{ JoinOperator.LeftOuter, "outer" },
		};

        public static readonly IDictionary<LogicalOperator, string> LogicalOperatorToText = new Dictionary<LogicalOperator, string>
		{
			{ LogicalOperator.And, "and" },
			{ LogicalOperator.Or, "or" },
		};

		public static readonly IDictionary<MappingType, string> MappingTypeToText = new Dictionary<MappingType, string>
		{
			{ MappingType.Logical, "logical" },
			{ MappingType.Internal, "internal" },
		};

		public static readonly IDictionary<OutputFormatType, string> OutputFormatTypeToText = new Dictionary<OutputFormatType, string>
		{
			{ OutputFormatType.Auto, "xml-auto" },
			{ OutputFormatType.Ado, "xml-ado" },
			{ OutputFormatType.Elements, "xml-elements" },
			{ OutputFormatType.Raw, "xml-raw" },
			{ OutputFormatType.Platform, "xml-platform" },
		};
	}
}
