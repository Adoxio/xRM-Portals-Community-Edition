/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Linq;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Sdk;

	internal class Snippet : ISnippet
	{
		public Snippet(Entity entity, IPortalViewEntity viewEntity, ContextLanguageInfo language)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (viewEntity == null)
			{
				throw new ArgumentNullException("viewEntity");
			}

			if (language == null)
			{
				throw new ArgumentNullException("language");
			}

			Entity = entity;
			Name = entity.GetAttributeValue<string>("adx_name");
			Value = viewEntity.GetAttribute("adx_value");
			DisplayName = entity.GetAttributeValue<string>("adx_display_name");

			// set the language name value - fails if the solutions have not been updated, so set LanguageName to null
			this.LanguageName = GetLanguageName(entity, language);
		}

		/// <summary>
		/// Gets the DisplayName attribute of the entity record
		/// </summary>
		public string DisplayName { get; private set; }

		public Entity Entity { get; private set; }

		public string Name { get; private set; }

		public IPortalViewAttribute Value { get; private set; }

		public string LanguageName { get; set; }

		private static string GetLanguageName(Entity entity, ContextLanguageInfo language)
		{
			if (language.IsCrmMultiLanguageEnabled)
			{
				var langReference = entity.GetAttributeValue<EntityReference>("adx_contentsnippetlanguageid");
				if (langReference != null)
				{
					var websiteLanguages = language.ActiveWebsiteLanguages.ToArray();
					var snippetLanguage = language.GetWebsiteLanguage(langReference.Id, websiteLanguages);

					if (snippetLanguage != null)
					{
						return snippetLanguage.DisplayName;
					}
				}
			}

			return null;
		}
	}
}
