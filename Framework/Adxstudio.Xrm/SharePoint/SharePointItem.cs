/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.SharePoint
{
	public class SharePointItem : ISharePointItem
	{
		public int? Id { get; set; }

		public string Name { get; set; }
		
		public DateTime? CreatedOn { get; set; }

		public string CreatedOnDisplay
		{
			get { return CreatedOn.HasValue ? CreatedOn.Value.ToString("r") : string.Empty; }
		}

		public DateTime? ModifiedOn { get; set; }
		
		public string ModifiedOnDisplay
		{
			get { return ModifiedOn.HasValue ? ModifiedOn.Value.ToString("r") : string.Empty; }
		}
		
		public long? FileSize { get; set; }

		public string FileSizeDisplay
		{
			get
			{
				if (!FileSize.HasValue)
				{
					return string.Empty;
				}

				var size = FileSize.Value;

				return size >= 0x400 ? (size / 1024) + " KB" : "1 KB";
			}
		}
		
		public string FolderPath { get; set; }
		
		public bool IsFolder { get; set; }
		
		public bool? IsParent { get; set; }
		
		public string Url { get; set; }
	}
}
