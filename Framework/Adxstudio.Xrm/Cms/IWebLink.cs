/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IWebLink
	{
		IPortalViewAttribute Description { get; }

		bool DisplayImageOnly { get; }

		bool DisplayPageChildLinks { get; }

		Entity Entity { get; }

		IPortalViewAttribute Name { get; }

		string ImageAlternateText { get; }

		int? ImageHeight { get; }

		string ImageUrl { get; }

		int? ImageWidth { get; }

		bool IsExternal { get; }

		bool HasImage { get; }

		bool NoFollow { get; }

		bool OpenInNewWindow { get; }

		EntityReference Page { get; }

		string ToolTip { get; }

		string Url { get; }

		IEnumerable<IWebLink> WebLinks { get; }
	}
}
