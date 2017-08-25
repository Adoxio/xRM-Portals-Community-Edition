/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumThreadType : IForumThreadType
	{
		public ForumThreadType(EntityReference entityReference, string name, int displayOrder, bool allowsVoting, bool isDefault, bool requiresAnswer)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			EntityReference = entityReference;
			Name = name;
			DisplayOrder = displayOrder;
			AllowsVoting = allowsVoting;
			IsDefault = isDefault;
			RequiresAnswer = requiresAnswer;
		}

		public ForumThreadType(Entity entity) : this(
			entity.ToEntityReference(),
			entity.GetAttributeValue<string>("adx_name"),
			entity.GetAttributeValue<int?>("adx_displayorder").GetValueOrDefault(),
			entity.GetAttributeValue<bool?>("adx_allowsvoting").GetValueOrDefault(),
			entity.GetAttributeValue<bool?>("adx_isdefault").GetValueOrDefault(),
			entity.GetAttributeValue<bool?>("adx_requiresanswer").GetValueOrDefault())
		{
			if (entity == null) throw new ArgumentNullException("entity");
		}

		public bool AllowsVoting { get; private set; }

		public int DisplayOrder { get; private set; }

		public EntityReference EntityReference { get; private set; }

		public bool IsDefault { get; private set; }

		public string Name { get; private set; }

		public bool RequiresAnswer { get; private set; }
	}
}
