/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Issues
{
	internal enum IssueCommentPolicy
	{
		Open                     = 100000000,
		OpenToAuthenticatedUsers = 100000001,
		Moderated                = 100000002,
		Closed                   = 100000003,
		None                     = 100000004,
		Inherit                  = 100000005
	}
}
