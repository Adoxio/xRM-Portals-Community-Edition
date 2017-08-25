/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public interface ICellMetadataFactory
	{
		ICellMetadata GetMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode);
	}
}
