/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Linq;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// A collection of <see cref="ConfigurationElement"/> objects.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ConfigurationElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, new()
	{
		private static readonly ConfigurationProperty _propDefault = new ConfigurationProperty("default", typeof(string), null, ConfigurationPropertyOptions.None);
		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection { _propDefault };
		private static readonly T _defaultElement = new T();

		public override bool IsReadOnly()
		{
			return false;
		}

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public abstract string CollectionName { get; }

		/// <summary>
		/// Gets or sets the default element name.
		/// </summary>
		[ConfigurationProperty("default", DefaultValue = null)]
		public string Default
		{
			get { return (string)base[_propDefault]; }
			set { base[_propDefault] = value; }
		}

		/// <summary>
		/// Gets the selected or default element.
		/// </summary>
		public T Current
		{
			get
			{
				return !string.IsNullOrEmpty(Default)
					? this[Default]
					: this.Cast<T>().FirstOrDefault() ?? _defaultElement;
			}
		}

		protected ConfigurationElementCollection()
			: base(StringComparer.OrdinalIgnoreCase)
		{
			var attribute = GetConfigurationCollectionAttribute();

			if (attribute != null)
			{
				AddElementName = attribute.AddItemName;
			}
		}

		/// <summary>
		/// Retrieves the selected element or falls back to the default.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public T GetElementOrDefault(string name, bool allowDefaultFallback = false)
		{
			if (string.IsNullOrWhiteSpace(name)) return Current;
			if (this[name] != null) return this[name];
			if (allowDefaultFallback) return Current;

			throw new ConfigurationErrorsException("A configuration element with the name '{0}' under the '{1}' collection does not exist.".FormatWith(name, CollectionName));
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var pi = element.GetType().GetProperty("Name");
			return pi.GetValue(element, null);
		}

		public void Add(T element)
		{
			BaseAdd(element);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		public string GetKey(int index)
		{
			return (string)BaseGetKey(index);
		}

		public void Remove(string name)
		{
			BaseRemove(name);
		}

		public void Remove(T element)
		{
			BaseRemove(GetElementKey(element));
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public object[] AllKeys
		{
			get { return BaseGetAllKeys(); }
		}

		public T this[int index]
		{
			get { return (T)BaseGet(index); }

			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}

				BaseAdd(index, value);
			}
		}

		public new T this[string name]
		{
			get { return (T)BaseGet(name); }
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		protected override string ElementName
		{
			get
			{
				var attribute = GetConfigurationCollectionAttribute();
				return attribute != null ? attribute.AddItemName : base.ElementName;
			}
		}

		private ConfigurationCollectionAttribute GetConfigurationCollectionAttribute()
		{
			var attributes = GetType().GetCustomAttributes(typeof(ConfigurationCollectionAttribute), true).Cast<ConfigurationCollectionAttribute>();
			return attributes != null ? attributes.FirstOrDefault() : null;
		}
	}
}
