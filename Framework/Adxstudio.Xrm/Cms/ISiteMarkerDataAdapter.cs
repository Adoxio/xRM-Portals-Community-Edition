/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface ISiteMarkerDataAdapter
	{
		ISiteMarkerTarget Select(string siteMarkerName);

		ISiteMarkerTarget SelectWithReadAccess(string siteMarkerName);
	}

	internal class RequestCachingSiteMarkerDataAdapter : RequestCachingDataAdapter, ISiteMarkerDataAdapter
	{
		private readonly ISiteMarkerDataAdapter _siteMarkers;

		public RequestCachingSiteMarkerDataAdapter(ISiteMarkerDataAdapter siteMarkers, EntityReference website) : base("{0}:{1}".FormatWith(siteMarkers.GetType().FullName, website.Id))
		{
			if (siteMarkers == null) throw new ArgumentNullException("siteMarkers");
			if (website == null) throw new ArgumentNullException("website");

			_siteMarkers = siteMarkers;
		}

		public ISiteMarkerTarget Select(string siteMarkerName)
		{
			return Get("Select:" + siteMarkerName, () => _siteMarkers.Select(siteMarkerName));
		}

		public ISiteMarkerTarget SelectWithReadAccess(string siteMarkerName)
		{
			return Get("SelectWithReadAccess:" + siteMarkerName, () => _siteMarkers.SelectWithReadAccess(siteMarkerName));
		}
	}
}
