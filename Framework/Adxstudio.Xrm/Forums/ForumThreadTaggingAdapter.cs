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

namespace Adxstudio.Xrm.Forums
{
	/// <summary>
	/// Provides an implementation of <see cref="ITaggable"/> targeting a given <see cref="ForumThread"/>, using
	/// either a provided or implicit <see cref="OrganizationServiceContext"/>.
	/// </summary>
	public class ForumThreadTaggingAdapter : ITaggable
	{
		public ForumThreadTaggingAdapter(Entity taggableForumThread, string portalName)
		{
			if (taggableForumThread == null)
			{
				throw new ArgumentNullException("taggableForumThread");
			}

			taggableForumThread.AssertEntityName("adx_communityforumthread");

			ForumThread = taggableForumThread;
			PortalName = portalName;
			ServiceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public Entity ForumThread { get; private set; }

		public string PortalName { get; private set; }
		
		public OrganizationServiceContext ServiceContext { get; private set; }

		public IEnumerable<Entity> Tags
		{
			get
			{
				var forumThread = ServiceContext.CreateQuery(ForumThread.LogicalName).Single(t => t.GetAttributeValue<Guid>("adx_communityforumthreadid") == ForumThread.Id);

				return forumThread.GetRelatedEntities(ServiceContext, "adx_forumthreadtag_forumthread");
			}
		}

		/// <summary>
		/// Adds a tag association by name to <see cref="ForumThread"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be associated with the page (will be created if necessary).
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void AddTag(string tagName)
		{
			ServiceContext.AddTagToForumThreadAndSave(ForumThread.Id, tagName);
		}

		/// <summary>
		/// Removes a tag association by name from <see cref="ForumThread"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated from the page.
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void RemoveTag(string tagName)
		{
			ServiceContext.RemoveTagFromForumThreadAndSave(ForumThread.Id, tagName);
		}
	}
}
