/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template for cell
	/// </summary>
	public abstract class CellContainerTemplate : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.CellContainerTemplate
	{
		protected CellContainerTemplate(XNode node, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory)
			: base(node, languageCode, entityMetadata, cellTemplateFactory)
		{
			cellTemplateFactory.ThrowOnNull("cellTemplateFactory");

			CellTemplateFactory = cellTemplateFactory;
		}

		protected new ICellTemplateFactory CellTemplateFactory { get; private set; }
	}
}
