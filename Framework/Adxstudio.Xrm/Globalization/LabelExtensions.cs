/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Globalization
{
	using System.Web;
	using System.Globalization;
	using System.Linq;
	using Microsoft.Xrm.Sdk;
	using Adxstudio.Xrm.Web;

	public static class LabelExtensions
	{
		/// <summary>
		/// Gets localized label for Currect Culture
		/// </summary>
		/// <param name="label"></param>
		/// <returns>Localized label for Currect Culture</returns>
		public static LocalizedLabel GetLocalizedLabel(this Label label)
		{
			var language = CultureInfo.CurrentCulture.LCID;

			if (HttpContext.Current != null)
			{
				var languageContext = HttpContext.Current.GetContextLanguageInfo();
				if (languageContext.IsCrmMultiLanguageEnabled)
				{
					language = languageContext.ContextLanguage.CrmLcid;
				}
			}

			var localizedLabel = label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == language);

			// If label for Current Culture wasn't found, use UserLocalizedLabel
			return localizedLabel == null ? label.UserLocalizedLabel : localizedLabel;
		}

		/// <summary>
		/// Gets localized label text for Currect Culture
		/// </summary>
		/// <param name="label"></param>
		/// <returns>Localized label text for Currect Culture</returns>
		public static string GetLocalizedLabelString(this Label label)
		{
			return label.GetLocalizedLabel().Label;
		}
	}
}
