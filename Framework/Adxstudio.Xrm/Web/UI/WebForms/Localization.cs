/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Json;
	using System.Text;
	using System.Threading;
	using System.Globalization;
	using System.Web;
	using Adxstudio.Xrm.Globalization;

	/// <summary>
	/// Class used to localize a resourse string.
	/// </summary>
	public static class Localization
	{
		/// <summary>
		/// Get the Base Language Code for the Organization
		/// </summary>
		/// <param name="context">Organization Service Context used to retrieve the organization entities.</param>
		/// <returns>Language Code</returns>
		public static int RetrieveOrganizationBaseLanguageCode(HttpContext context)
		{
			return context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;
		}

		/// <summary>
		/// Gets a localized string from a JSON string containing language resources.
		/// </summary>
		/// <param name="json">String containing the JSON object array of language resources</param>
		/// <param name="languageCode">Language code used to retrieve the localized string</param>
		/// <returns></returns>
		public static string GetLocalizedString(string json, int languageCode)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return string.Empty;
			}

			if (!json.StartsWith(@"[{""LCID"":"))
			{
				return json;
			}

			var resources = ConvertJsonStringToList(json);

			if (resources.Count <= 0)
			{
				return string.Empty;
			}

			var localizedString = string.Empty;

			foreach (var res in resources)
			{
				if (res.LCID == languageCode)
				{
					localizedString = res.Value;
					break;
				}
			}

			return localizedString;
		}

		/// <summary>
		/// Gets a localized string from a list containing language resources.
		/// </summary>
		/// <param name="languageResources">List of language resources</param>
		/// <param name="languageCode">Language code used to retrieve the localized string</param>
		public static string GetLocalizedString(List<LanguageResources> languageResources, int languageCode)
		{
			if (languageResources == null)
			{
				return string.Empty;
			}

			var resource = languageResources.FirstOrDefault(l => l.LCID == languageCode);

			return resource == null ? string.Empty : resource.Value;
		}

		/// <summary>
		/// Gets a localized string from a list containing language resources.
		/// </summary>
		/// <param name="languageResources">List of language resources</param>
		/// <param name="languageCode">Language code used to retrieve the localized string</param>
		public static Lazy<string> CreateLazyLocalizedString(List<LanguageResources> languageResources, int languageCode)
		{
			return new Lazy<string>(() =>
			{
				var localized = GetLocalizedString(languageResources, languageCode);

				return string.IsNullOrEmpty(localized) ? null : localized;

			}, LazyThreadSafetyMode.None);
		}

		/// <summary>
		/// Returns full name respectfully to user localization
		/// </summary>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <returns> localized full name string</returns>
		public static string LocalizeFullName(string firstName, string lastName)
		{
			return IsWesternType() ? string.Join(" ", firstName, lastName) : string.Join(" ", lastName, firstName);
		}

		/// <summary>
		/// Determines type of localization for Full name.
		/// </summary>
		/// <returns></returns>
		public static bool IsWesternType()
		{
			return CultureInfo.CurrentUICulture.LCID != LocaleIds.Japanese && CultureInfo.CurrentUICulture.LCID != LocaleIds.Hebrew;
		}

		/// <summary>
		/// Returns full name respectfully to user localization
		/// </summary>
		/// <param name="firstNameInput"></param>
		/// <param name="lastNameInput"></param>
		/// <returns> localized full name string</returns>
		public static string LocalizeFullName(object firstNameInput, object lastNameInput)
		{
			var firstName = firstNameInput != null ? firstNameInput.ToString() : string.Empty;
			var lastName = lastNameInput != null ? lastNameInput.ToString() : string.Empty;

			return LocalizeFullName(firstName, lastName);
		}

		private static string ConvertListToJsonString(List<LanguageResources> languageResources)
		{
			var stream = new MemoryStream();
			var serialiser = new DataContractJsonSerializer(typeof(List<LanguageResources>));

			serialiser.WriteObject(stream, languageResources);

			var json = Encoding.Default.GetString(stream.ToArray());

			stream.Close();

			return json;
		}

		private static List<LanguageResources> ConvertJsonStringToList(string json)
		{
			List<LanguageResources> result = null;

			if (!string.IsNullOrWhiteSpace(json))
			{
				var byteArray = Encoding.Unicode.GetBytes(json);
				var stream = new MemoryStream(byteArray);
				var serialiser = new DataContractJsonSerializer(typeof(List<LanguageResources>));

				result = serialiser.ReadObject(stream) as List<LanguageResources>;

				stream.Close();
			}

			return result;
		}
	}
}
