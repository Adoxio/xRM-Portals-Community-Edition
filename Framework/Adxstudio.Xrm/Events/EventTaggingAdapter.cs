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

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Provides an implementation of <see cref="ITaggable"/> targeting a given <see cref="Event"/>, using
	/// either a provided or implicit <see cref="OrganizationServiceContext"/>.
	/// </summary>
	public class EventTaggingAdapter : ITaggable
	{
		public EventTaggingAdapter(Entity taggableEvent, string portalName)
		{
			if (taggableEvent == null)
			{
				throw new ArgumentNullException("taggableEvent");
			}

			taggableEvent.AssertEntityName("adx_event");

			Event = taggableEvent;
			PortalName = portalName;
			ServiceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public Entity Event { get; private set; }

		public string PortalName { get; private set; }

		public OrganizationServiceContext ServiceContext { get; private set; }

		public IEnumerable<Entity> Tags
		{
			get
			{
				var evnt = ServiceContext.CreateQuery(Event.LogicalName).Single(e => e.GetAttributeValue<Guid>("adx_eventid") == Event.Id);

				return evnt.GetRelatedEntities(ServiceContext, "adx_eventtag_event");
			}
		}

		/// <summary>
		/// Adds a tag association by name to <see cref="Event"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be associated with the page (will be created if necessary).
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void AddTag(string tagName)
		{
			ServiceContext.AddTagToEventAndSave(Event.Id, tagName);
		}

		/// <summary>
		/// Removes a tag association by name from <see cref="Event"/>, through <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated from the page.
		/// </param>
		/// <remarks>
		/// This operation will persist all changes.
		/// </remarks>
		public void RemoveTag(string tagName)
		{
			ServiceContext.RemoveTagFromEventAndSave(Event.Id, tagName);
		}
	}
}
