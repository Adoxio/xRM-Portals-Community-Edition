/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class SavedQueryCellMetadata : CellMetadata
	{
		private readonly string _label;

		public SavedQueryCellMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode)
			: base(cellNode, entityMetadata, languageCode, GetDataFieldName)
		{
			if (string.IsNullOrEmpty(DataFieldName))
			{
				return;
			}

			var attribute = entityMetadata.Attributes.SingleOrDefault(a => a.LogicalName == DataFieldName.ToLowerInvariant());

			if (attribute == null)
			{
				return;
			}

			var localizedDisplayName = attribute.DisplayName.LocalizedLabels.SingleOrDefault(label => label.LanguageCode == LanguageCode);

			if (localizedDisplayName != null)
			{
				_label = localizedDisplayName.Label;
			}
		}

		public override string Label
		{
			get { return _label; }
		}

		public override bool ShowLabel
		{
			get { return true; }
		}

		private static string GetDataFieldName(XNode cellNode, EntityMetadata entityMetadata)
		{
			string dataFieldName;

			return XNodeUtility.TryGetAttributeValue(cellNode, ".", "name", out dataFieldName) ? dataFieldName : null;
		}
	}
}
