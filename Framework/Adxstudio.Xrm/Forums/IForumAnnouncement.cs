/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumAnnouncement : IPortalViewEntity
	{
		string Content { get; }

		Entity Entity { get; }

		string Name { get; }

		DateTime? PostedOn { get; }
	}
}
