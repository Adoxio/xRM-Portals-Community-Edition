/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;

namespace Adxstudio.Xrm.Cms
{
	internal abstract class RequestCachingDataAdapter
	{
		private readonly string _key;

		protected RequestCachingDataAdapter(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			_key = key;
		}

		protected T Get<T>(string key, Func<T> get)
		{
			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return get();
			}

			var cache = httpContext.Items[_key] as IDictionary<string, object>;

			T value;

			if (cache == null)
			{
				value = get();

				httpContext.Items[_key] = new Dictionary<string, object>(StringComparer.Ordinal)
				{
					{ key, value }
				};

				return value;
			}

			object cached;

			if (cache.TryGetValue(key, out cached))
			{
				return (T)cached;
			}

			value = get();

			cache[key] = value;

			return value;
		}
	}
}
