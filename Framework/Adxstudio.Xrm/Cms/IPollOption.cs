/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
    public interface IPollOption
	{
		int? DisplayOrder { get; }

		Entity Entity { get; }

		Guid Id { get; }

        string Name { get; }

		string Answer { get; }

		int? Votes { get; }

		decimal Percentage { get; }
    }
}
