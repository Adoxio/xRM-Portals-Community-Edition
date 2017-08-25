/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	public interface IEventSchedule
	{
		Entity Entity { get; }

		DateTime StartTime { get; }

		DateTime EndTime { get;  }

		Entity Event { get; }

		bool IsAllDayEvent { get; }

		string Name { get; }
	}
}
