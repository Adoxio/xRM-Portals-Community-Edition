/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.SharePoint
{
	public interface ISharePointResult
	{
		bool CanRead { get; }
		bool CanCreate { get; }
		bool CanWrite { get; }
		bool CanDelete { get; }
		bool CanAppend { get; }
		bool CanAppendTo { get; }
		bool PermissionsExist { get; }
	}
}
