/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Web;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// Configuration settings for specifying custom <see cref="HttpCachePolicy"/> values.
	/// </summary>
	public sealed class HttpCachePolicyElement : ConfigurationElement
	{
		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propCacheExtension;
		private static readonly ConfigurationProperty _propCacheability;
		private static readonly ConfigurationProperty _propExpires;
		private static readonly ConfigurationProperty _propMaxAge;
		private static readonly ConfigurationProperty _propRevalidation;
		private static readonly ConfigurationProperty _propSlidingExpiration;
		private static readonly ConfigurationProperty _propValidUntilExpires;
		private static readonly ConfigurationProperty _propVaryByCustom;
		private static readonly ConfigurationProperty _propVaryByContentEncodings;
		private static readonly ConfigurationProperty _propVaryByHeaders;
		private static readonly ConfigurationProperty _propVaryByParams;

		static HttpCachePolicyElement()
		{
			_propCacheExtension = new ConfigurationProperty("cacheExtension", typeof(string), null, ConfigurationPropertyOptions.None);
			_propCacheability = new ConfigurationProperty("cacheability", typeof(HttpCacheability?), null, ConfigurationPropertyOptions.None);
			_propExpires = new ConfigurationProperty("expires", typeof(string), null, ConfigurationPropertyOptions.None);
			_propMaxAge = new ConfigurationProperty("maxAge", typeof(string), null, ConfigurationPropertyOptions.None);
			_propRevalidation = new ConfigurationProperty("revalidation", typeof(HttpCacheRevalidation?), null, ConfigurationPropertyOptions.None);
			_propSlidingExpiration = new ConfigurationProperty("slidingExpiration", typeof(bool?), null, ConfigurationPropertyOptions.None);
			_propValidUntilExpires = new ConfigurationProperty("validUntilExpires", typeof(bool?), null, ConfigurationPropertyOptions.None);
			_propVaryByCustom = new ConfigurationProperty("varyByCustom", typeof(string), null, ConfigurationPropertyOptions.None);
			_propVaryByContentEncodings = new ConfigurationProperty("varyByContentEncodings", typeof(string), null, ConfigurationPropertyOptions.None);
			_propVaryByHeaders = new ConfigurationProperty("varyByHeaders", typeof(string), null, ConfigurationPropertyOptions.None);
			_propVaryByParams = new ConfigurationProperty("varyByParams", typeof(string), null, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection
			{
				_propCacheExtension,
				_propCacheability,
				_propExpires,
				_propMaxAge,
				_propRevalidation,
				_propSlidingExpiration,
				_propValidUntilExpires,
				_propVaryByContentEncodings,
				_propVaryByHeaders,
				_propVaryByParams,
			};
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		/// <summary>
		/// Appends the specified text to the Cache-Control HTTP header.
		/// </summary>
		[ConfigurationProperty("cacheExtension", DefaultValue = null)]
		public string CacheExtension
		{
			get { return (string)base[_propCacheExtension]; }
			set { base[_propCacheExtension] = value; }
		}

		/// <summary>
		/// The 'Cache-Control' HTTP header value.
		/// </summary>
		[ConfigurationProperty("cacheability", DefaultValue = null)]
		public HttpCacheability? Cacheability
		{
			get { return (HttpCacheability?)base[_propCacheability]; }
			set { base[_propCacheability] = value; }
		}

		/// <summary>
		/// The 'Expires' HTTP header value in <see cref="DateTime"/> format.
		/// </summary>
		[ConfigurationProperty("expires", DefaultValue = null)]
		public string Expires
		{
			get { return (string)base[_propExpires]; }
			set { base[_propExpires] = value; }
		}

		/// <summary>
		/// The 'max-age' HTTP header value in <see cref="TimeSpan"/> format.
		/// </summary>
		[ConfigurationProperty("maxAge", DefaultValue = null)]
		public string MaxAge
		{
			get { return (string)base[_propMaxAge]; }
			set { base[_propMaxAge] = value; }
		}

		/// <summary>
		/// Sets the Cache-Control HTTP header to either the must-revalidate or the proxy-revalidate directives based on the supplied enumeration value.
		/// </summary>
		[ConfigurationProperty("revalidation", DefaultValue = null)]
		public HttpCacheRevalidation? Revalidation
		{
			get { return (HttpCacheRevalidation?)base[_propRevalidation]; }
			set { base[_propRevalidation] = value; }
		}

		/// <summary>
		/// Sets cache expiration to from absolute to sliding.
		/// </summary>
		[ConfigurationProperty("slidingExpiration", DefaultValue = null)]
		public bool? SlidingExpiration
		{
			get { return (bool?)base[_propSlidingExpiration]; }
			set { base[_propSlidingExpiration] = value; }
		}

		/// <summary>
		/// Specifies whether the ASP.NET cache should ignore HTTP Cache-Control headers sent by the client that invalidate the cache.
		/// </summary>
		[ConfigurationProperty("validUntilExpires", DefaultValue = null)]
		public bool? ValidUntilExpires
		{
			get { return (bool?)base[_propValidUntilExpires]; }
			set { base[_propValidUntilExpires] = value; }
		}

		/// <summary>
		/// Specifies a custom text string to vary cached output responses by.
		/// </summary>
		[ConfigurationProperty("varyByCustom", DefaultValue = null)]
		public string VaryByCustom
		{
			get { return (string)base[_propVaryByCustom]; }
			set { base[_propVaryByCustom] = value; }
		}

		/// <summary>
		/// A semicolon-separated list of 'Content-Encoding' headers used to vary the output cache.
		/// </summary>
		[ConfigurationProperty("varyByContentEncodings", DefaultValue = null)]
		public string VaryByContentEncodings
		{
			get { return (string)base[_propVaryByContentEncodings]; }
			set { base[_propVaryByContentEncodings] = value; }
		}

		/// <summary>
		/// A semicolon-separated list of HTTP headers used to vary the output cache.
		/// </summary>
		[ConfigurationProperty("varyByContentHeaders", DefaultValue = null)]
		public string VaryByHeaders
		{
			get { return (string)base[_propVaryByHeaders]; }
			set { base[_propVaryByHeaders] = value; }
		}

		/// <summary>
		/// A semicolon-separated list of parameters received by an HTTP GET or HTTP POST used to vary the output cache.
		/// </summary>
		[ConfigurationProperty("varyByParams", DefaultValue = null)]
		public string VaryByParams
		{
			get { return (string)base[_propVaryByParams]; }
			set { base[_propVaryByParams] = value; }
		}
	}
}
