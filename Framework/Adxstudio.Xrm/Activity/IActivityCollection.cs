/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Activity
{
	public interface IActivityCollection : IEnumerable<IActivity>
	{
		int TotalCount { get; }
		bool PermissionDenied { get; }
	}
}
