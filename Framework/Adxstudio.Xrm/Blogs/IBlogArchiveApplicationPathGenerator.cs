/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	internal interface IBlogArchiveApplicationPathGenerator
	{
		ApplicationPath GetAuthorPath(Guid authorId, EntityReference blog);

		ApplicationPath GetMonthPath(DateTime month, EntityReference blog);

		ApplicationPath GetTagPath(string tag, EntityReference blog);
	}
}
