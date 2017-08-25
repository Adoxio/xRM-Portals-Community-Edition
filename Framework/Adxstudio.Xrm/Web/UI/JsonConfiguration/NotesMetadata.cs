/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class NotesMetadata
	{
		public List<LanguageResources> AddNoteButtonLabel { get; set; }

		public List<LanguageResources> EditNoteButtonLabel { get; set; }

		public List<LanguageResources> DeleteNoteButtonLabel { get; set; }

		public List<LanguageResources> ToolbarButtonLabel { get; set; }

		public bool? CreateEnabled { get; set; }

		public bool? EditEnabled { get; set; }

		public bool? DeleteEnabled { get; set; }

		public NoteModal CreateDialog { get; set; }

		public NoteModal EditDialog { get; set; }

		public DeleteModal DeleteDialog { get; set; }

		public List<LanguageResources> LoadingMessage { get; set; }

		public List<LanguageResources> ErrorMessage { get; set; }

		public List<LanguageResources> AccessDeniedMessage { get; set; }

		public List<LanguageResources> EmptyMessage { get; set; }

		public List<LanguageResources> ListTitle { get; set; }

		public List<Order> ListOrders { get; set; }

		public List<LanguageResources> NotePrivacyLabel { get; set; }

		public StorageLocation? AttachFileLocation { get; set; }

		public string AttachFileAccept { get; set; }

		public bool? AttachFileRestrictAccept { get; set; }
		
		public List<LanguageResources> AttachFileRestrictErrorMessage { get; set; }

		public int? AttachFileMaximumSize { get; set; }
		
		public List<LanguageResources> AttachFileMaximumSizeErrorMessage { get; set; }
	}
}
