/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class ForumAuthor : IForumAuthor
	{
		public ForumAuthor(EntityReference entityReference, string displayName, string emailAddress)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			EntityReference = entityReference;
			DisplayName = displayName;
			EmailAddress = emailAddress;
		}

		public string DisplayName { get; private set; }

		public string EmailAddress { get; set; }

		public EntityReference EntityReference { get; private set; }
	}
}
