/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	public interface IEventOccurrence
	{
		DateTime End { get; }

		Entity Event { get; }

		Entity EventSchedule { get; }

		bool IsAllDayEvent { get; }

		string Location { get; }

		DateTime Start { get; }

		string Url { get; }
	}
}
