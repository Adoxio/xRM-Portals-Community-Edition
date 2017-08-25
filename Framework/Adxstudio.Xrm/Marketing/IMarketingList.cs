/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Marketing
{
	public interface IMarketingList
	{
		Guid Id { get; }
		string Name { get; }
		string Purpose { get; }
		IEnumerable<EntityReference> Subscribers { get; }
	}
}
