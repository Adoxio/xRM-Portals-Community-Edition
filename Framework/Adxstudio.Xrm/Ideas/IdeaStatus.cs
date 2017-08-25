/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Enum representing the types of Ideas in which to query for and display
	/// </summary>
	public enum IdeaStatus
	{
		New = 1,
		Accepted = 100000000,
		Completed = 100000001,
		Rejected = 100000002,
		Inactive = 2,
		Any = -1
	}
}
