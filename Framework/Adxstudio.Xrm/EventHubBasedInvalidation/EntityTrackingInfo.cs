/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Stores entity tracking info.
	/// </summary>
	public class EntityTrackingInfo
	{
		/// <summary>
		/// Website lookup attribite name (ex: adx_websiteid)
		/// </summary>
		public string WebsiteLookupAttribute { get; set; }

		/// <summary>
		/// Entity key attribute name (ex: adx_webpageid for adx_webpage)
		/// </summary>
		public string EntityKeyAttribute { get; set; }
	}
}
