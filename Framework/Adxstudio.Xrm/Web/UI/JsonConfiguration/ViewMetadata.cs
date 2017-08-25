/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class ViewMetadata
	{
		public IEnumerable<View> Views { get; set; }

		public static ViewMetadata Parse(string json)
		{
			return JsonConvert.DeserializeObject<ViewMetadata>(
				json,
				new JsonSerializerSettings
				{
					ContractResolver = JsonConfigurationContractResolver.Instance,
					MissingMemberHandling = MissingMemberHandling.Ignore,
					TypeNameHandling = TypeNameHandling.Objects,
					Binder = new ActionSerializationBinder(),
					Converters = new List<JsonConverter>
					{
						new GuidConverter()
					}
				});
		}
	}
}
