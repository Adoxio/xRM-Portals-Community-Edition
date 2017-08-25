/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Adxstudio.Xrm.Search.Analysis
{
	public class AppSettingSnowballAnalyzerFactory : SettingSnowballAnalyzerFactory
	{
		protected override bool TryGetSettingValue(string name, out string value)
		{
			value = ConfigurationManager.AppSettings[name];

			return !string.IsNullOrEmpty(value);
		}
	}
}
