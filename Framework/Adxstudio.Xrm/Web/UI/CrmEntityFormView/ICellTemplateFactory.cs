/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.UI;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Factory pattern class to create a cell template.
	/// </summary>
	public interface ICellTemplateFactory : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.ICellTemplateFactory
	{
		/// <summary>
		/// CellTemplateFactory Initialization.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="fields"></param>
		/// <param name="metadataFactory"></param>
		/// <param name="cellBindings"></param>
		/// <param name="languageCode"></param>
		/// <param name="validationGroup"></param>
		/// <param name="enableUnsupportedFields"></param>
		/// <param name="toolTipEnabled"></param>
		/// <param name="recommendedFieldsRequired"></param>
		/// <param name="validationText"></param>
		/// <param name="contextName"></param>
		/// <param name="renderWebResourcesInline"></param>
		/// <param name="webFormMetadata"></param>
		/// <param name="forceAllFieldsRequired"></param>
		/// <param name="enableValidationSummaryLinks"></param>
		/// <param name="messages"> </param>
		void Initialize(Control control, Collection<CrmEntityFormViewField> fields, ICellMetadataFactory metadataFactory,
										IDictionary<string, CellBinding> cellBindings, int languageCode, string validationGroup, bool enableUnsupportedFields,
										bool? toolTipEnabled, bool? recommendedFieldsRequired, string validationText, string contextName, bool? renderWebResourcesInline, IEnumerable<Entity> webFormMetadata, bool? forceAllFieldsRequired, bool? enableValidationSummaryLinks, Dictionary<string, string> messages, bool? showOwnerFields, int baseOrganizationLanguageCode = 0);
	}
}
