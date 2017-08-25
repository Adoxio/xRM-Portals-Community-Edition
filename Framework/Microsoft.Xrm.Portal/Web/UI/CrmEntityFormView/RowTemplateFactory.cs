/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class RowTemplateFactory
	{
		public RowTemplateFactory(int languageCode)
		{
			LanguageCode = languageCode;
		}

		public int LanguageCode { get; private set; }

		public virtual ITemplate CreateTemplate(XNode rowNode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory)
		{
			return new GridLayoutRowTemplate(rowNode, LanguageCode, entityMetadata, cellTemplateFactory);
		}
	}
}
