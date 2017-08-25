/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Search
{
	public interface ICrmEntitySearchResult
	{
		Entity Entity { get; }

		Guid EntityID { get; }

		string EntityLogicalName { get; }

		Dictionary<string, string> ExtendedAttributes { get; }

		string Fragment { get; set; }

		int ResultNumber { get; }

		float Score { get; }

		string Title { get; }

		Uri Url { get; }
	}
}
