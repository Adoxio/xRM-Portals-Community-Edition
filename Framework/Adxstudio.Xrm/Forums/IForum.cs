/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForum : IPortalViewEntity, IForumInfo
	{
		new string Description { get; }

		Entity Entity { get; }

		string Name { get; }

		int PostCount { get; }

		int ThreadCount { get; }
	}
}
