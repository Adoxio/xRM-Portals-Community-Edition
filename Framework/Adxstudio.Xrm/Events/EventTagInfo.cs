/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	public class EventTagInfo : TagInfo
	{
		public EventTagInfo(Entity crmEntity)
			: base("adx_name", "adx_eventtag_event", crmEntity)
		{
		}
	}
}
