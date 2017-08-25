/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Renders metadata required by the CMS front-side editing system to the client DOM.
	/// </summary>
	/// <remarks>
	/// This interface is intended as a replacement for <see cref="ICrmEntityEditingMetadataProvider"/>, which assumed
	/// an ASP.NET Web Forms control hierarchy as its render target. This interface is rendering-system-agnostic, by
	/// doing its rendering operations through <see cref="ICmsEntityEditingMetadataContainer"/>. This allows for
	/// rendering of CMS metadata through ASP.NET MVC, etc.
	/// </remarks>
	public interface ICmsEntityEditingMetadataProvider
	{
		void AddAttributeMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entity, string attributeLogicalName, string attributeDisplayName, string portalName = null);

		void AddEntityMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entity, string portalName = null, string entityDisplayName = null);

		void AddEntityMetadata(ICmsEntityEditingMetadataContainer container, string entityLogicalName, string portalName = null, string entityDisplayName = null, JObject initialValues = null);

		void AddSiteMapNodeMetadata(ICmsEntityEditingMetadataContainer container, SiteMapNode node, string portalName = null);
	}
}
