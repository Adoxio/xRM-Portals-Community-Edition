/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Xml.Linq;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public abstract class CrmEntityFormViewTemplate : ITemplate
	{
		protected CrmEntityFormViewTemplate(XNode node, int languageCode, EntityMetadata entityMetadata)
		{
			node.ThrowOnNull("node");
			entityMetadata.ThrowOnNull("entityMetadata");

			Node = node;
			LanguageCode = languageCode;
			EntityMetadata = entityMetadata;
		}

		protected EntityMetadata EntityMetadata { get; private set; }

		protected int LanguageCode { get; private set; }

		protected XNode Node { get; private set; }

		public abstract void InstantiateIn(Control container);
	}
}
