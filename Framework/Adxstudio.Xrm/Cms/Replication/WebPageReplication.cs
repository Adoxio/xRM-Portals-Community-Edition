/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Replication of a Web Page (adx_webpage)
	/// </summary>
	public class WebPageReplication : CrmEntityReplication
	{
		/// <summary>
		/// WebPageReplication class initialization
		/// </summary>
		/// <param name="source">Web Page</param>
		/// <param name="context">Organization Service Context</param>
		public WebPageReplication(Entity source, OrganizationServiceContext context) : base(source, context, "adx_webpage") { }

		/// <summary>
		/// Entity Created Event
		/// </summary>
		public override void Created()
		{
			var source = Context.CreateQuery("adx_webpage").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_webpageid") == Source.Id);

			if (source == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity is null.");
				return;
			}

			// Get the parent page, so we can find any subscriber pages of that page, and generate new children.

			var parentPage = source.GetRelatedEntity(Context, "adx_webpage_webpage", EntityRole.Referencing);

			// If there's no parent page, we'll currently do nothing. (Maybe create a root page?)
			if (parentPage == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity has no parent page; ending replication.");
				return;
			}

			// Get the pages subscribed to the parent.
			var subscribedPages = parentPage.GetRelatedEntities(Context, "adx_webpage_masterwebpage", EntityRole.Referenced).ToList();

			if (!subscribedPages.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity parent has no subscribed pages; ending replication.");
				return;
			}

			var replicatedPages = subscribedPages.Select(subscribedPage =>
			{
				var replicatedPage = source.Clone(false);
				
				replicatedPage.EntityState = null;
				replicatedPage.Id = Guid.Empty;
				replicatedPage.Attributes.Remove("adx_webpageid");
				replicatedPage.SetAttributeValue("adx_websiteid", subscribedPage.GetAttributeValue("adx_websiteid"));
				replicatedPage.SetAttributeValue("adx_masterwebpageid", new EntityReference(Source.LogicalName, Source.Id));
				replicatedPage.SetAttributeValue("adx_parentpageid", new EntityReference(Source.LogicalName, subscribedPage.Id));

				var pageTemplate = GetMatchingPageTemplate(Source.GetAttributeValue<EntityReference>("adx_pagetemplateid"), subscribedPage.GetAttributeValue<EntityReference>("adx_websiteid"));

				if (pageTemplate != null)
				{
					replicatedPage.SetAttributeValue("adx_pagetemplateid", pageTemplate);
				}

				var publishingState = GetDefaultPublishingState(subscribedPage.GetAttributeValue<EntityReference>("adx_websiteid"));

				if (publishingState != null)
				{
					replicatedPage.SetAttributeValue("adx_publishingstateid", publishingState);
				}

				return replicatedPage;
			});

			foreach (var page in replicatedPages)
			{
				Context.AddObject(page);
			}

			Context.SaveChanges();

			//ExecuteWorkflowOnSourceAndSubscribers("Created");
		}

		/// <summary>
		/// Entity Deleted Event
		/// </summary>
		public override void Deleted()
		{
			//ExecuteWorkflowOnSourceAndSubscribers("Deleted");
		}

		/// <summary>
		/// Entity Updated Event
		/// </summary>
		public override void Updated()
		{
			//ExecuteWorkflowOnSourceAndSubscribers("Updated");
		}

		protected void ExecuteWorkflowOnSourceAndSubscribers(string eventName)
		{
			ExecuteWorkflowOnSourceAndSubscribers("Web Page", eventName, "adx_webpage_masterwebpage", EntityRole.Referencing, "adx_webpageid");
		}

		private EntityReference GetMatchingPageTemplate(EntityReference sourcePageTemplateReference, EntityReference website)
		{
			if (sourcePageTemplateReference == null || website == null)
			{
				return null;
			}

			var sourcePageTemplate = Context.CreateQuery("adx_pagetemplate").FirstOrDefault(p => p.GetAttributeValue<Guid?>("adx_pagetemplateid") == sourcePageTemplateReference.Id);

			if (sourcePageTemplate == null)
			{
				return null;
			}

			var sourcePageTemplateName = sourcePageTemplate.GetAttributeValue<string>("adx_name");

			var targetPageTemplate = Context.CreateQuery("adx_pagetemplate").FirstOrDefault(p => p.GetAttributeValue<string>("adx_name") == sourcePageTemplateName && p.GetAttributeValue<EntityReference>("adx_websiteid") == website);

			return targetPageTemplate == null ? null : targetPageTemplate.ToEntityReference();
		}
	}
}
