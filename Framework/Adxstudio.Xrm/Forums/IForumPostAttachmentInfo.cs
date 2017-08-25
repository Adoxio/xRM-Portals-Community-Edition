/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumPostAttachmentInfo
	{
		string ContentType { get; }

		string Name { get; }

		ApplicationPath Path { get; }

		FileSize Size { get; }
	}
}
