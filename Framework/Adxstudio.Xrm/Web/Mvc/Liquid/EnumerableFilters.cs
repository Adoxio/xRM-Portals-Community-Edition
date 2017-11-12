/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class EnumerableFilters
	{
		public static object All(object input)
		{
			var blogPostsDrop = input as BlogPostsDrop;

			if (blogPostsDrop != null)
			{
				return BlogFunctions.All(blogPostsDrop);
			}

			var forumThreadsDrop = input as ForumThreadsDrop;

			if (forumThreadsDrop != null)
			{
				return ForumFunctions.All(forumThreadsDrop);
			}

			var forumPostsDrop = input as ForumPostsDrop;

			if (forumPostsDrop != null)
			{
				return ForumFunctions.All(forumPostsDrop);
			}

			var enumerable = input as IEnumerable;

			if (enumerable != null)
			{
				return enumerable;
			}

			return input;
		}

		public static IEnumerable Batch(IEnumerable input, int batchSize)
		{
			return input.Cast<object>().Batch(batchSize);
		}

		public static IEnumerable Concat(IEnumerable first, object second)
		{
			var firstEnumerable = first == null ? Enumerable.Empty<object>() : first.Cast<object>();
			var secondEnumerable = second as IEnumerable;

			return secondEnumerable == null
				? firstEnumerable.Concat(new[] { second })
				: firstEnumerable.Concat(secondEnumerable.Cast<object>());
		}

		public static IEnumerable Except(IEnumerable input, string key, object value)
		{
			return input.Cast<object>().Where(e => !KeyEquals(e, key, value));
		}

		public static IEnumerable GroupBy(IEnumerable input, string key)
		{
			return input.Cast<object>().GroupBy(e => Get(e, key)).Select(group => new Hash
			{
				{ "key", @group.Key },
				{ "items", @group.AsEnumerable() }
			});
		}

		public static object Orderby(object input, string key, string direction = "asc")
		{
			return OrderBy(input, key, direction);
		}

		public static object OrderBy(object input, string key, string direction = "asc")
		{
			var blogPostsDrop = input as BlogPostsDrop;

			if (blogPostsDrop != null)
			{
				return BlogFunctions.OrderBy(blogPostsDrop, key, direction);
			}

			var forumThreadsDrop = input as ForumThreadsDrop;

			if (forumThreadsDrop != null)
			{
				return ForumFunctions.OrderBy(forumThreadsDrop, key, direction);
			}

			var enumerable = input as IEnumerable;

			if (enumerable != null)
			{
				return string.Equals(direction, "desc", StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(direction, "descending", StringComparison.InvariantCultureIgnoreCase)
					? enumerable.Cast<object>().OrderByDescending(e => Get(e, key))
					: enumerable.Cast<object>().OrderBy(e => Get(e, key));
			}

			return input;
		}

		public static object TopLevel(object input, int pageSize)
		{
			var categoriesDrop = input as CategoriesDrop;

			if (categoriesDrop != null)
			{
				return CategoryFunctions.TopLevel(categoriesDrop, pageSize);
			}

			return input;
		}

		public static object CategoryNumber(object input, string categoryNumber)
		{
			var categoriesDrop = input as CategoriesDrop;

			if (categoriesDrop != null)
			{
				return CategoryFunctions.CategoryNumber(categoriesDrop, categoryNumber);
			}

			return input;
		}

		public static object Related(object input, string categoryIdString, int pageSize)
		{
			var categoriesDrop = input as CategoriesDrop;
			Guid categoryId;
			
			if (categoriesDrop != null && Guid.TryParse(categoryIdString, out categoryId))
			{
				return CategoryFunctions.Related(categoriesDrop, categoryId, pageSize);
			}

			return input;
		}


		public static object Top(object input, int pageSize, string lang)
		{
			var articlesDrop = input as KnowledgeArticlesDrop;

			if (articlesDrop != null)
			{
				return KnowledgeArticleFunctions.Top(articlesDrop, pageSize, lang);
			}

			return input;
		}

		public static object Recent(object input, int pageSize, string lang)
		{
			var articlesDrop = input as KnowledgeArticlesDrop;

			if (articlesDrop != null)
			{
				return KnowledgeArticleFunctions.Recent(articlesDrop, pageSize, lang);
			}

			var categoriesDrop = input as CategoriesDrop;

			if (categoriesDrop != null)
			{
				return CategoryFunctions.Recent(categoriesDrop, pageSize);
			}

			return input;
		}

		public static object Popular(object input, int pageSize, string lang)
		{
			var articlesDrop = input as KnowledgeArticlesDrop;

			if (articlesDrop != null)
			{
				return KnowledgeArticleFunctions.Popular(articlesDrop, pageSize, lang);
			}

			return input;
		}

		public static object Paginate(object input, int index, int count)
		{
			var blogPostsDrop = input as BlogPostsDrop;

			if (blogPostsDrop != null)
			{
				return BlogFunctions.Paginate(blogPostsDrop, index, count);
			}

			var forumThreadsDrop = input as ForumThreadsDrop;

			if (forumThreadsDrop != null)
			{
				return ForumFunctions.Paginate(forumThreadsDrop, index, count);
			}

			var forumPostsDrop = input as ForumPostsDrop;

			if (forumPostsDrop != null)
			{
				return ForumFunctions.Paginate(forumPostsDrop, index, count);
			}

			var enumerable = input as IEnumerable;

			if (enumerable != null)
			{
				return enumerable.Cast<object>().Skip(index).Take(count);
			}

			return input;
		}

		public static object Random(Context context, IEnumerable input)
		{
			IPortalLiquidContext portalLiquidContext;

			var random = context.TryGetPortalLiquidContext(out portalLiquidContext)
				? portalLiquidContext.Random
				: new Random();

			var items = input.Cast<object>().ToArray();

			return items.ElementAt(random.Next(items.Length));
		}

		public static IEnumerable Select(IEnumerable input, string key)
		{
			return input.Cast<object>().Select(e => Get(e, key));
		}

		public static IEnumerable Shuffle(Context context, IEnumerable input)
		{
			IPortalLiquidContext portalLiquidContext;

			var random = context.TryGetPortalLiquidContext(out portalLiquidContext)
				? portalLiquidContext.Random
				: new Random();

			return input.Cast<object>().ShuffleIterator(random);
		}

		private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random random)
		{
			var buffer = source.ToList();

			for (var i = 0; i < buffer.Count; i++)
			{
				var j = random.Next(i, buffer.Count);

				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}

		public static object Skip(object input, int count)
		{
			var blogPostsDrop = input as BlogPostsDrop;

			if (blogPostsDrop != null)
			{
				return BlogFunctions.FromIndex(blogPostsDrop, count);
			}

			var forumThreadsDrop = input as ForumThreadsDrop;

			if (forumThreadsDrop != null)
			{
				return ForumFunctions.FromIndex(forumThreadsDrop, count);
			}

			var forumPostsDrop = input as ForumPostsDrop;

			if (forumPostsDrop != null)
			{
				return ForumFunctions.FromIndex(forumPostsDrop, count);
			}

			var enumerable = input as IEnumerable;

			if (enumerable != null)
			{
				return enumerable.Cast<object>().Skip(count);
			}

			return input;
		}

		public static object Take(object input, int count)
		{
			var blogPostsDrop = input as BlogPostsDrop;

			if (blogPostsDrop != null)
			{
				return BlogFunctions.Take(blogPostsDrop, count);
			}

			var forumThreadsDrop = input as ForumThreadsDrop;

			if (forumThreadsDrop != null)
			{
				return ForumFunctions.Take(forumThreadsDrop, count);
			}

			var forumPostsDrop = input as ForumPostsDrop;

			if (forumPostsDrop != null)
			{
				return ForumFunctions.Take(forumPostsDrop, count);
			}

			var enumerable = input as IEnumerable;

			if (enumerable != null)
			{
				return enumerable.Cast<object>().Take(count);
			}

			return input;
		}

		public static IEnumerable Thenby(IOrderedEnumerable<object> input, string key, string direction = "asc")
		{
			return ThenBy(input, key, direction);
		}

		public static IEnumerable ThenBy(IOrderedEnumerable<object> input, string key, string direction = "asc")
		{
			return string.Equals(direction, "desc", StringComparison.InvariantCultureIgnoreCase)
				|| string.Equals(direction, "descending", StringComparison.InvariantCultureIgnoreCase)
				? input.ThenByDescending(e => Get(e, key))
				: input.ThenBy(e => Get(e, key));
		}

		public static IEnumerable Where(IEnumerable input, string key, object value)
		{
			return input.Cast<object>().Where(e => KeyEquals(e, key, value));
		}

		private static object Get(object input, string key)
		{
			if (input == null || key == null)
			{
				return null;
			}

			var hash = input as Hash;

			if (hash != null)
			{
				return hash[key];
			}

			var dictionary = input as IDictionary<string, object>;

			if (dictionary != null)
			{
				object value;

				return dictionary.TryGetValue(key, out value) ? value : null;
			}

			var drop = input as Drop;

			if (drop != null)
			{
				var dotIndex = key.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);

				if (dotIndex > 0 && dotIndex < (key.Length - 1))
				{
					return Get(drop.InvokeDrop(key.Substring(0, dotIndex)), key.Substring(dotIndex + 1));
				}

				return drop.InvokeDrop(key);
			}

			var liquidizable = input as ILiquidizable;

			if (liquidizable != null)
			{
				return Get(liquidizable.ToLiquid(), key);
			}

			return null;
		}

		private static bool KeyEquals(object @object, string key, object value)
		{
			var propertyValue = Get(@object, key);

			return propertyValue == null ? value == null : propertyValue.Equals(value);
		}
	}
}
