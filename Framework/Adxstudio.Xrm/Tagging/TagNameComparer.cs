/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Tagging
{
	public class TagNameComparer : IEqualityComparer<string>
	{
		public int GetHashCode(string name)
		{
			return name.GetHashCode();
		}

		bool IEqualityComparer<string>.Equals(string nameA, string nameB)
		{
			return TagName.Equals(nameA, nameB);
		}
	}
}
