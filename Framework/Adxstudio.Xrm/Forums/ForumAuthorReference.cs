/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class ForumAuthorReference : IForumAuthor
	{
		public ForumAuthorReference(EntityReference entityReference)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			EntityReference = entityReference;
		}

		public string DisplayName
		{
			get { throw new NotSupportedException(); }
		}

		public string EmailAddress
		{
			get { return null; }
		}

		public EntityReference EntityReference { get; set; }
	}
}
