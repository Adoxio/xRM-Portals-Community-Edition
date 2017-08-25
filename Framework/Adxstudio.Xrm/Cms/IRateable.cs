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
	public interface IRateable
	{
		IRatingInfo RatingInfo { get; }

		bool RatingEnabled { get; }
	}
}
