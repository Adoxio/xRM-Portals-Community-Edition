/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public abstract class CellContainerTemplate : CrmEntityFormViewTemplate
	{
		protected CellContainerTemplate(XNode node, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory)
			: base(node, languageCode, entityMetadata)
		{
			cellTemplateFactory.ThrowOnNull("cellTemplateFactory");

			CellTemplateFactory = cellTemplateFactory;
		}

		protected ICellTemplateFactory CellTemplateFactory { get; private set; }
	}
}
