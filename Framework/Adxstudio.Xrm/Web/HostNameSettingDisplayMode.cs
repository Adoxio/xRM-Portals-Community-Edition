/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Text.RegularExpressions;
using System.Web;
using System.Web.WebPages;
using Adxstudio.Xrm.Text;

namespace Adxstudio.Xrm.Web
{
	public class HostNameSettingDisplayMode : DefaultDisplayMode
	{
		public HostNameSettingDisplayMode(string suffix, string hostName) : base(suffix)
		{
			HostName = hostName;
			ContextCondition = IsHostNameMatch;
		}

		protected string HostName { get; private set; }

		private bool IsHostNameMatch(HttpContextBase httpContext)
		{
			if (httpContext == null || httpContext.Request == null || httpContext.Request.Url == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(HostName))
			{
				return false;
			}

			return Mask.IsMatch(httpContext.Request.Url.Host, HostName, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		}
	}
}
