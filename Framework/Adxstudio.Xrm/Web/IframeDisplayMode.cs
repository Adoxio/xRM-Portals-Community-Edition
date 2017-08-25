/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web
{
	public class IframeDisplayMode : HostNameSettingDisplayMode
	{
		public IframeDisplayMode(string hostName)
			: base("iframe", hostName) { }
	}
}
