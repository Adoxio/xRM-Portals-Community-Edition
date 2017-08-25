/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Events
{
	public interface IEventDataAdapter
	{
		IEvent Select(Guid eventId);

		IEvent Select(string eventName);

		IEnumerable<IEventSpeaker> SelectSpeakers();

		IEnumerable<IEventSponsor> SelectSponsors();

		IEnumerable<IEventSchedule> SelectSchedules();
	}
}
