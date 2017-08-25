/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Activity
{
	/// <summary>
	/// Every Entity Object that is to be used by data adapters should inherit from this if possible
	/// </summary>
	public class Activity : IActivity
	{
		public Entity Entity { get; set; }
		public EntityReference Regarding { get; set; }

		public EntityReference EntityReference
		{
			get { return Entity.ToEntityReference(); }
		}

		public ICollection<IAnnotationFile> FileAttachments { get; set; }

		public Activity()
		{
			
		}

		public Activity(Entity entity, EntityReference regarding)
		{
			Regarding = regarding;
			Entity = entity;
		}
		
		public enum ParticipationTypeMaskOptionSetValue
		{
			Sender = 1,
			ToRecipient = 2,
			CcRecipient = 3,
			BccRecipient = 4,
			RequiredAttendee = 5,
			OptionalAttendee = 6,
			Organizer = 7,
			Regarding = 8,
			Owner = 9,
			Resource = 10,
			Customer = 11
		}
	}
}
