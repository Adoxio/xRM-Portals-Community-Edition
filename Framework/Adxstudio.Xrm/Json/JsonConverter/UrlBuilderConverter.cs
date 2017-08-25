/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Json.JsonConverter
{
	/// <summary>
	/// Convert a UrlBuilder from JSON
	/// </summary>
	public class UrlBuilderConverter : Newtonsoft.Json.JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(UrlBuilder) == objectType;
		}

		public override bool CanWrite { get { return false; } }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;

			var jObject = JObject.Load(reader);

			var uri = jObject["Uri"];

			return uri == null ? null : new UrlBuilder(uri.ToString());
		}
	}
}
