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
	public class TimelineMetadata : NotesMetadata
	{
		public List<LanguageResources> LoadMoreButtonLabel { get; set; }
		public string AttachFileAcceptExtensions { get; set; }
	}
}
