/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	class GuidConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Guid) == objectType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
					return Guid.Empty;
				case JsonToken.String:
					var str = reader.Value as string;
					return string.IsNullOrEmpty(str) ? Guid.Empty : new Guid(reader.Value as string);
				default:
					throw new ArgumentException("Invalid token type");
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (Guid.Empty.Equals(value))
			{
				writer.WriteValue(string.Empty);
			}
			else
			{
				writer.WriteValue((Guid)value);
			}
		}
	}
}
