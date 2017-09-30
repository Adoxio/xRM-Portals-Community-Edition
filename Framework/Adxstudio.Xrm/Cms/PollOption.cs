/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Cms
{
	public class PollOption : IPollOption
	{
		private IPoll _poll;

		public PollOption(Entity entity, IPoll poll)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;

			_poll = poll;

			Id = entity.GetAttributeValue<Guid>("adx_polloptionid");
			DisplayOrder = entity.GetAttributeValue<int?>("adx_displayorder");
			Name = entity.GetAttributeValue<string>("adx_name");
			Answer = entity.GetAttributeValue<string>("adx_answer");
			Votes = entity.GetAttributeValue<int?>("adx_votes");
		}

		[JsonIgnore]
		public Entity Entity { get; private set; }

		public int? DisplayOrder { get; private set; }

		public Guid Id { get; private set; }

		public string Name { get; private set; }

		public string Answer { get; private set; }

		public int? Votes { get; private set; }

		public decimal Percentage {
			get { return _poll.Votes > 0 ? (Votes.GetValueOrDefault(0) / (decimal)_poll.Votes) * 100 : 0; }
		}
	}
}
