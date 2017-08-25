/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Activity
{
	public interface IAttachment
	{
		string AttachmentFileName { get; set; }
		bool AttachmentIsImage { get; set; }
		FileSize AttachmentSize { get; set; }
		string AttachmentSizeDisplay { get; set; }
		string AttachmentContentType { get; set; }
		string AttachmentUrl { get; set; }
		byte[] AttachmentBody { get; set; }
		Entity Entity { get; set; }
	}
}
