/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Cms
{
	public interface IWebLinkSetDataAdapter
	{
		IWebLinkSet Select(Guid webLinkSetId);

		IWebLinkSet Select(string webLinkSetName);

		IEnumerable<IWebLink> SelectWebLinks(Guid webLinkSetId);

		IEnumerable<IWebLink> SelectWebLinks(string webLinkSetName);
	}
}
