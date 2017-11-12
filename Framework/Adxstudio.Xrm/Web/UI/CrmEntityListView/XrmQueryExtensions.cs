/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Extension methods to aid in building dynamic linq where clauses
	/// </summary>
	public static class XrmQueryExtensions
	{
		/// <summary>
		/// Build a dynamic where clause to filter savedquery views by a collection of returnedtypecode and name value pairs.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static IQueryable<Entity> FilterByNames(this IQueryable<Entity> query, List<Tuple<string, string>> values)
		{
			return !values.Any()
				? new List<Entity>().AsQueryable()
				: query.Where(ContainsPropertyValueEqual<Entity>("returnedtypecode", "name", values));
		}

		private static Expression<Func<TParameter, bool>> ContainsPropertyValueEqual<TParameter>(string propertyName1,
			string propertyName2, IEnumerable<Tuple<string, string>> values)
		{
			var parameterType = typeof(TParameter);

			var parameter = Expression.Parameter(parameterType, parameterType.Name.ToLowerInvariant());

			var expression = ContainsPropertyValueEqual(propertyName1, propertyName2, values, parameter);

			return Expression.Lambda<Func<TParameter, bool>>(expression, parameter);
		}

		private static Expression ContainsPropertyValueEqual(string propertyName1, string propertyName2,
			IEnumerable<Tuple<string, string>> values, ParameterExpression parameter)
		{
			var enumerable = values as IList<Tuple<string, string>> ?? values.ToList();
			var left = PropertyValueEqual(parameter, propertyName1, enumerable.Select(v => v.Item1).First());
			var right = PropertyValueEqual(parameter, propertyName2, enumerable.Select(v => v.Item2).First());
			var leftAnd = Expression.And(left, right);

			return ContainsPropertyValueEqual(propertyName1, propertyName2, enumerable.Skip(1), parameter, leftAnd);
		}

		private static Expression ContainsPropertyValueEqual(string propertyName1, string propertyName2,
			IEnumerable<Tuple<string, string>> values, ParameterExpression parameter, Expression expression)
		{
			var enumerable = values as IList<Tuple<string, string>> ?? values.ToList();

			if (!enumerable.Any())
			{
				return expression;
			}

			var orElse = Expression.OrElse(expression,
				ContainsPropertyValueEqual(propertyName1, propertyName2, enumerable, parameter));

			return ContainsPropertyValueEqual(propertyName1, propertyName2, enumerable.Skip(1), parameter, orElse);
		}

		private static Expression PropertyValueEqual(Expression parameter, string crmPropertyName, string value)
		{
			var methodCall = Expression.Call(parameter, "GetAttributeValue", new[] { typeof(string) },
				Expression.Constant(crmPropertyName));

			return Expression.Equal(methodCall, Expression.Constant(value));
		}
	}
}
