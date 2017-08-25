/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Class used to define language resources.
	/// </summary>
	[DataContract]
	[Serializable]
	public class LanguageResources
	{
		/// <summary>
		/// Language Code of the resource string.
		/// </summary>
		[DataMember]
		public int LCID { get; set; }

		/// <summary>
		/// Text of the resource string
		/// </summary>
		[DataMember]
		public string Value { get; set; }
	}

	/// <summary>
	/// Language Resource Extensions
	/// </summary>
	public static class LanguageResourcesExentions
	{
		/// <summary>
		/// Retrieves the string if there is a LanguageResource in the list that matches the LCID with the language code specified otherwise it returns an empty string.
		/// </summary>
		/// <param name="languageResources">List of <see cref="LanguageResources" /></param>
		/// <param name="languageCode">Language code used to retrieve the localized string. See http://msdn.microsoft.com/en-us/library/ms912047(WinEmbedded.10).aspx </param>
		public static string GetLocalizedString(this List<LanguageResources> languageResources, int languageCode)
		{
			if (languageResources == null)
			{
				return string.Empty;
			}

			var resource = languageResources.FirstOrDefault(l => l.LCID == languageCode);

			return resource == null ? string.Empty : resource.Value;
		}
	}
}
