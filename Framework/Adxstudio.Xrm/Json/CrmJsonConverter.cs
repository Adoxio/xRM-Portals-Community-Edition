/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Json
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Metadata.Query;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using Services.Query;

	/// <summary>
	/// A Json.NET converter for CRM SDK objects.
	/// </summary>
	public class CrmJsonConverter : Newtonsoft.Json.JsonConverter
	{
		/// <summary>
		/// The concrete types that can be converted.
		/// </summary>
		internal static readonly Type[] CanConvertTypes =
		{
			typeof(Guid),
			typeof(EntityFilters),
			typeof(Condition),
		};

		/// <summary>
		/// The base types that can be converted.
		/// </summary>
		internal static readonly Type[] CanConvertBaseTypes =
		{
			typeof(DataCollection<string, object>),
			typeof(DataCollection<string, string>),
			typeof(DataCollection<Relationship, EntityCollection>),
			typeof(DataCollection<Relationship, QueryBase>),
			typeof(DataCollection<DeletedMetadataFilters, DataCollection<Guid>>),
			typeof(DataCollection<object>),
		};

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
		public override bool CanConvert(Type objectType)
		{
			if (CanConvertTypes.Any(type => type == objectType))
			{
				 return true;
			}

			if (CanConvertBaseTypes.Any(type => type.IsAssignableFrom(objectType)))
			{
				 return true;
			}

			return false;
		}

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// convert from actual type to surrogate type

			var condition = value as Condition;

			if (condition != null)
			{
				var jcondition = JsonCondition.Parse(condition);
				serializer.Serialize(writer, jcondition);
			}

			if (value is Guid)
			{
				serializer.Serialize(writer, new JsonGuid { Value = value.ToString() });
			}

			if (value is EntityFilters)
			{
				serializer.Serialize(writer, new JsonEntityFilters { Value = (int)value });
			}

			CrmJsonConverter.Serialize(writer, value as DataCollection<string, object>, serializer);
			CrmJsonConverter.Serialize(writer, value as DataCollection<string, string>, serializer);
			CrmJsonConverter.Serialize(writer, value as DataCollection<Relationship, EntityCollection>, serializer);
			CrmJsonConverter.Serialize(writer, value as DataCollection<Relationship, QueryBase>, serializer);
			CrmJsonConverter.Serialize(writer, value as DataCollection<DeletedMetadataFilters, DataCollection<Guid>>, serializer);
			CrmJsonConverter.Serialize(writer, value as ICollection<object>, serializer);
		}

		/// <summary>
		/// Serializes a collection.
		/// </summary>
		/// <typeparam name="TKey">The key type.</typeparam>
		/// <typeparam name="TValue">The value type.</typeparam>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		private static void Serialize<TKey, TValue>(JsonWriter writer, DataCollection<TKey, TValue> value, JsonSerializer serializer)
		{
			if (value != null)
			{
				if (typeof(TKey) == typeof(Relationship))
				{
					// serialize complex keys as lists instead
					var surrogate = new JsonList<KeyValuePair<TKey, TValue>> { Value = value.ToList() };
					serializer.Serialize(writer, surrogate);
				}
				else
				{
					var surrogate = value.ToDictionary(pair => pair.Key, pair => pair.Value);
					serializer.Serialize(writer, surrogate);
				}
			}
		}

		/// <summary>
		/// Serializes a collection.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		private static void Serialize(JsonWriter writer, ICollection<object> value, JsonSerializer serializer)
		{
			if (value != null)
			{
				var surrogate = value.ToList();
				serializer.Serialize(writer, surrogate);
			}
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// convert from surrogate type to actual type

			if (existingValue is Guid)
			{
				var id = serializer.Deserialize<JsonGuid>(reader);
				return new Guid(id.Value);
			}

			if (existingValue is EntityFilters)
			{
				var filters = serializer.Deserialize<JsonEntityFilters>(reader);
				return (EntityFilters)filters.Value;
			}

			if (objectType == typeof(Condition))
			{
				var jcondition = serializer.Deserialize<JsonCondition>(reader);
				return jcondition.ToCondition(Deserialize);
			}

			CrmJsonConverter.Deserialize(reader, existingValue as DataCollection<string, object>, serializer);
			CrmJsonConverter.Deserialize(reader, existingValue as DataCollection<string, string>, serializer);
			CrmJsonConverter.Deserialize(reader, existingValue as DataCollection<Relationship, EntityCollection>, serializer);
			CrmJsonConverter.Deserialize(reader, existingValue as DataCollection<Relationship, QueryBase>, serializer);
			CrmJsonConverter.Deserialize(reader, existingValue as DataCollection<DeletedMetadataFilters, DataCollection<Guid>>, serializer);
			CrmJsonConverter.Deserialize(reader, existingValue as ICollection<object>, serializer);

			return existingValue;
		}

		/// <summary>
		/// Deserializes a collection.
		/// </summary>
		/// <typeparam name="TKey">The key type.</typeparam>
		/// <typeparam name="TValue">The value type.</typeparam>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		private static void Deserialize<TKey, TValue>(JsonReader reader, DataCollection<TKey, TValue> existingValue, JsonSerializer serializer)
		{
			if (existingValue != null)
			{
				// scan for object references where surrogate conversion is not done automatically

				var surrogate = typeof(TKey) == typeof(Relationship)
					? serializer.Deserialize<JsonList<KeyValuePair<TKey, TValue>>>(reader).Value.Select(Deserialize).ToList()
					: serializer.Deserialize<Dictionary<TKey, TValue>>(reader).Select(Deserialize).ToList();

				// merge the surrogate with the result object

				foreach (var pair in surrogate)
				{
					existingValue[pair.Key] = pair.Value;
				}
			}
		}

		/// <summary>
		/// Deserializes a KeyValuePair.
		/// </summary>
		/// <typeparam name="TKey">The key type.</typeparam>
		/// <typeparam name="TValue">The value type.</typeparam>
		/// <param name="pair">The pair.</param>
		/// <returns>The deserialized pair.</returns>
		private static KeyValuePair<TKey, TValue> Deserialize<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		{
			// scan for object references where surrogate conversion is not done automatically

			var alias = pair.Value as AliasedValue;

			if (alias != null)
			{
				return new KeyValuePair<TKey, TValue>(pair.Key, (TValue)(object)new AliasedValue(alias.EntityLogicalName, alias.AttributeLogicalName, Deserialize(alias.Value)));
			}

			return new KeyValuePair<TKey, TValue>(pair.Key, (TValue)Deserialize(pair.Value));
		}

		/// <summary>
		/// Deserializes a collection.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		private static void Deserialize(JsonReader reader, ICollection<object> existingValue, JsonSerializer serializer)
		{
			if (existingValue != null)
			{
				// scan for object references where surrogate conversion is not done automatically

				var surrogate = serializer.Deserialize<List<object>>(reader).Select(Deserialize);

				foreach (var item in surrogate)
				{
					existingValue.Add(item);
				}
			}
		}

		/// <summary>
		/// Deserializes a surrogate object.
		/// </summary>
		/// <param name="value">The surrogate.</param>
		/// <returns>The object.</returns>
		private static object Deserialize(object value)
		{
			// convert from surrogate type to actual type

			if (value is long)
			{
				// for object references, the default json numeric type is long but the end result should be int

				return Convert.ToInt32(value);
			}

			if (value is JsonGuid)
			{
				return new Guid(((JsonGuid)value).Value);
			}

			if (value is JsonEntityFilters)
			{
				return (EntityFilters)((JsonEntityFilters)value).Value;
			}

			if (value is JsonList<KeyValuePair<Relationship, QueryBase>>)
			{
				var dictionary = new RelationshipQueryCollection();
				dictionary.AddRange(((JsonList<KeyValuePair<Relationship, QueryBase>>)value).Value);
				return dictionary;
			}

			return value;
		}
	}
}
