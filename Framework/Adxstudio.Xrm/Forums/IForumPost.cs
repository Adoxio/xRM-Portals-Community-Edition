/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumPost : IPortalViewEntity, IForumPostInfo
	{
		string Content { get; }

		bool CanEdit { get; }

		bool CanMarkAsAnswer { get; }

		ApplicationPath DeletePath { get; }

		ApplicationPath EditPath { get; }

		Entity Entity { get; }

		int HelpfulVoteCount { get; }

		bool IsAnswer { get; }

		string Name { get; }

		IForumThread Thread { get; }
	}
}
