/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IAdPlacement
	{
		Entity Entity { get; }

		string Name { get; }

		IEnumerable<IAd> Ads { get; }

		EntityReference WebTemplate { get; }
	}
}
