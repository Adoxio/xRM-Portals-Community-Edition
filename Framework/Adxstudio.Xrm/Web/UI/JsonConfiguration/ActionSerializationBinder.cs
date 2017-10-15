/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.CrmEntityFormView;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class ActionSerializationBinder : SerializationBinder
	{
		private IList<Type> _knownTypes;
		
		private IList<Type> KnownTypes
		{
			get
			{
				if (_knownTypes == null)
				{
					_knownTypes = new List<Type>
					{
						typeof(ViewLayout),
						typeof(Order),
						typeof(EntityReference),
						typeof(FormConfiguration),
						typeof(GridMetadata),
						typeof(FormActionMetadata),
						typeof(NotesMetadata),
						typeof(SharePointGridMetadata),
						typeof(TimelineMetadata),
						typeof(ViewMetadata),
						typeof(View),
						typeof(Action),
						typeof(SubmitAction),
						typeof(NextAction),
						typeof(PreviousAction),
						typeof(EditAction),
						typeof(DetailsAction),
						typeof(CreateAction),
						typeof(AssociateAction),
						typeof(DisassociateAction),
						typeof(SearchAction),
						typeof(DownloadAction),
						typeof(ReopenCaseAction),
						typeof(ActivateAction),
						typeof(DeactivateAction),
						typeof(ActivateQuoteAction),
						typeof(SetOpportunityOnHoldAction),
						typeof(ReopenOpportunityAction),
						typeof(UpdatePipelinePhaseAction),
						typeof(RecordFormViewAction),
						typeof(FormViewAction),
						typeof(RedirectAction),
						typeof(WorkflowAction),
						typeof(DeleteAction),
						typeof(CloseIncidentAction),
						typeof(ResolveCaseAction),
						typeof(CancelCaseAction),
						typeof(QualifyLeadAction),
						typeof(ConvertQuoteToOrderAction),
						typeof(ConvertOrderToInvoiceAction),
						typeof(CalculateOpportunityAction),
						typeof(WinOpportunityAction),
						typeof(LoseOpportunityAction),
						typeof(GenerateQuoteFromOpportunityAction),
						typeof(CreateRelatedRecordAction),
						typeof(Modal),
						typeof(NoteModal),
						typeof(TimelineModal),
						typeof(SharePointAddFilesModal),
						typeof(SharePointAddFolderModal),
						typeof(LookupModal),
						typeof(ErrorModal),
						typeof(DeleteModal),
						typeof(CloseIncidentModal),
						typeof(WorkflowModal),
						typeof(DisassociateModal),
						typeof(ResolveCaseModal),
						typeof(ReopenCaseModal),
						typeof(CancelCaseModal),
						typeof(ActivateModal),
						typeof(DeactivateModal),
						typeof(ActivateQuoteModal),
						typeof(SetOpportunityOnHoldModal),
						typeof(ReopenOpportunityModal),
						typeof(WinOpportunityModal),
						typeof(LoseOpportunityModal),
						typeof(GenerateQuoteFromOpportunityModal),
						typeof(UpdatePipelinePhaseModal),
						typeof(QualifyLeadModal),
						typeof(ConvertQuoteModal),
						typeof(ConvertOrderModal),
						typeof(CalculateOpportunityModal),
						typeof(FormModal),
						typeof(EditFormModal),
						typeof(DetailsFormModal),
						typeof(CreateFormModal),
						typeof(CreateRelatedRecordModal),
						typeof(LanguageResources),
						typeof(GridOptions),
						typeof(CrmEntityListView.GridMetadata),
						typeof(CrmEntityListView.GridMetadata.Action),
						typeof(CrmEntityListView.GridMetadata.EditAction),
						typeof(CrmEntityListView.GridMetadata.DetailsAction),
						typeof(CrmEntityListView.GridMetadata.CreateAction),
						typeof(CrmEntityListView.GridMetadata.DeleteAction),
						typeof(CrmEntityListView.GridMetadata.AssociateAction),
						typeof(CrmEntityListView.GridMetadata.DisassociateAction),
						typeof(CrmEntityListView.GridMetadata.SearchAction),
						typeof(CrmEntityListView.GridMetadata.WorkflowAction),
						typeof(CrmEntityListView.GridMetadata.DownloadAction),
						typeof(CrmEntityListView.GridMetadata.Modal),
						typeof(CrmEntityListView.GridMetadata.LookupModal),
						typeof(CrmEntityListView.GridMetadata.ErrorModal),
						typeof(CrmEntityListView.GridMetadata.DeleteModal),
						typeof(CrmEntityListView.GridMetadata.FormModal),
						typeof(CrmEntityListView.GridMetadata.EditFormModal),
						typeof(CrmEntityListView.GridMetadata.DetailsFormModal),
						typeof(CrmEntityListView.GridMetadata.CreateFormModal),
						typeof(CrmEntityListView.ViewColumn),
						typeof(CrmEntityListView.GridMetadata.LookupModal.GridOptions),
						typeof(CrmEntityFormView.NotesMetadata),
						typeof(CrmEntityFormView.NotesMetadata.Modal),
						typeof(CrmEntityFormView.NotesMetadata.DeleteModal),
						typeof(CrmEntityFormView.NotesMetadata.NoteModal)
					};
				}
				return _knownTypes;
			}
		}

		public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			assemblyName = null;
			typeName = serializedType.Name;
		}
		public override Type BindToType(string assemblyName, string typeName)
		{
			return KnownTypes.FirstOrDefault(t => t.FullName == typeName) ?? typeof(NotSupportedAction);
		}
	}
}
