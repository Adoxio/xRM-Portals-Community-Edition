/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public interface ICellMetadata
	{
		string AttributeType { get; }

		int? ColumnSpan { get; }

		string DataFieldName { get; }

		bool Disabled { get; }

		string Format { get; }

		bool IsRequired { get; }

		string Label { get; }

		int LanguageCode { get; }

		decimal? MaxValue { get; }

		decimal? MinValue { get; }

		IEnumerable<OptionMetadata> PicklistOptions { get; }

		int? RowSpan { get; }

		bool ShowLabel { get; }

		string ToolTip { get; }

		bool HasAttributeType(string attributeTypeName);
	}
}
