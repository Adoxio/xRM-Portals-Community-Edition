/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adxstudio.Xrm.AspNet.Cms;

namespace Adxstudio.Xrm.Cms
{
	public class SettingDataAdapter : ISettingDataAdapter
	{
		private readonly CrmWebsite _website;
		public SettingDataAdapter(IDataAdapterDependencies dependencies, CrmWebsite website)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			if (website == null)
			{
				throw new ArgumentNullException("website");
			}

			Dependencies = dependencies;
			_website = website;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public IEnumerable<ISetting> Select()
		{
			var settings = _website.Settings;
			var allEntities = settings.Select(s => new Setting(s.Entity));

			return allEntities;
		}

		public ISetting Select(string settingName)
		{
			if (string.IsNullOrEmpty(settingName))
			{
				return null;
			}

			var entity = Select().FirstOrDefault(e => e.Name == settingName);

			return entity;
		}

		/// <summary>
		/// Mappings between possible string setting values, and their boolean equivalents.
		/// </summary>
		private static readonly IDictionary<string, bool> _booleanValueMappings = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "on", true },
			{ "enabled", true },
			{ "yes", true },
			{ "off", false },
			{ "disabled", false },
			{ "no", false },
		};

		public bool? GetBooleanValue(string settingName)
		{
			return SelectValue(settingName, value =>
			{
				if (value == null)
				{
					return null;
				}

				bool b;

				if (_booleanValueMappings.TryGetValue(value, out b))
				{
					return b;
				}

				return bool.TryParse(value, out b) ? new bool?(b) : null;
			});
		}

		public decimal? GetDecimalValue(string settingName)
		{
			return SelectValue(settingName, value =>
			{
				decimal d;

				return value != null && decimal.TryParse(value, out d) ? new decimal?(d) : null;
			});
		}

		public int? GetIntegerValue(string settingName)
		{
			return SelectValue(settingName, value =>
			{
				int i;

				return value != null && int.TryParse(value, out i) ? new int?(i) : null;
			});
		}

		public TimeSpan? GetTimeSpanValue(string settingName)
		{
			return SelectValue(settingName, value =>
			{
				TimeSpan ts;

				return value != null && TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out ts)
					? new TimeSpan?(ts)
					: null;
			});
		}

		public string GetValue(string settingName)
		{
			return SelectValue(settingName, value => value);
		}

		private T SelectValue<T>(string settingName, Func<string, T> parse)
		{
			var setting = Select(settingName);

			return setting == null ? default(T) : parse(setting.Value);
		}
	}
}
