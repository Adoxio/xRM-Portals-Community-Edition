/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web
{
	internal sealed class CrmEntityAttachmentInfo : ICrmEntityAttachmentInfo
	{
		public CrmEntityAttachmentInfo(string url, DateTime? lastModified)
		{
			url.ThrowOnNullOrWhitespace("url");

			Url = url;
			LastModified = lastModified;
		}

		public DateTime? LastModified { get; private set; }

		public string Url { get; private set; }
	}
}
