/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Renders the value (adx_value) property of a content snippet (adx_contentsnippet).
	/// </summary>
	public class Snippet : Property // MSBug #120116: Won't seal, Inheritance is expected extension point.
	{
		public string SnippetName { get; set; }

		protected override string GetEditDisplayName(Entity entity, string propertyName)
		{
			return SnippetName ?? base.GetEditDisplayName(entity, propertyName);
		}

		protected override void OnLoad(EventArgs args)
		{
			Entity snippet;

			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var context = portal.ServiceContext;
			var website = portal.Website;

			if (TryGetSnippetEntity(context, website, SnippetName, out snippet))
			{
				DataItem = snippet;

				if (string.IsNullOrEmpty(PropertyName))
				{
					EntitySetInfo entitySetInfo;

					if (OrganizationServiceContextInfo.TryGet(context, snippet, out entitySetInfo))
					{
						var attributeInfo = entitySetInfo.Entity.AttributesByLogicalName["adx_value"];
						PropertyName = attributeInfo.Property.Name;
					}
				}
			}
			else
			{
				DataItem = DefaultText;
			}

			base.OnLoad(args);
		}

		private static bool TryGetSnippetEntity(OrganizationServiceContext context, Entity website, string snippetName, out Entity snippet)
		{
			snippet = null;

			if (string.IsNullOrEmpty(snippetName))
			{
				return false;
			}

			snippet = context.GetSnippetByName(website, snippetName);

			return snippet != null;
		}
	}
}
