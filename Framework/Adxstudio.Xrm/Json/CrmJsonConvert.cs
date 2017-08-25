/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Json
{
	using Newtonsoft.Json;

	/// <summary>
	/// JSON serialization helpers.
	/// </summary>
	public static class CrmJsonConvert
	{
		/// <summary>
		/// Default serializer settings.
		/// </summary>
		private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Objects,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = new Newtonsoft.Json.JsonConverter[] { new CrmJsonConverter() },
			Binder = new CrmSerializationBinder()
		};

		/// <summary>
		/// Creates a serializer.
		/// </summary>
		/// <returns>The serializer.</returns>
		public static JsonSerializer CreateJsonSerializer()
		{
			return JsonSerializer.Create(SerializerSettings);
		}

		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <returns>The JSON string.</returns>
		public static string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value, SerializerSettings);
		}

		/// <summary>
		/// Deserializes the string.
		/// </summary>
		/// <param name="value">The JSON string.</param>
		/// <returns>The object.</returns>
		public static object DeserializeObject(string value)
		{
			return JsonConvert.DeserializeObject(value, SerializerSettings);
		}

		/// <summary>
		/// Deserializes the string.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="value">The JSON string.</param>
		/// <returns>The object.</returns>
		public static T DeserializeObject<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
		}

		/// <summary>
		/// Serializes the object to file.
		/// </summary>
		/// <param name="path">The filename path.</param>
		/// <param name="value">The object.</param>
		public static void SerializeFile(string path, object value)
		{
			using (var writer = System.IO.File.CreateText(path))
			using (var jtw = new JsonTextWriter(writer))
			{
				var serializer = JsonSerializer.Create(SerializerSettings);
				serializer.Serialize(jtw, value);
			}
		}

		/// <summary>
		/// Deserializes the string from a file.
		/// </summary>
		/// <param name="path">The filename path.</param>
		/// <returns>The object.</returns>
		public static object DeserializeFile(string path)
		{
			using (var reader = System.IO.File.OpenText(path))
			using (var jtr = new JsonTextReader(reader))
			{
				var serializer = JsonSerializer.Create(SerializerSettings);
				var value = serializer.Deserialize(jtr);
				return value;
			}
		}
	}
}
