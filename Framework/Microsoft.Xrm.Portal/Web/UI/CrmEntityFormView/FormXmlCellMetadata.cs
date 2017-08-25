/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class FormXmlCellMetadata : CellMetadata // MSBug #120064: Won't seal, inheritance is expected extension point.
	{
		private readonly string _label;
		private readonly bool _showLabel;

		public FormXmlCellMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode)
			: base(cellNode, entityMetadata, languageCode, GetDataFieldName)
		{
			bool disabled;

			if (XNodeUtility.TryGetBooleanAttributeValue(cellNode, "control", "disabled", out disabled))
			{
				// Preserve any existing true value of Disabled.
				Disabled = Disabled || disabled;
			}

			XNodeUtility.TryGetAttributeValue(cellNode, "labels/label[@languagecode='{0}']".FormatWith(LanguageCode), "description", out _label);
			
			if (!XNodeUtility.TryGetBooleanAttributeValue(cellNode, ".", "showlabel", out _showLabel))
			{
				// The CRM defaults to true if the showlabel attribute does not exist, so we will do the same.
				_showLabel = true;
			}
		}

		public override string Label
		{
			get { return _label; }
		}

		public override bool ShowLabel
		{
			get { return _showLabel; }
		}

		private static string GetDataFieldName(XNode cellNode, EntityMetadata entityMetadata)
		{
			string dataFieldName;

			return XNodeUtility.TryGetAttributeValue(cellNode, "control", "datafieldname", out dataFieldName) ? dataFieldName : null;
		}
	}
}
