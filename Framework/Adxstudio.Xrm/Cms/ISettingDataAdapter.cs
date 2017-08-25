/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface ISettingDataAdapter
	{
		bool? GetBooleanValue(string settingName);

		decimal? GetDecimalValue(string settingName);

		int? GetIntegerValue(string settingName);

		TimeSpan? GetTimeSpanValue(string settingName);

		string GetValue(string settingName);

		ISetting Select(string settingName);

		IEnumerable<ISetting> Select();
	}

	internal class RequestCachingSettingDataAdapter : RequestCachingDataAdapter, ISettingDataAdapter
	{
		private readonly ISettingDataAdapter _settings;

		public RequestCachingSettingDataAdapter(ISettingDataAdapter settings, EntityReference website) : base("{0}:{1}".FormatWith(settings.GetType().FullName, website.Id))
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (website == null) throw new ArgumentNullException("website");

			_settings = settings;
		}

		public bool? GetBooleanValue(string settingName)
		{
			return Get("GetBooleanValue:" + settingName, () => _settings.GetBooleanValue(settingName));
		}

		public decimal? GetDecimalValue(string settingName)
		{
			return Get("GetDecimalValue:" + settingName, () => _settings.GetDecimalValue(settingName));
		}

		public int? GetIntegerValue(string settingName)
		{
			return Get("GetIntegerValue:" + settingName, () => _settings.GetIntegerValue(settingName));
		}

		public TimeSpan? GetTimeSpanValue(string settingName)
		{
			return Get("GetTimeSpanValue:" + settingName, () => _settings.GetTimeSpanValue(settingName));
		}

		public string GetValue(string settingName)
		{
			return Get("GetValue:" + settingName, () => _settings.GetValue(settingName));
		}

		public ISetting Select(string settingName)
		{
			return Get("Select:" + settingName, () => _settings.Select(settingName));
		}

		public IEnumerable<ISetting> Select()
		{
			return Get("Select", () => _settings.Select());
		}
	}
}
