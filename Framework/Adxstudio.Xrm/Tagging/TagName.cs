/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Tagging
{
	public static class TagName
	{
		public static StringComparison Comparison
		{
			get { return StringComparison.InvariantCultureIgnoreCase; }
		}

		public static bool Equals(string nameA, string nameB)
		{
			return string.Equals(nameA, nameB, Comparison);
		}
	}
}
