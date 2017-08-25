/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	public interface ICommentPolicyReader
	{
		bool IsCommentPolicyOpen { get; }

		bool IsCommentPolicyOpenToAuthenticatedUsers { get; }

		bool IsCommentPolicyModerated { get; }

		bool IsCommentPolicyClosed { get; }

		bool IsCommentPolicyNone { get; }
	}
}
