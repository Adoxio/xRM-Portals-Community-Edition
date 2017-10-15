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
	public class EventSpeaker : IEventSpeaker
	{
		public EventSpeaker(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;
		}

		public Entity Entity { get; private set; }
	}
}
