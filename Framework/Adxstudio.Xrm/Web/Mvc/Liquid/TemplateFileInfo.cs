/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class TemplateFileInfo
	{
		private string _title;

		public TemplateFileInfo(string name, JObject metadata = null)
		{
			if (name == null) throw new ArgumentNullException("name");

			Name = name;
			Metadata = metadata ?? new JObject();

			Title = GetMetadataValue<string>("title");
			Description = GetMetadataValue<string>("description");
			DefaultArguments = GetMetadataValue<string>("default_arguments");
		}

		public string DefaultArguments { get; set; }

		public string Description { get; set; }

		public string Name { get; private set; }

		public string Title
		{
			get { return _title ?? Name; }
			set { _title = value; }
		}

		protected JObject Metadata { get; private set; }

		private T GetMetadataValue<T>(string key)
		{
			JToken token;

			if (!Metadata.TryGetValue(key, StringComparison.InvariantCultureIgnoreCase, out token))
			{
				return default(T);
			}

			try
			{
				return token.Value<T>();
			}
			catch (InvalidCastException)
			{
				return default(T);
			}
		}
	}
}
