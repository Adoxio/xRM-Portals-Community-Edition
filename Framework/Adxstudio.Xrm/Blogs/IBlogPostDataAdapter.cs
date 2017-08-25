/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogPostDataAdapter
	{
		IBlogPost Select();
	}
}
