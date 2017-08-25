/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Represents data for the resolution of a case.
	/// </summary>
	public interface ICaseResolution
	{
		DateTime CreatedOn { get; }

		string Description { get; }

		string Subject { get; }
	}
}
