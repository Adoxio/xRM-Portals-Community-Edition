/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Xml.Linq;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering layout for a row.
	/// </summary>
	public class TableLayoutRowTemplateFactory : RowTemplateFactory
	{
		/// <summary>
		/// TableLayoutRowTemplateFactory class initialization.
		/// </summary>
		/// <param name="languageCode"></param>
		public TableLayoutRowTemplateFactory(int languageCode) : base(languageCode) { }

		/// <summary>
		/// Method used to create template.
		/// </summary>
		/// <param name="rowNode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="cellTemplateFactory"></param>
		/// <returns></returns>
		public ITemplate CreateTemplate(XNode rowNode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory)
		{
			return new TableLayoutRowTemplate(rowNode, LanguageCode, entityMetadata, cellTemplateFactory);
		}
	}
}
