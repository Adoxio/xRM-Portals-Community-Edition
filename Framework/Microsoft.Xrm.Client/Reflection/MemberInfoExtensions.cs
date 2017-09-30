/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Reflection
{
	internal static class MemberInfoExtensions
	{
		private struct MethodInfoAttributeType
		{
			public MemberInfo MethodInfo;
			public Type Type;
		}

		private static readonly ConcurrentDictionary<MethodInfoAttributeType, IEnumerable<object>> _memberInfoToAttributesLookup = new ConcurrentDictionary<MethodInfoAttributeType, IEnumerable<object>>();

		/// <summary>
		/// Returns a collection of custom attributes for a given member.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo info) where T : Attribute
		{
			var pair = new MethodInfoAttributeType { MethodInfo = info, Type = typeof(T) };

			if (!_memberInfoToAttributesLookup.ContainsKey(pair))
			{
				_memberInfoToAttributesLookup[pair] = info.GetCustomAttributes(typeof(T), true);
			}

			return _memberInfoToAttributesLookup[pair].Select(attribute => attribute as T);
		}

		/// <summary>
		/// Returns the first custom attribute of a given member.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public static T GetFirstOrDefaultCustomAttribute<T>(this MemberInfo info) where T : Attribute
		{
			return GetCustomAttributes<T>(info).FirstOrDefault();
		}

		/// <summary>
		/// Determines if the member is annotated by a given <see cref="T:System.Attribute"></see>.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="customAttributeType"></param>
		/// <returns></returns>
		public static bool ContainsCustomAttribute(this MemberInfo info, Type customAttributeType)
		{
			return info.GetCustomAttributes(customAttributeType, true).Count() > 0;
		}

		public static string GetCrmPropertyName(this MemberInfo property)
		{
			var attribute = property.GetFirstOrDefaultCustomAttribute<AttributeLogicalNameAttribute>();

			if (attribute == null)
			{
				throw new ArgumentException("Unable to determine the 'LogicalName' from the '{0}' member.".FormatWith(property), "property");
			}

			return attribute.LogicalName;
		}

		public static string GetCrmAssocationName(this PropertyInfo property)
		{
			var attribute = property.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

			if (attribute == null)
			{
				throw new ArgumentException("Unable to determine the 'SchemaName' from the '{0}' member.".FormatWith(property), "property");
			}

			return attribute.SchemaName;
		}
	}
}
