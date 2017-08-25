/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumThreadType
	{
		bool AllowsVoting { get; }

		int DisplayOrder { get; }

		EntityReference EntityReference { get; }

		bool IsDefault { get; }

		string Name { get; }

		bool RequiresAnswer { get; }
	}

	internal class UnknownForumThreadType : IForumThreadType
	{
		public bool AllowsVoting
		{
			get { return false; }
		}

		public int DisplayOrder
		{
			get { return 0; }
		}

		public EntityReference EntityReference
		{
			get { return null; }
		}

		public bool IsDefault
		{
			get { return false; }
		}

		public string Name
		{
			get { return string.Empty; }
		}

		public bool RequiresAnswer
		{
			get { return false; }
		}
	}
}
