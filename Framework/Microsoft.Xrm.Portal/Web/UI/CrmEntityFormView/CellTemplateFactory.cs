/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class CellTemplateFactory : ICellTemplateFactory // MSBug #120057: Won't seal, inheritance is used extension point.
	{
		public void Initialize(Control control, ICellMetadataFactory metadataFactory, IDictionary<string, CellBinding> cellBindings, int languageCode, string validationGroup, bool enableUnsupportedFields)
		{
			metadataFactory.ThrowOnNull("metadataFactory");

			MetadataFactory = metadataFactory;
			CellBindings = cellBindings ?? new Dictionary<string, CellBinding>();
			LanguageCode = languageCode;
			ValidationGroup = validationGroup;
			EnableUnsupportedFields = enableUnsupportedFields;

			IsInitialized = true;
		}

		protected IDictionary<string, CellBinding> CellBindings { get; private set; }

		protected bool EnableUnsupportedFields { get; private set; }

		protected bool IsInitialized { get; private set; }

		protected int LanguageCode { get; private set; }

		protected ICellMetadataFactory MetadataFactory { get; private set; }

		protected string ValidationGroup { get; private set; }

		public virtual ICellTemplate CreateTemplate(XNode cellNode, EntityMetadata entityMetadata)
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException("Factory is not initialized.");
			}

			var cellMetadata = MetadataFactory.GetMetadata(cellNode, entityMetadata, LanguageCode);

			if (cellMetadata.HasAttributeType("boolean"))
			{
				return new BooleanControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("datetime"))
			{
				return new DateTimeControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("integer"))
			{
				return new IntegerControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("memo"))
			{
				return new MemoControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("picklist"))
			{
				return new PicklistControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("string"))
			{
				if (string.Equals("email", cellMetadata.Format, StringComparison.InvariantCultureIgnoreCase))
				{
					return new EmailStringControlTemplate(cellMetadata, ValidationGroup, CellBindings);
				}

				return new StringControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.HasAttributeType("money"))
			{
				return new MoneyControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (cellMetadata.AttributeType == null)
			{
				return new EmptyCellTemplate();
			}

			return new UnsupportedControlTemplate(cellMetadata, ValidationGroup, CellBindings, EnableUnsupportedFields);
		}
	}
}
