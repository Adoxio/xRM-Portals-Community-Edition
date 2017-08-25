/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm
{
	public static class Extensions
	{
		internal static void ThrowOnNull(this object obj, string paramName, string message = null)
		{
			if (obj == null) throw new ArgumentNullException(paramName, message);
		}

		internal static void ThrowOnNullOrWhitespace(this string obj, string paramName, string message = null)
		{
			if (string.IsNullOrWhiteSpace(obj)) throw new ArgumentNullException(paramName, message);
		}

		/// <summary>
		/// Divides <paramref name="source"/> into partitions of a given <paramref name="size"/>.
		/// </summary>
		public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (size <= 0) throw new ArgumentException("Value must be positive and non-zero.", "size");

			return Batch(source, size, x => x);
		}

		/// <summary>
		/// Divides <paramref name="source"/> into partitions of a given <paramref name="size"/>, and applies a projection to each partition.
		/// </summary>
		public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> @select)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (size <= 0) throw new ArgumentException("Value must be positive and non-zero.", "size");
			if (@select == null) throw new ArgumentNullException("select");

			return BatchIterator(source, size, @select);
		}

		private static IEnumerable<TResult> BatchIterator<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> @select)
		{
			TSource[] partition = null;
			var count = 0;

			foreach (var item in source)
			{
				if (partition == null)
				{
					partition = new TSource[size];
				}

				partition[count++] = item;

				if (count != size)
				{
					continue;
				}

				yield return @select(partition.Select(x => x));
			   
				partition = null;
				count = 0;
			}

			// Return the last bucket with whatever elements are left.
			if (partition != null && count > 0)
			{
				yield return @select(partition.Take(count));
			}
		}
	}
}
