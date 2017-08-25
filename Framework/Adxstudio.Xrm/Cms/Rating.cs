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
	public class Rating : IRating
	{
		public Rating(Entity entity)
		{
			Entity = entity;
		}

		public Entity Entity { get; private set; }

		public EntityReference Regarding
		{
			get { return Entity.GetAttributeValue<EntityReference>("regardingobjectid"); }
		}

		public int Value
		{
			get { return Entity.GetAttributeValue<int>(FeedbackMetadataAttributes.RatingValueAttributeName); }
		}

		public int MaximumValue
		{
			get { return Entity.GetAttributeValue<int>(FeedbackMetadataAttributes.MaxRatingAttributeName); }
		}

		public int MinimumValue
		{
			get { return Entity.GetAttributeValue<int>(FeedbackMetadataAttributes.MinRatingAttributeName); }
		}
	}
}
