/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

//Begin Internal Documentation

using System.Diagnostics;
using System.Web.Hosting;

namespace Microsoft.Xrm.Portal.Web.Security
{
	internal class Utility
	{
		public static string GetDefaultApplicationName()
		{
			try
			{
				string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;

				if (!string.IsNullOrEmpty(applicationVirtualPath)) return applicationVirtualPath;

				applicationVirtualPath = Process.GetCurrentProcess().MainModule.ModuleName;

				int index = applicationVirtualPath.IndexOf('.');

				if (index != -1)
				{
					applicationVirtualPath = applicationVirtualPath.Remove(index);
				}

				if (!string.IsNullOrEmpty(applicationVirtualPath)) return applicationVirtualPath;

				return "/";
			}
			catch
			{
				return "/";
			}
		}
	}
}

//End Internal Documentation
