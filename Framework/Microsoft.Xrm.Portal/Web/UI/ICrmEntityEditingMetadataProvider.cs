/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.UI
{
	public interface ICrmEntityEditingMetadataProvider
	{
		void AddAttributeMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity, string propertyName, string propertyDisplayName);

		void AddEntityMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity);

		void AddSiteMapNodeMetadata(string portalName, IEditableCrmEntityControl control, Control container, SiteMapNode node);
	}
}
