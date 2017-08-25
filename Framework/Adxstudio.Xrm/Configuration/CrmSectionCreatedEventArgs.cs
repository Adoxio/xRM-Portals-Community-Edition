/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// Arguments for the configuration creation event.
	/// </summary>
	public class CrmSectionCreatedEventArgs : EventArgs
	{
		public CrmSection Configuration { get; set; }
	}
}
