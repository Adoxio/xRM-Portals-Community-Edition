/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SettingsDrop : Drop
	{
		private readonly ISettingDataAdapter _settings;

		public SettingsDrop(ISettingDataAdapter settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
		}

		public override object BeforeMethod(string method)
		{
			return method == null ? null : _settings.GetValue(method);
		}
	}
}
