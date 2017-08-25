/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumThreadTag : IForumThreadTag
	{
		public ForumThreadTag(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value can't be null or empty.", "name");

			Name = name;
		}

		public string Name { get; private set; }
	}
}
