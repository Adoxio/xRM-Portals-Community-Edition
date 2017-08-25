/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Configuration;
using Microsoft.Xrm.Client.Services;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// The modes in which the <see cref="CrmConfigurationManager"/> instantiates <see cref="ObjectCache"/> objects.
	/// </summary>
	public enum ObjectCacheInstanceMode
	{
		/// <summary>
		/// Create a static instance.
		/// </summary>
		Static,

		/// <summary>
		/// Create an instance for each element name.
		/// </summary>
		PerName,

		/// <summary>
		/// Create an instance on every invocation.
		/// </summary>
		PerInstance,
	}

	/// <summary>
	/// The configuration settings for <see cref="ObjectCache"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="CrmConfigurationManager"/>.
	/// </remarks>
	/// <seealso cref="CrmConfigurationManager"/>
	public sealed class ObjectCacheElement : InitializableConfigurationElement<ObjectCache>, ICacheItemPolicyFactory
	{
		/// <summary>
		/// The default element name.
		/// </summary>
		public const string DefaultObjectCacheName = "Microsoft.Xrm.Client";

		private const string _defaultObjectCacheTypeName = "System.Runtime.Caching.MemoryCache, System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationProperty _propInstanceMode;
		private static readonly ConfigurationProperty _propAbsoluteExpiration;
		private static readonly ConfigurationProperty _propSlidingExpiration;
		private static readonly ConfigurationProperty _propDuration;
		private static readonly ConfigurationProperty _propPriority;
		private static readonly ConfigurationProperty _propOutputCacheProfileName;

		static ObjectCacheElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propType = new ConfigurationProperty("type", typeof(string), _defaultObjectCacheTypeName, ConfigurationPropertyOptions.None);
			_propInstanceMode = new ConfigurationProperty("instanceMode", typeof(ObjectCacheInstanceMode), ObjectCacheInstanceMode.PerName, ConfigurationPropertyOptions.None);
			_propAbsoluteExpiration = new ConfigurationProperty("absoluteExpiration", typeof(DateTimeOffset), ObjectCache.InfiniteAbsoluteExpiration, ConfigurationPropertyOptions.None);
			_propSlidingExpiration = new ConfigurationProperty("slidingExpiration", typeof(TimeSpan), ObjectCache.NoSlidingExpiration, ConfigurationPropertyOptions.None);
			_propDuration = new ConfigurationProperty("duration", typeof(TimeSpan?), null, ConfigurationPropertyOptions.None);
			_propPriority = new ConfigurationProperty("priority", typeof(CacheItemPriority), CacheItemPriority.Default, ConfigurationPropertyOptions.None);
			_propOutputCacheProfileName = new ConfigurationProperty("outputCacheProfileName", typeof(string), null, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection
				{
					_propName,
					_propType,
					_propInstanceMode,
					_propAbsoluteExpiration,
					_propSlidingExpiration,
					_propDuration,
					_propPriority,
					_propOutputCacheProfileName,
				};
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets or sets the element name.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = null, IsKey = true, IsRequired = true)]
		public override string Name
		{
			get { return (string)base[_propName]; }
			set { base[_propName] = value; }
		}

		/// <summary>
		/// The dependency type name.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue = _defaultObjectCacheTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// The instance mode.
		/// </summary>
		[ConfigurationProperty("instanceMode", DefaultValue = ObjectCacheInstanceMode.PerName)]
		public ObjectCacheInstanceMode InstanceMode
		{
			get { return (ObjectCacheInstanceMode)base[_propInstanceMode]; }
			set { base[_propInstanceMode] = value; }
		}

		/// <summary>
		/// The cache policy absolute expiration date.
		/// </summary>
		[ConfigurationProperty("absoluteExpiration")]
		public DateTimeOffset AbsoluteExpiration
		{
			get { return (DateTimeOffset)base[_propAbsoluteExpiration]; }
			set { base[_propAbsoluteExpiration] = value; }
		}

		/// <summary>
		/// The cache policy sliding expiration.
		/// </summary>
		[ConfigurationProperty("slidingExpiration")]
		public TimeSpan SlidingExpiration
		{
			get { return (TimeSpan)base[_propSlidingExpiration]; }
			set { base[_propSlidingExpiration] = value; }
		}

		/// <summary>
		/// The cache policy cache duration.
		/// </summary>
		/// <remarks>
		/// This value overrides the absolute expiration value as well as the duration specified by the OutputCacheProfileName property.
		/// </remarks>
		[ConfigurationProperty("duration")]
		public TimeSpan? Duration
		{
			get { return (TimeSpan?)base[_propDuration]; }
			set { base[_propDuration] = value; }
		}

		/// <summary>
		/// The cache policy priority.
		/// </summary>
		[ConfigurationProperty("priority", DefaultValue = CacheItemPriority.Default)]
		public CacheItemPriority Priority
		{
			get { return (CacheItemPriority)base[_propPriority]; }
			set { base[_propPriority] = value; }
		}

		/// <summary>
		/// The name of the <see cref="OutputCacheProfile"/> from which the cache profile duration is obtained.
		/// </summary>
		/// <remarks>
		/// Only the duration value of the <see cref="OutputCacheProfile"/> is used. This value overrides the absolute expiration value.
		/// </remarks>
		[ConfigurationProperty("outputCacheProfileName")]
		public string OutputCacheProfileName
		{
			get { return (string)base[_propOutputCacheProfileName]; }
			set { base[_propOutputCacheProfileName] = value; }
		}

		/// <summary>
		/// Creates a <see cref="ObjectCache"/> object.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public ObjectCache CreateObjectCache(string objectCacheName = null)
		{
			var name = objectCacheName ?? Name ?? DefaultObjectCacheName;

			return CreateDependencyAndInitialize(
				() => new MemoryCache(name, Parameters),
				name, Parameters);
		}

		/// <summary>
		/// Creates a <see cref="CacheItemPolicy"/> object.
		/// </summary>
		/// <returns></returns>
		public CacheItemPolicy CreateCacheItemPolicy()
		{
			var duration = GetDuration();

			return new CacheItemPolicy
			{
				AbsoluteExpiration = duration != null ? DateTimeOffset.UtcNow + duration.Value : AbsoluteExpiration,
				SlidingExpiration = SlidingExpiration,
				Priority = Priority,
			};
		}

		private TimeSpan? GetDuration()
		{
			if (Duration != null) return Duration.Value;

			if (!string.IsNullOrWhiteSpace(OutputCacheProfileName))
			{
				var section = ConfigurationManager.GetSection("system.web/caching/outputCacheSettings") as OutputCacheSettingsSection;

				if (section != null)
				{
					var profile = section.OutputCacheProfiles.Cast<OutputCacheProfile>().FirstOrDefault(p => p.Name == OutputCacheProfileName);

					if (profile != null && profile.Enabled && profile.Duration >= 0)
					{
						return new TimeSpan(0, 0, profile.Duration);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Creates a <see cref="CacheItemPolicy"/> object.
		/// </summary>
		/// <returns></returns>
		CacheItemPolicy ICacheItemPolicyFactory.Create()
		{
			return CreateCacheItemPolicy();
		}
	}
}
