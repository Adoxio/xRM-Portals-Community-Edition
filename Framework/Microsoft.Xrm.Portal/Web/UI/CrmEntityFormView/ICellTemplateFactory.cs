/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public interface ICellTemplateFactory
	{
		void Initialize(Control control, ICellMetadataFactory metadataFactory, IDictionary<string, CellBinding> cellBindings, int languageCode, string validationGroup, bool enableUnsupportedFields);

		ICellTemplate CreateTemplate(XNode cellNode, EntityMetadata entityMetadata);
	}
}
