/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumAuthor
	{
		string DisplayName { get; }

		string EmailAddress { get; }

		EntityReference EntityReference { get; }
	}

	internal class UnknownForumAuthor : IForumAuthor
	{
		public string DisplayName
		{
			get { return string.Empty; }
		}

		public string EmailAddress
		{
			get { return null; }
		}

		public EntityReference EntityReference
		{
			get { return null; }
		}
	}
}
