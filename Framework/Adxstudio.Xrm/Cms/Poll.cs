/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Cms
{
	public class Poll : IPoll
	{
		public Poll(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;

			Name = entity.GetAttributeValue<string>("adx_name");
			Question = entity.GetAttributeValue<string>("adx_question");
			SubmitButtonLabel = entity.GetAttributeValue<string>("adx_submitbuttonlabel");

			CloseVotingDate = entity.GetAttributeValue<DateTime?>("adx_closevotingdate");

			WebTemplate = entity.GetAttributeValue<EntityReference>("adx_webtemplateid");
		}

		public DateTime? CloseVotingDate { get; private set; }

		[JsonIgnore]
		public Entity Entity { get; private set; }

		public Guid Id
		{
			get { return Entity.Id; }
		}

		public string Name { get; private set; }

		public IEnumerable<IPollOption> Options { get; set; }

		public string Question { get; private set; }

		public string SubmitButtonLabel { get; private set; }

		public IPollOption UserSelectedOption { get; set; }

		public int Votes
		{
			get { return Options == null ? 0 : Options.Sum(o => o.Votes.GetValueOrDefault(0)); }
		}

		[JsonIgnore]
		public EntityReference WebTemplate { get; private set; }
	}
}
