/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Blogs
{
	internal enum BlogPostCommentPolicy
	{
		None                      = 100000000,
		Open                      = 100000001,
		OpenToAuthenticatedUsers  = 100000002,
		Moderated                 = 100000003,
		Closed                    = 100000004,
		Inherit                   = 100000005,
	}
}
