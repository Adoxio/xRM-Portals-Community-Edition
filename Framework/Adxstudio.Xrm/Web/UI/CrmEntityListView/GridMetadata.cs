/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	public class GridMetadata : JsonConfiguration.GridMetadata
	{
		public class Modal : JsonConfiguration.Modal { }
		public class LookupModal : JsonConfiguration.LookupModal { public class GridOptions : JsonConfiguration.GridOptions { } }
		public class ErrorModal : JsonConfiguration.ErrorModal { }
		public class DeleteModal : JsonConfiguration.DeleteModal { }
		public class FormModal : JsonConfiguration.FormModal { }
		public class EditFormModal : JsonConfiguration.EditFormModal { }
		public class DetailsFormModal : JsonConfiguration.DetailsFormModal { }
		public class CreateFormModal : JsonConfiguration.CreateFormModal { }
		public class Action : JsonConfiguration.Action { }
		public class EditAction : JsonConfiguration.EditAction { }
		public class DetailsAction : JsonConfiguration.DetailsAction { }
		public class CreateAction : JsonConfiguration.CreateAction { }
		public class DeleteAction : JsonConfiguration.DeleteAction { }
		public class AssociateAction : JsonConfiguration.AssociateAction { }
		public class DisassociateAction : JsonConfiguration.DisassociateAction { }
		public class SearchAction : JsonConfiguration.SearchAction { }
		public class WorkflowAction : JsonConfiguration.WorkflowAction { }
		public class DownloadAction : JsonConfiguration.DownloadAction { }
	}
}
