/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class ForumThreadTypeReference : IForumThreadType
	{
		public ForumThreadTypeReference(EntityReference entityReference)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			EntityReference = entityReference;
		}

		public bool AllowsVoting
		{
			get { throw new NotSupportedException(); }
		}

		public int DisplayOrder
		{
			get { throw new NotSupportedException(); }
		}

		public EntityReference EntityReference { get; private set; }

		public bool IsDefault
		{
			get { throw new NotSupportedException(); }
		}

		public string Name
		{
			get { throw new NotSupportedException(); }
		}

		public bool RequiresAnswer
		{
			get { throw new NotSupportedException(); }
		}
	}
}
