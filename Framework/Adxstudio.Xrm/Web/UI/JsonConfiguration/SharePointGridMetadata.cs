/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class SharePointGridMetadata
	{
		public List<LanguageResources> AddFileButtonLabel { get; set; }

		public List<LanguageResources> DeleteFileButtonLabel { get; set; }

		public List<LanguageResources> ToolbarButtonLabel { get; set; }

		public bool? CreateEnabled { get; set; }

		public bool? DeleteEnabled { get; set; }

		public SharePointAddFilesModal AddFilesDialog { get; set; }
		
		public SharePointAddFolderModal AddFolderDialog { get; set; }

		public DeleteModal DeleteFileDialog { get; set; }
		
		public DeleteModal DeleteFolderDialog { get; set; }

		public List<LanguageResources> LoadingMessage { get; set; }

		public List<LanguageResources> ErrorMessage { get; set; }

		public List<LanguageResources> AccessDeniedMessage { get; set; }

		public List<LanguageResources> EmptyMessage { get; set; }

		public List<LanguageResources> GridTitle { get; set; }

		public List<LanguageResources> FileNameColumnLabel { get; set; }

		public List<LanguageResources> ModifiedColumnLabel { get; set; }
		
		public List<LanguageResources> ParentFolderPrefix { get; set; }

		public string AttachFileAccept { get; set; }

		public bool? AttachFileRestrictAccept { get; set; }
		
		public List<LanguageResources> AttachFileRestrictErrorMessage { get; set; }

		public int? AttachFileMaximumSize { get; set; }
		
		public List<LanguageResources> AttachFileMaximumSizeErrorMessage { get; set; }
	}
}
