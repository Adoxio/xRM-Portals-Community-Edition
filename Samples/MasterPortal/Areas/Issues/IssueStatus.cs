/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Issues
{
	public enum IssueStatus
	{
		NewOrUnconfirmed =		100000010,
		Confirmed =				100000011,
		WorkaroundAvailable =	100000012,
		Resolved =				100000013,
		WillNotFix =			100000014,
		ByDesign =				100000015,
		UnableToReproduce =		100000016,
		Inactive = 2
	}
}
