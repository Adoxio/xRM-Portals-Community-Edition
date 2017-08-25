/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class Setting : ISetting
	{
		public Setting(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;
			Name = entity.GetAttributeValue<string>("adx_name");
			Value = entity.GetAttributeValue<string>("adx_value");
		}

		public Entity Entity { get; private set; }

		public string Name { get; private set; }

		public string Value { get; private set; }
	}
}
