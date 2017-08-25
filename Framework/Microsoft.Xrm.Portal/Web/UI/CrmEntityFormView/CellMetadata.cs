/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public abstract class CellMetadata : ICellMetadata
	{
		protected CellMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode, Func<XNode, EntityMetadata, string> getDataFieldName)
		{
			cellNode.ThrowOnNull("cellNode");
			entityMetadata.ThrowOnNull("entityMetadata");

			string colSpan;

			if (XNodeUtility.TryGetAttributeValue(cellNode, ".", "colspan", out colSpan))
			{
				ColumnSpan = int.Parse(colSpan);
			}

			string rowSpan;

			if (XNodeUtility.TryGetAttributeValue(cellNode, ".", "rowspan", out rowSpan))
			{
				RowSpan = int.Parse(rowSpan);
			}

			EntityMetadata = entityMetadata;
			LanguageCode = languageCode;

			var dataFieldName = getDataFieldName(cellNode, entityMetadata);

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(attribute => attribute.LogicalName == dataFieldName);

			if (string.IsNullOrEmpty(dataFieldName) || attributeMetadata == null)
			{
				Disabled = true;

				return;
			}

			DataFieldName = dataFieldName;
			AttributeMetadata = attributeMetadata;

			ExtractMetadata();
		}

		public string AttributeType
		{
			get { return AttributeMetadata == null ? null : AttributeMetadata.AttributeType.Value.ToString(); }
		}

		public int? ColumnSpan { get; private set; }

		public string DataFieldName { get; private set; }

		public bool Disabled { get; protected set; }

		public string Format { get; private set; }

		public bool IsRequired
		{
			get
			{
				if (AttributeMetadata == null)
				{
					return false;
				}

				try
				{
					return AttributeMetadata.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired
						|| AttributeMetadata.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired;
				}
				catch
				{
					return false;
				}
			}
		}

		public abstract string Label { get; }

		public int LanguageCode { get; private set; }

		public decimal? MaxValue { get; private set; }

		public decimal? MinValue { get; private set; }

		public IEnumerable<OptionMetadata> PicklistOptions
		{
			get
			{
				var picklistMetadata = AttributeMetadata as PicklistAttributeMetadata;

				return picklistMetadata == null
					? new List<OptionMetadata>() as IEnumerable<OptionMetadata>
					: picklistMetadata.OptionSet.Options;
			}
		}

		public int? RowSpan { get; private set; }

		public abstract bool ShowLabel { get; }

		public string ToolTip { get; private set; }

		protected AttributeMetadata AttributeMetadata { get; private set; }

		protected EntityMetadata EntityMetadata { get; private set; }

		public bool HasAttributeType(string attributeTypeName)
		{
			try
			{
				return string.Equals(AttributeType, attributeTypeName, StringComparison.InvariantCultureIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private void ExtractMetadata()
		{
			if (AttributeMetadata.GetType() == typeof(BigIntAttributeMetadata))
			{
				var metadata = AttributeMetadata as BigIntAttributeMetadata;
				
				MaxValue = metadata.MaxValue;
				MinValue = metadata.MinValue;
			}
			else if (AttributeMetadata.GetType() == typeof(DateTimeAttributeMetadata))
			{
				var metadata = AttributeMetadata as DateTimeAttributeMetadata;

				Format = metadata.Format.HasValue
					? Enum.GetName(typeof(DateTimeFormat), metadata.Format.Value)
					: null;
			}
			else if (AttributeMetadata.GetType() == typeof(DecimalAttributeMetadata))
			{
				var metadata = AttributeMetadata as DecimalAttributeMetadata;
				
				MaxValue = metadata.MaxValue;
				MinValue = metadata.MinValue;
			}
			else if (AttributeMetadata.GetType() == typeof(DoubleAttributeMetadata))
			{
				var metadata = AttributeMetadata as DoubleAttributeMetadata;
				
				MaxValue = (decimal)metadata.MaxValue;
				MinValue = (decimal)metadata.MinValue;
			}
			else if (AttributeMetadata.GetType() == typeof(IntegerAttributeMetadata))
			{
				var metadata = AttributeMetadata as IntegerAttributeMetadata;
				
				Format = metadata.Format.HasValue
					? Enum.GetName(typeof(IntegerFormat), metadata.Format.Value)
					: null;
				MaxValue = metadata.MaxValue;
				MinValue = metadata.MinValue;
			}
			else if (AttributeMetadata.GetType() == typeof(MemoAttributeMetadata))
			{
				var metadata = AttributeMetadata as MemoAttributeMetadata;

				Format = metadata.Format.HasValue
					? Enum.GetName(typeof(StringFormat), metadata.Format.Value)
					: null;
			}
			else if (AttributeMetadata.GetType() == typeof(MoneyAttributeMetadata))
			{
				var metadata = AttributeMetadata as MoneyAttributeMetadata;
				
				MaxValue = (decimal)metadata.MaxValue;
				MinValue = (decimal)metadata.MinValue;
			}
			else if (AttributeMetadata.GetType() == typeof(StringAttributeMetadata))
			{
				var metadata = AttributeMetadata as StringAttributeMetadata;

				Format = metadata.Format.HasValue
					? Enum.GetName(typeof(StringFormat), metadata.Format.Value)
					: null;
			}

			var localiazedDescription = AttributeMetadata.Description.LocalizedLabels.SingleOrDefault(label => label.LanguageCode == LanguageCode);

			if (localiazedDescription != null)
			{
				ToolTip = localiazedDescription.Label;
			}
		}
	}
}
