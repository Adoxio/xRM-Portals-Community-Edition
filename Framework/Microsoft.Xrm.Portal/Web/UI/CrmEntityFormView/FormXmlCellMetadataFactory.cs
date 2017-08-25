/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public sealed class FormXmlCellMetadataFactory : ICellMetadataFactory
	{
		public ICellMetadata GetMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode)
		{
			return new FormXmlCellMetadata(cellNode, entityMetadata, languageCode);
		}
	}
}
