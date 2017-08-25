/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// Provides an implementation of <see cref="ITaggable"/> targeting a given <see cref="WebPage"/>, using
	/// either a provided or implicit <see cref="OrganizationServiceContext"/>.
	/// </summary>
	public class WebPageTaggingAdapter : ITaggable
	{
		public WebPageTaggingAdapter(Entity taggableWebPage, string portalName)
		{
			if (taggableWebPage == null)
			{
				throw new ArgumentNullException("taggableWebPage");
			}

			taggableWebPage.AssertEntityName("adx_webpage");

			WebPage = taggableWebPage;
			PortalName = portalName;
			ServiceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public string PortalName { get; private set; }

		public OrganizationServiceContext ServiceContext { get; private set; }

		public IEnumerable<Entity> Tags
		{
			get
			{
				var webPage = ServiceContext.CreateQuery(WebPage.LogicalName).Single(wp => wp.GetAttributeValue<Guid>("adx_webpageid") == WebPage.Id);

				return webPage.GetRelatedEntities(ServiceContext, "adx_pagetag_webpage");
			}
		}

		public Entity WebPage { get; private set; }

		/// <summary>
		/// Adds a tag association by name to <see cref="WebPage"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be associated with the page (will be created if necessary).
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void AddTag(string tagName)
		{
			ServiceContext.AddTagToWebPageAndSave(WebPage.Id, tagName);
		}

		/// <summary>
		/// Removes a tag association by name from <see cref="WebPage"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated from the page.
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void RemoveTag(string tagName)
		{
			ServiceContext.RemoveTagFromWebPageAndSave(WebPage.Id, tagName);
		}
	}
}
