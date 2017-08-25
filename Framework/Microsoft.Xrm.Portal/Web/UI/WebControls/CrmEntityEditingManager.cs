/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Coordinates rendering of hidden DOM metadata for a bound entity, to support XRM inline editing.
	/// </summary>
	public class CrmEntityEditingManager : EditableCrmEntityDataBoundControl
	{
		public string SiteMapProvider { get; set; }

		public override ITemplate ItemTemplate
		{
			set { throw new NotSupportedException("Use of ItemTemplate is not supported by this control."); }
		}

		protected override void PerformDataBindingOfCrmEntity(Entity entity)
		{
			CssClass = GetCssClass(entity);

			Attributes["style"] = "display:none;";

			if (HasEditPermission(entity))
			{
				var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityEditingMetadataProvider>();

				metadataProvider.AddEntityMetadata(PortalName, this, this, entity);

				this.RegisterClientSideDependencies();
			}
		}

		protected override void PerformDataBindingOfCrmEntityProperty(Entity entity, string propertyName, string value)
		{
			// Ignore any attempt at binding to properties. We are not concerned with those here.
			PerformDataBindingOfCrmEntity(entity);
		}

		private string GetCssClass(Entity entity)
		{
			var cssClasses = new List<string>
			{
				"xrm-entity",
				"xrm-editable-{0}".FormatWith(entity.LogicalName)
			};

			if (!string.IsNullOrEmpty(CssClass))
			{
				cssClasses.Add(CssClass);
			}

			if (IsCurrentSiteMapEntity(entity, SiteMapProvider))
			{
				cssClasses.Add("xrm-entity-current");
			}

			return string.Join(" ", cssClasses.ToArray());
		}

		private static bool IsCurrentSiteMapEntity(Entity entity, string siteMapProvider)
		{
			if (!SiteMap.Enabled)
			{
				return false;
			}

			var currentNode = GetProvider(siteMapProvider).CurrentNode as CrmSiteMapNode;

			if (currentNode == null)
			{
				return false;
			}

			var currentEntity = currentNode.Entity;

			return currentEntity != null && entity.Id.Equals(currentEntity.Id);
		}

		private static SiteMapProvider GetProvider(string siteMapProvider)
		{
			return string.IsNullOrEmpty(siteMapProvider) ? SiteMap.Provider : SiteMap.Providers[siteMapProvider];
		}
	}
}
