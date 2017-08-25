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
	public class PollPlacement : IPollPlacement
	{
		public PollPlacement(Entity entity, IEnumerable<IPoll> polls)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;
			Name = entity.GetAttributeValue<string>("adx_name");
			Polls = polls.ToArray();
			WebTemplate = entity.GetAttributeValue<EntityReference>("adx_webtemplate");
		}

		[JsonIgnore]
		public Entity Entity { get; private set; }

		public string Name { get; private set; }

		public IEnumerable<IPoll> Polls { get; private set; }

		public EntityReference WebTemplate { get; private set; }
	}
}
