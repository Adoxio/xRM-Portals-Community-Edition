/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IComment : IRateable
	{
		IAuthor Author { get; }

		ApplicationPath DeletePath { get; }

		ApplicationPath EditPath { get; }

		bool Editable { get; }

		Entity Entity { get; }

		bool IsApproved { get; }

		string Content { get; }

		DateTime Date { get; }
	}
}
