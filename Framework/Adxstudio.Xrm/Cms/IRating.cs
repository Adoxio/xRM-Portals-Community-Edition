/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IRating
	{
		Entity Entity { get; }

		EntityReference Regarding { get; }

		int Value { get; }

		int MaximumValue { get; }

		int MinimumValue { get; }
	}
}
