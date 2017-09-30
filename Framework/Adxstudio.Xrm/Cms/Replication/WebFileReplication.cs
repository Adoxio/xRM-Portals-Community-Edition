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
	/// Replication of a Web File (adx_webfile)
	/// </summary>
	public class WebFileReplication : CrmEntityReplication
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="source">Entity</param>
		/// <param name="context">Organization Service Context</param>
		public WebFileReplication(Entity source, OrganizationServiceContext context) : base(source, context, "adx_webfile") { }

		public override void Created()
		{
			var source = Context.CreateQuery("adx_webfile").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_webfileid") == Source.Id);

			if (source == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity is null.");
				return;
			}

			// Get the parent page, so we can find any subscriber pages of that page, and generate
			// new children.
			var parentPage = source.GetRelatedEntity(Context, "adx_webpage_webfile");

			// If there's no parent page, we'll currently do nothing. (Maybe create a root file?)
			if (parentPage == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity has no parent page; ending replication.");
				return;
			}

			// Get the pages subscribed to the parent.
			var subscribedPages = parentPage.GetRelatedEntities(Context, "adx_webpage_masterwebpage", EntityRole.Referenced).ToList();

			if (!subscribedPages.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity parent page has no subscribed pages; ending replication.");
				return;
			}

			var replicatedFiles = subscribedPages.Select(subscribedPage =>
			{
				var replicatedFile = Source.Clone(false);

				replicatedFile.EntityState = null;
				replicatedFile.Id = Guid.Empty;
				replicatedFile.Attributes.Remove("adx_webfileid");
				replicatedFile.SetAttributeValue("adx_websiteid", subscribedPage.GetAttributeValue("adx_websiteid"));
				replicatedFile.SetAttributeValue("adx_masterwebfileid", new EntityReference(Source.LogicalName, Source.Id));
				replicatedFile.SetAttributeValue("adx_parentpageid", new EntityReference(Source.LogicalName, subscribedPage.Id));

				var publishingState = GetDefaultPublishingState(subscribedPage.GetAttributeValue<EntityReference>("adx_websiteid"));

				if (publishingState != null)
				{
					replicatedFile.SetAttributeValue("adx_publishingstateid", publishingState);
				}
				
				return replicatedFile;
			});

			foreach (var file in replicatedFiles)
			{
				Context.AddObject(file);
			}

			Context.SaveChanges();

			//ExecuteWorkflowOnSourceAndSubscribers("Created");
		}

		public override void Deleted()
		{
			//ExecuteWorkflowOnSourceAndSubscribers("Deleted");
		}

		public override void Updated()
		{
			//ExecuteWorkflowOnSourceAndSubscribers("Updated");
		}

		protected void ExecuteWorkflowOnSourceAndSubscribers(string eventName)
		{
			ExecuteWorkflowOnSourceAndSubscribers("Web File", eventName, "adx_webfile_masterwebfile", EntityRole.Referencing, "adx_webfileid");
		}
	}
}
