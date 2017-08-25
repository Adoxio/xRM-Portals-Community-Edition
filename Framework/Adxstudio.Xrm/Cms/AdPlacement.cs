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
	public class AdPlacement : IAdPlacement
	{
		public AdPlacement(Entity entity, IEnumerable<IAd> ads)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;
			Ads = ads.ToArray();

			Name = entity.GetAttributeValue<string>("adx_name");
			WebTemplate = entity.GetAttributeValue<EntityReference>("adx_webtemplateid");
		}

		[JsonIgnore]
		public Entity Entity { get; private set; }

		public string Name { get; private set; }

		public IEnumerable<IAd> Ads { get; private set; }

		public EntityReference WebTemplate { get; private set; }
	}
}
