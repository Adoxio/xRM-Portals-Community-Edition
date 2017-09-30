/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Forums
{
	public class ForumCounts : Tuple<int, int>
	{
		public ForumCounts(int threadCount, int postCount) : base(threadCount, postCount) { }

		public int PostCount
		{
			get { return Item2; }
		}

		public int ThreadCount
		{
			get { return Item1; }
		}
	}
}
