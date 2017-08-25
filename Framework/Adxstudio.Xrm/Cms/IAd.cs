/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IAd
	{
		string Copy { get; }

		Entity Entity { get; }

		string ImageAlternateText { get; }

		int? ImageHeight { get; }

		string ImageUrl { get; }

		int? ImageWidth { get; }

		string Name { get; }

		bool OpenInNewWindow { get; }

		string RedirectUrl { get; }

		string Title { get; }

		EntityReference WebTemplate { get; }
	}
}
