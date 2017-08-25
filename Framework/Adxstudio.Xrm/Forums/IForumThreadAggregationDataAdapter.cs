/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Tagging;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumThreadAggregationDataAdapter
	{
		int SelectPostCount();

		int SelectThreadCount();

		IEnumerable<IForumThread> SelectThreads();

		IEnumerable<IForumThread> SelectThreads(int startRowIndex, int maximumRows = -1);

		IEnumerable<IForumThreadWeightedTag> SelectWeightedTags(int weights);
	}

	public interface IForumThreadWeightedTag : IForumThreadTag
	{
		int ThreadCount { get; }

		int Weight { get; }
	}

	internal class ForumThreadWeightedTag : ForumThreadTag, IForumThreadWeightedTag
	{
		public ForumThreadWeightedTag(string name, int threadCount, int weight) : base(name)
		{
			ThreadCount = threadCount;
			Weight = weight;
		}

		public int ThreadCount { get; private set; }

		public int Weight { get; private set; }
	}

	internal class ForumThreadWeightedTagInfo : ITagInfo
	{
		public ForumThreadWeightedTagInfo(string name, int count)
		{
			Name = name;
			TaggedItemCount = count;
		}

		public string Name { get; private set; }

		public int TaggedItemCount { get; private set; }
	}
}
