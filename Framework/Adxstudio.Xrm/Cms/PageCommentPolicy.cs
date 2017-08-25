/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adxstudio.Xrm.Cms
{
	internal enum PageCommentPolicy
	{
		None = 756150001,
		Open = 756150002,
		OpenToAuthenticatedUsers = 756150003,
		Moderated = 756150004,
		Closed = 756150005,
		Inherit = 756150000,
	}
}
