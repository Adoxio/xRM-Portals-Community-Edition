/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Performance
{
	internal interface IPerformanceMarkerIdGenerator
	{
		string GetId();
	}

	internal class GuidIdGenerator : IPerformanceMarkerIdGenerator
	{
		public string GetId()
		{
			return Guid.NewGuid().ToString("D");
		}
	}
}
