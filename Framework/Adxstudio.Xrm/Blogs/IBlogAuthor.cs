/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogAuthor
	{
		ApplicationPath ApplicationPath { get; }

		string EmailAddress { get; }

		Guid Id { get; }

		string Name { get; }
	}
}
