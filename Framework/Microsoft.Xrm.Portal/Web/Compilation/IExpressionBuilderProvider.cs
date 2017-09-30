/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.CodeDom;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	public interface IExpressionBuilderProvider
	{
		object Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix);
	}

	internal static class NameValueCollectionExtensions
	{
		public static string GetValueByIndexOrName(this NameValueCollection collection, int index, string name)
		{
			if (collection.AllKeys.Contains(name)) return collection[name];
			if (collection.Count > index && collection.AllKeys[index] == "_{0}".FormatWith(index)) return collection[index];
			return null;
		}

		public static string[] GetValuesByIndexOrName(this NameValueCollection collection, int index, string name)
		{
			if (collection.AllKeys.Contains(name)) return collection.GetValues(name);
			if (collection.Count > index && collection.AllKeys[index] == "_{0}".FormatWith(index)) return collection.GetValues(index);
			return null;
		}
	}

	public class CrmExpressionBuilder<T> : ExpressionBuilder where T : IExpressionBuilderProvider, new()
	{
		private static readonly IExpressionBuilderProvider _builder = new T();

		public override bool SupportsEvaluate
		{
			get { return true; }
		}

		public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			return GetEvalData(entry.Expression, target.GetType(), entry.Name, entry.ExpressionPrefix);
		}

		public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			var codeExpressions = new CodeExpression[]
			{
				new CodePrimitiveExpression(entry.Expression.Trim()),
				new CodeTypeOfExpression(entry.DeclaringType),
				new CodePrimitiveExpression(entry.Name),
				new CodePrimitiveExpression(entry.ExpressionPrefix),
			};

			return new CodeCastExpression(
				TypeDescriptor.GetProperties(entry.DeclaringType)[entry.PropertyInfo.Name].PropertyType,
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(GetType()), "GetEvalData", codeExpressions));
		}

		protected static Type GetReturnType(Type controlType, string propertyName)
		{
			return controlType.GetProperty(propertyName).PropertyType;
		}

		protected static object GetEvalData(object value, string attributeName = null, string eval = null, string format = null, Type returnType = null)
		{
			object result;
			var entity = value as Entity;

			if (entity != null && attributeName != null)
			{
				result = entity.GetAttributeValue<object>(attributeName);
			}
			else
			{
				result = value;
			}

			if (eval != null)
			{
				result = format != null ? DataBinder.Eval(result, eval, format) : DataBinder.Eval(result, eval);
			}
			else if (format != null)
			{
				result = format.FormatWith(result);
			}

			var entityresult = result as Entity;

			if (returnType != null && entityresult != null)
			{
				if (returnType.IsA(typeof(Entity))) return entityresult;
				if (returnType == typeof(Guid)) return entityresult.Id;
				if (returnType == typeof(EntityReference)) return entityresult.ToEntityReference();
			}

			return result;
		}

		public static object GetEvalData(string expression, Type controlType, string propertyName, string expressionPrefix)
		{
			var arguments = ParseExpression(expression);
			return _builder.Evaluate(arguments, controlType, propertyName, expressionPrefix);
		}

		protected static object GetEvalData(
			string propertyName,
			string attributeName,
			string eval,
			string format,
			string defaultValue,
			string portalName,
			Type returnType)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);

			if (propertyName == "Current") return portal;
			var entity = DataBinder.Eval(portal, propertyName);

			if (entity != null)
			{
				var value = GetEvalData(entity, attributeName == "Current" ? null : attributeName, eval, format, returnType);
				return value == null && returnType == typeof(string) ? defaultValue : value;
			}

			if (returnType == typeof(string)) return defaultValue;

			return null;
		}

		protected static void ThrowArgumentException(string propertyName, string expressionPrefix, string usage)
		{
			throw new ArgumentException("Invalid arguments for the expression builder applied to the '{0}' property. Usage: {1}: {2}".FormatWith(propertyName, expressionPrefix, usage));
		}

		private static NameValueCollection ParseExpression(string expression)
		{
			var results = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
			var pairs = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(arg => arg.Trim().Split('='));
			var index = 0;

			foreach (var pair in pairs)
			{
				if (pair.Length > 1)
				{
					results.Add(HttpUtility.UrlDecode(pair[0]), HttpUtility.UrlDecode(pair[1]));
				}
				else if (pair.Length == 1)
				{
					results.Add("_{0}".FormatWith(index), HttpUtility.UrlDecode(pair[0]));
				}

				++index;
			}

			return results;
		}
	}
}
