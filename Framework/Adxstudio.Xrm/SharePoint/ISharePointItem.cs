/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.SharePoint
{
	public interface ISharePointItem
	{
		int? Id { get; set; }
		string Name { get; set; }
		DateTime? CreatedOn { get; set; }
		string CreatedOnDisplay { get; }
		DateTime? ModifiedOn { get; set; }
		string ModifiedOnDisplay { get; }
		long? FileSize { get; set; }
		string FileSizeDisplay { get; }
		string FolderPath { get; set; }
		bool IsFolder { get; set; }
		bool? IsParent { get; set; }
		string Url { get; set; }
	}
}
