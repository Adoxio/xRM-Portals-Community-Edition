/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Query
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// FetchXMLConditionalOperators class
	/// </summary>
	internal static class FetchXMLConditionalOperators
	{
		/// <summary>
		/// ConditionOperatorToTextFromLookUp variable
		/// </summary>
		private static readonly IDictionary<ConditionOperator, string[]> ConditionOperatorToTextFromLookUp = new Dictionary<ConditionOperator, string[]>
		{
			{ ConditionOperator.Equal, new string[] { "eq" } },
			{ ConditionOperator.NotEqual, new string[] { "ne", "neq" } },
			{ ConditionOperator.GreaterThan, new string[] { "gt" } },
			{ ConditionOperator.LessThan, new string[] { "lt" } },
			{ ConditionOperator.GreaterEqual, new string[] { "ge" } },
			{ ConditionOperator.LessEqual, new string[] { "le" } },
			{ ConditionOperator.Like, new string[] { "like" } },
			{ ConditionOperator.NotLike, new string[] { "not-like" } },
			{ ConditionOperator.In, new string[] { "in" } },
			{ ConditionOperator.NotIn, new string[] { "not-in" } },
			{ ConditionOperator.Between, new string[] { "between" } },
			{ ConditionOperator.NotBetween, new string[] { "not-between" } },
			{ ConditionOperator.Null, new string[] { "null" } },
			{ ConditionOperator.NotNull, new string[] { "not-null" } },
			{ ConditionOperator.Yesterday, new string[] { "yesterday" } },
			{ ConditionOperator.Today, new string[] { "today" } },
			{ ConditionOperator.Tomorrow, new string[] { "tomorrow" } },
			{ ConditionOperator.Last7Days, new string[] { "last-seven-days" } },
			{ ConditionOperator.Next7Days, new string[] { "next-seven-days" } },
			{ ConditionOperator.LastWeek, new string[] { "last-week" } },
			{ ConditionOperator.ThisWeek, new string[] { "this-week" } },
			{ ConditionOperator.NextWeek, new string[] { "next-week" } },
			{ ConditionOperator.LastMonth, new string[] { "last-month" } },
			{ ConditionOperator.ThisMonth, new string[] { "this-month" } },
			{ ConditionOperator.NextMonth, new string[] { "next-month" } },
			{ ConditionOperator.On, new string[] { "on" } },
			{ ConditionOperator.OnOrBefore, new string[] { "on-or-before" } },
			{ ConditionOperator.OnOrAfter, new string[] { "on-or-after" } },
			{ ConditionOperator.LastYear, new string[] { "last-year" } },
			{ ConditionOperator.ThisYear, new string[] { "this-year" } },
			{ ConditionOperator.NextYear, new string[] { "next-year" } },
			{ ConditionOperator.LastXHours, new string[] { "last-x-hours" } },
			{ ConditionOperator.NextXHours, new string[] { "next-x-hours" } },
			{ ConditionOperator.LastXDays, new string[] { "last-x-days" } },
			{ ConditionOperator.NextXDays, new string[] { "next-x-days" } },
			{ ConditionOperator.LastXWeeks, new string[] { "last-x-weeks" } },
			{ ConditionOperator.NextXWeeks, new string[] { "next-x-weeks" } },
			{ ConditionOperator.LastXMonths, new string[] { "last-x-months" } },
			{ ConditionOperator.NextXMonths, new string[] { "next-x-months" } },
			{ ConditionOperator.LastXYears, new string[] { "last-x-years" } },
			{ ConditionOperator.NextXYears, new string[] { "next-x-years" } },
			{ ConditionOperator.EqualUserId, new string[] { "eq-userid" } },
			{ ConditionOperator.NotEqualUserId, new string[] { "ne-userid" } },
			{ ConditionOperator.EqualBusinessId, new string[] { "eq-businessid" } },
			{ ConditionOperator.NotEqualBusinessId, new string[] { "ne-businessid" } },
			{ ConditionOperator.ChildOf, new string[] { "child-of" } },
			{ ConditionOperator.Mask, new string[] { "mask" } }, // unconfirmed
		    { ConditionOperator.NotMask, new string[] { "not-mask" } }, // unconfirmed
		    { ConditionOperator.MasksSelect, new string[] { "masks-select" } }, // unconfirmed
		    { ConditionOperator.Contains, new string[] { "like" } },
			{ ConditionOperator.DoesNotContain, new string[] { "not-like" } },
			{ ConditionOperator.EqualUserLanguage, new string[] { "eq-userlanguage" } },
			{ ConditionOperator.NotOn, new string[] { "not-on" } }, // unconfirmed
		    { ConditionOperator.OlderThanXMonths, new string[] { "olderthan-x-months" } },
			{ ConditionOperator.BeginsWith, new string[] { "begins-with" } },
			{ ConditionOperator.DoesNotBeginWith, new string[] { "not-begin-with" } },
			{ ConditionOperator.EndsWith, new string[] { "ends-with" } },
			{ ConditionOperator.DoesNotEndWith, new string[] { "not-end-with" } },
			{ ConditionOperator.ThisFiscalYear, new string[] { "this-fiscal-year" } },
			{ ConditionOperator.ThisFiscalPeriod, new string[] { "this-fiscal-period" } },
			{ ConditionOperator.NextFiscalYear, new string[] { "next-fiscal-year" } },
			{ ConditionOperator.NextFiscalPeriod, new string[] { "next-fiscal-period" } },
			{ ConditionOperator.LastFiscalYear, new string[] { "last-fiscal-year" } },
			{ ConditionOperator.LastFiscalPeriod, new string[] { "last-fiscal-period" } },
			{ ConditionOperator.LastXFiscalYears, new string[] { "last-x-fiscal-years" } },
			{ ConditionOperator.LastXFiscalPeriods, new string[] { "last-x-fiscal-periods" } },
			{ ConditionOperator.NextXFiscalYears, new string[] { "next-x-fiscal-years" } },
			{ ConditionOperator.NextXFiscalPeriods, new string[] { "next-x-fiscal-periods" } },
			{ ConditionOperator.InFiscalYear, new string[] { "in-fiscal-year" } },
			{ ConditionOperator.InFiscalPeriod, new string[] { "in-fiscal-period" } },
			{ ConditionOperator.InFiscalPeriodAndYear, new string[] { "in-fiscal-period-and-year" } },
			{ ConditionOperator.InOrBeforeFiscalPeriodAndYear, new string[] { "in-or-before-fiscal-period-and-year" } },
			{ ConditionOperator.InOrAfterFiscalPeriodAndYear, new string[] { "in-or-after-fiscal-period-and-year" } },
			{ ConditionOperator.EqualUserTeams, new string[] { "eq-userteams" } },
			{ ConditionOperator.AboveOrEqual, new string[] { "eq-or-above" } },
			{ ConditionOperator.UnderOrEqual, new string[] { "eq-or-under" } },
};

		/// <summary>
		/// Returns the Key from KeyValuePair on passing value as input
		/// </summary>
		/// <param name="value">Takes value as input and returns Key</param>
		/// <returns>returns Key</returns>
		public static ConditionOperator GetKeyByValue(string value)
		{
			var keyValue = ConditionOperatorToTextFromLookUp.First(pair => pair.Value.Contains(value));
			return keyValue.Key;
		}

		/// <summary>
		/// Returns the Value from KeyValuePair on passing key as input
		/// </summary>
		/// <param name="key">Takes key as input and returns Value</param>
		/// <returns>returns Value</returns>
		public static string GetValueByKey(ConditionOperator key)
		{
			var keyValue = ConditionOperatorToTextFromLookUp.First(pair => pair.Key == key);
			return keyValue.Value[0];
		}
	}
}
