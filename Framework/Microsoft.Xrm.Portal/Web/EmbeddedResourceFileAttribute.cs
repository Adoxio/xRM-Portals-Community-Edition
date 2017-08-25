/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Portal.Web
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class EmbeddedResourceFileAttribute : Attribute
	{
		public string ResourceName { get; private set; }
		public string VirtualPath { get; private set; }

		public EmbeddedResourceFileAttribute(string resourceName, string virtualPath)
		{
			ResourceName = resourceName;
			VirtualPath = virtualPath;
		}
	}
}


