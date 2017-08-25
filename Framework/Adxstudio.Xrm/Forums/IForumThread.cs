/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumThread : IPortalViewEntity, IForumThreadInfo
	{
		Entity Entity { get; }

		bool IsAnswered { get; }

		bool IsSticky { get; }

		bool Locked { get; }

		string Name { get; }

		int PostCount { get; }

		int ReplyCount { get; }
	}
}
