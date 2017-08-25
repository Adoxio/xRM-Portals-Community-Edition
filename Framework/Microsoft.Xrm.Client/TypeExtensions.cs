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
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Type class related extension methods and utilities.
	/// </summary>
	/// <exclude/>
	public static class TypeExtensions
	{
		/// <summary>
		/// Retrieves a type object by name and by searching through all available assemblies.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static Type GetType(string typeName)
		{
			Type type = Type.GetType(typeName);

			if (type != null)
			{
				return type;
			}

			if (typeName.Contains(","))
			{
				// assume a comma means that the assembly name is explicity specified
				return null;
			}

			// the specified type is not in mscorlib so we will go through the loaded assemblies
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				type = Type.GetType(typeName + ", " + assembly.FullName);

				if (type != null)
				{
					return type;
				}
			}

			return null;
		}

		/// <summary>
		/// Retrieves the underlying type if the type is nullable, otherwise returns the current type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetUnderlyingType(this Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}

		/// <summary>
		/// Determines if a generic type is assignable from this type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsA<T>(this Type type)
		{
			return IsA(type, typeof(T));
		}

		/// <summary>
		/// Determines if the input reference type is assignable from this type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="referenceType"></param>
		/// <returns></returns>
		public static bool IsA(this Type type, Type referenceType)
		{
			return referenceType != null ? referenceType.IsAssignableFrom(type) : false;
		}

		#region Custom Attributes Members

		private struct ClassTypeAttributeType
		{
			public Type ClassType;
			public Type Type;
		}

		private static readonly ConcurrentDictionary<ClassTypeAttributeType, IEnumerable<object>> _typeToAttributesLookup = new ConcurrentDictionary<ClassTypeAttributeType, IEnumerable<object>>();

		/// <summary>
		/// Returns a collection of custom attributes for a given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this Type type) where T : Attribute
		{
			var pair = new ClassTypeAttributeType { ClassType = type, Type = typeof(T) };

			IEnumerable<object> attributes;

			if (!_typeToAttributesLookup.TryGetValue(pair, out attributes))
			{
				attributes = type.GetCustomAttributes(typeof(T), true);
				_typeToAttributesLookup[pair] = attributes;
			}

			return attributes.Select(attribute => attribute as T);
		}

		/// <summary>
		/// Returns the first custom attribute of a given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T GetFirstOrDefaultCustomAttribute<T>(this Type type) where T : Attribute
		{
			return GetCustomAttributes<T>(type).FirstOrDefault();
		}

		/// <summary>
		/// Returns the logical name value of the <see cref="EntityLogicalNameAttribute"/> attribute associated to the class type.
		/// </summary>
		/// <param name="type">The type of a custom <see cref="Entity"/>.</param>
		/// <returns></returns>
		public static string GetEntityLogicalName(this Type type)
		{
			var attribute = type.GetFirstOrDefaultCustomAttribute<EntityLogicalNameAttribute>();

			if (attribute == null)
			{
				throw new ArgumentException("Unable to determine the 'LogicalName' from the '{0}' type.".FormatWith(type), "type");
			}

			return attribute.LogicalName;
		}

		#endregion
	}
}
