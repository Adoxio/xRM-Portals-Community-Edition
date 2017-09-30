/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Tagging
{
	/// <summary>
	/// Provides an enumerable collection of <see cref="TagCloudDataItem"/>s, with <see cref="ITagInfo"/>s
	/// merged (and associated counts aggregated) based on a provided tag name <see cref="IEqualityComparer{T}"/>.
	/// Enumerated <see cref="TagCloudDataItem"/>s are then assigned weights relative to all other tags in the
	/// collection.
	/// </summary>
	public class TagCloudData : IEnumerable<TagCloudDataItem>
	{
		private IEnumerable<TagCloudDataItem> _items;
		private readonly IEnumerable<ITagInfo> _tags;

		/// <param name="numberOfWeights">
		/// The number of weight "buckets" from which weights will be assigned to each item.
		/// </param>
		/// <param name="tagNameEqualityComparer">
		/// The <see cref="IEqualityComparer{T}"/> used to compare tag names for equality. (The TaggedItemCounts of tags
		/// determined equal are summed, and the tags are merged into one data item.)
		/// </param>
		/// <param name="tags">
		/// The <see cref="ITagInfo"/>s that will be converted to weighted <see cref="TagCloudDataItem"/>s on enumeration.
		/// </param>
		public TagCloudData(int numberOfWeights, IEqualityComparer<string> tagNameEqualityComparer, IEnumerable<ITagInfo> tags)
		{
			NumberOfWeights = numberOfWeights;

			if (tagNameEqualityComparer == null)
			{
				throw new ArgumentNullException("tagNameEqualityComparer");
			}

			TagNameEqualityComparer = tagNameEqualityComparer;

			if (tags == null)
			{
				throw new ArgumentNullException("tags");
			}

			_tags = tags;
		}

		/// <param name="numberOfWeights">
		/// The number of weight "buckets" from which weights will be assigned to each item.
		/// </param>
		/// <param name="tagNameEqualityComparer">
		/// The <see cref="IEqualityComparer{T}"/> used to compare tag names for equality. (The TaggedItemCounts of tags
		/// determined equal are summed, and the tags are merged into one data item.)
		/// </param>
		/// <param name="tagSets">
		/// Multiple sets of <see cref="ITagInfo"/>s, which will be flattened into one collection.
		/// </param>
		public TagCloudData(int numberOfWeights, IEqualityComparer<string> tagNameEqualityComparer, params IEnumerable<ITagInfo>[] tagSets)
			: this(numberOfWeights, tagNameEqualityComparer, tagSets.SelectMany(tags => tags)) { }

		/// <summary>
		/// Gets the number of weight "buckets" from which weights will be assigned to items.
		/// </summary>
		public int NumberOfWeights { get; private set; }

		/// <summary>
		/// Gets the <see cref="IEqualityComparer{T}"/> used to compare tag names for equality.
		/// </summary>
		public IEqualityComparer<string> TagNameEqualityComparer { get; private set; }

		public IEnumerator<TagCloudDataItem> GetEnumerator()
		{
			if (_items != null)
			{
				return _items.GetEnumerator();
			}

			var items = _tags
				.GroupBy(tag => tag.Name, TagNameEqualityComparer)
				.Select(group => new TagCloudDataItem { Name = group.Key, TaggedItemCount = group.Sum(tag => tag.TaggedItemCount) });

			_items = AssignWeights(NumberOfWeights, items);

			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Assigns a weight to each <see cref="TagCloudDataItem"/> in a collection.
		/// </summary>
		/// <param name="items">The <see cref="TagCloudDataItem"/>s to which weights are to be assigned.</param>
		/// <param name="numberOfWeights">The number of weight groups/tiers from which to calculate weights.</param>
		/// <returns>The same <see cref="TagCloudDataItem"/>s, with the Weight property assigned.</returns>
		/// <remarks>
		/// This operation uses a logarithmic thresholding algorithm to group items into weight "buckets". This
		/// is intended to "smooth out" the distribution curve of weights, particularly when applied to a collection
		/// having the Pareto/Power Law distribution common to things like tags. See
		/// http://www.echochamberproject.com/node/247 and http://www.car-chase.net/2007/jan/16/log-based-tag-clouds-python/
		/// for further explanation of this choice.
		/// </remarks>
		protected virtual IEnumerable<TagCloudDataItem> AssignWeights(int numberOfWeights, IEnumerable<TagCloudDataItem> items)
		{
            // The call to Max on the next line will fail if there are no items,
            // so return if the collection is empty
            if (items.Count() == 0) return items;

			// Find the highest count in our collection--items with this count will have
			// the max weight (i.e., the value of the "weights" param)
			var maxFrequency = items.Max(item => item.TaggedItemCount);

			// Find the lowest count in our collection--items with this count will have a
			// weight of 1
			var minFrequency = items.Min(item => item.TaggedItemCount);

			// The size of each frequency threshold
			var delta = (maxFrequency - minFrequency) / (double)numberOfWeights;

			return items.Select(item =>
			{
				for (var weight = 1; weight <= numberOfWeights; weight++)
				{
					// We add 2 to each threshold and adjustedFrequency, to cancel out the
					// possibility of log(0) or log(1), which would have distorting effects

					var threshold = 100 * Math.Log((minFrequency + weight * delta) + 2);

					var adjustedFrequency = 100 * Math.Log(item.TaggedItemCount + 2);

					if (adjustedFrequency <= threshold)
					{
						item.Weight = weight;

						break;
					}
				}

				return item;
			});
		}
	}
}
