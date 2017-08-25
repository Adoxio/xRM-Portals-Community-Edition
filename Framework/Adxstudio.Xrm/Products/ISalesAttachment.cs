/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Text;

namespace Adxstudio.Xrm.Products
{
	public interface ISalesAttachment
	{
		string AbstractText { get; }

		string AttachedDocumentURL { get; }

		string AuthorName { get; }
		
		string ContentType { get; }

		string FileName { get; }

		FileSize FileSize { get; }

		bool HasFile { get; }

		Guid ID { get; }

		string Keywords { get; }

		string Title { get; }

		string URL { get; }
	}
}
