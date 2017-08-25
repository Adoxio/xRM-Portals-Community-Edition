/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Portal.Web.Routing;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Activity
{
	public class Attachment : IAttachment
	{
		public Attachment()
		{

		}

		public Attachment(Func<IAttachment> getAttachmentFile)
		{
			var attachmentFile = getAttachmentFile();

			AttachmentFileName = attachmentFile.AttachmentUrl;
			AttachmentIsImage = attachmentFile.AttachmentIsImage;
			AttachmentSize = attachmentFile.AttachmentSize;
			AttachmentSizeDisplay = attachmentFile.AttachmentSizeDisplay;
			AttachmentContentType = attachmentFile.AttachmentContentType;
			AttachmentUrl = attachmentFile.AttachmentUrl;
			AttachmentBody = attachmentFile.AttachmentBody;
			Entity = attachmentFile.Entity;
		}

		public string AttachmentFileName { get; set; }
		public bool AttachmentIsImage { get; set; }
		public FileSize AttachmentSize { get; set; }
		public string AttachmentSizeDisplay { get; set; }
		public string AttachmentContentType { get; set; }
		public string AttachmentUrl { get; set; }
		public byte[] AttachmentBody { get; set; }
		public Entity Entity { get; set; }
	}
}
