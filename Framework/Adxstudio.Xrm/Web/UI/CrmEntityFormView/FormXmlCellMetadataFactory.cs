/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Factory used to create cell metadata from form XML.
	/// </summary>
	public class FormXmlCellMetadataFactory : ICellMetadataFactory
	{
		/// <summary>
		/// The dictionary of all the metadata that are saved during the process of cell initialization.
		/// This dictionary is built up during construction of all the cell controls 
		/// </summary>
		private IDictionary<string, FormXmlCellMetadata> CellMetadata = new Dictionary<string, FormXmlCellMetadata>();

		private void AddCellMetadata(FormXmlCellMetadata metadata)
		{
			if (metadata != null && !string.IsNullOrWhiteSpace(metadata.DataFieldName))
			{
				CellMetadata[metadata.DataFieldName] = metadata;
			}
		}

		/// <summary>
		/// GetMetadata.
		/// </summary>
		/// <param name="cellNode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="languageCode"></param>
		/// <returns></returns>
		public ICellMetadata GetMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode)
		{
			var metadata = new FormXmlCellMetadata(cellNode, entityMetadata, languageCode, null, null, null, null, null, null, null, null);
			AddCellMetadata(metadata);

			return metadata;
		}

		/// <summary>
		/// GetMetadata.
		/// </summary>
		/// <param name="cellNode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="languageCode"></param>
		/// <param name="toolTipEnabled"></param>
		/// <param name="recommendedFieldsRequired"></param>
		/// <param name="validationText"></param>
		/// <param name="webformMetadata"></param>
		/// <param name="forceAllFieldsRequired"></param>
		/// <param name="enableValidationSummaryLinks"></param>
		/// <param name="validationSummaryLinkText"></param>
		/// <param name="messages"> </param>
		/// <returns></returns>
		public ICellMetadata GetMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode, bool? toolTipEnabled, bool? recommendedFieldsRequired, string validationText, IEnumerable<Entity> webformMetadata, bool? forceAllFieldsRequired, bool? enableValidationSummaryLinks, string validationSummaryLinkText, Dictionary<string, string> messages, int baseOrganizationLanguageCode)
		{
			var metadata = new FormXmlCellMetadata(cellNode, entityMetadata, languageCode, toolTipEnabled, recommendedFieldsRequired, validationText, webformMetadata, forceAllFieldsRequired, enableValidationSummaryLinks, validationSummaryLinkText, messages, baseOrganizationLanguageCode);
			AddCellMetadata(metadata);

			return metadata;
		}

		/// <summary>
		/// Get cell metadata that are created when the specific cell template is initialized.
		/// </summary>
		/// <param name="fieldName">The logical name for the field.</param>
		/// <param name="cellMetadata"></param>
		internal bool TryGetCellMetadata(string fieldName, out FormXmlCellMetadata cellMetadata)
		{
			return CellMetadata.TryGetValue(fieldName, out cellMetadata);
		}
	}
}
