/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Microsoft.Xrm.Client.Runtime.Serialization
{
	/// <exclude/>
	/// <summary>
	/// Object serialization methods.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Serializes an object using the <see cref="DataContractJsonSerializer"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static string SerializeByJson(this object obj, IEnumerable<Type> knownTypes)
		{
			return DataContractJsonSerialize(obj, obj.GetType(), knownTypes, CreateDataContractJsonSerializer);
		}

		/// <summary>
		/// Deserializes an object using the <see cref="DataContractJsonSerializer"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="type"></param>
		/// <param name="knownTypes"></param>
		/// <returns></returns>
		public static object DeserializeByJson(this string text, Type type, IEnumerable<Type> knownTypes)
		{
			return DataContractJsonDeserialize(text, type, knownTypes, CreateDataContractJsonSerializer);
		}

		private static string DataContractJsonSerialize(
			object value,
			Type type,
			IEnumerable<Type> knownTypes,
			Func<Type, IEnumerable<Type>, XmlObjectSerializer> create)
		{
			using (var ms = new MemoryStream())
			{
				var serializer = create(type, knownTypes);
				serializer.WriteObject(ms, value);
				ms.Position = 0;

				using (var reader = new StreamReader(ms))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static object DataContractJsonDeserialize(
			string text,
			Type type,
			IEnumerable<Type> knownTypes,
			Func<Type, IEnumerable<Type>, XmlObjectSerializer> create)
		{
			using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(text)))
			{
				var serializer = create(type, knownTypes);

				return serializer.ReadObject(ms);
			}
		}

		private static XmlObjectSerializer CreateDataContractJsonSerializer(Type type, IEnumerable<Type> knownTypes)
		{
			return new DataContractJsonSerializer(type, knownTypes, int.MaxValue, true, null, false);
		}
	}
}
