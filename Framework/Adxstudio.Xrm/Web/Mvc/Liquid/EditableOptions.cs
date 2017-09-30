/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EditableOptions
	{
		public EditableOptions(IDictionary<string, object> attributes = null)
		{
			Attributes = attributes ?? new Dictionary<string, object>();

			object @class;

			if (Attributes.TryGetValue("class", out @class) && @class != null)
			{
				CssClass = @class.ToString();
			}

			object @default;

			if (Attributes.TryGetValue("default", out @default) && @default != null)
			{
				Default = @default.ToString();
			}

			object escape;

			if (Attributes.TryGetValue("escape", out escape) && escape != null)
			{
				Escape = GetBooleanAttributeValue(escape);
			}

			object liquid;

			if (Attributes.TryGetValue("liquid", out liquid) && liquid != null)
			{
				Liquid = GetBooleanAttributeValue(liquid);
			}

			object tag;

			if (Attributes.TryGetValue("tag", out tag) && tag != null)
			{
				Tag = tag.ToString();
			}

			object title;

			if (Attributes.TryGetValue("title", out title) && title != null)
			{
				Title = title.ToString();
			}

			object type;

			if (Attributes.TryGetValue("type", out type) && type != null)
			{
				Type = type.ToString();
			}
		}

		public IDictionary<string, object> Attributes { get; private set; }

		public string CssClass { get; private set; }

		public string Default { get; private set; }

		public bool? Escape { get; private set; }

		public bool? Liquid { get; private set; }

		public string Tag { get; private set; }

		public string Title { get; private set; }

		public string Type { get; private set; }

		private static bool? GetBooleanAttributeValue(object value)
		{
			if (value == null)
			{
				return null;
			}

			if (value is bool)
			{
				return (bool)value;
			}

			bool parsed;

			return bool.TryParse(value.ToString(), out parsed)
				? parsed
				: (bool?)null;
		}
	}
}
