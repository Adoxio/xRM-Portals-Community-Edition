/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Security
{
	[Flags]
	public enum CrmEntityPermissionRight
	{
		Read = 0x01,
		Write = 0x02,
		Create = 0x04,
		Delete = 0x08,
		Append = 0x10,
		AppendTo = 0x20,
	}
}
