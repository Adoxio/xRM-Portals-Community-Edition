/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Activity
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Class that contains utilities relating to PortalTimeline solution
	/// </summary>
	public class PortalTimeline
	{
		/// <summary>
		/// Whitelist of <see cref="PortalTimeline"/> enabled Portals by Website Guid
		/// </summary>
		public static List<Guid> EnabledPortalsByWebsiteGuid = new List<Guid>()
		{
			new Guid("2ab10dab-d681-4911-b881-cc99413f07b6"), // CommunityPortalWebsiteGuid
			new Guid("6d6b3012-e709-4c45-a00d-df4b3befc518"), // PartnerPortalWebsiteGuid
			new Guid("7b138792-1090-45b6-9241-8f8d96d8c372"), // CustomerPortalWebsiteGuid
			new Guid("10152feb-f33d-4cbd-997e-f7a336c3b8bf"), // EssPortalWebsiteGuid
			new Guid("2ab10dab-d681-4911-b881-cc99413f07b6")  // SuperPortalWebsiteGuid
		};
	}
}
