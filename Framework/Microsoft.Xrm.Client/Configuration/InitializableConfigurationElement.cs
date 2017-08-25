/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// The base <see cref="ConfigurationElement"/> class capable of instantiating dependencies based on its settings.
	/// </summary>
	/// <remarks>
	/// The dependency class should implement <see cref="IInitializable"/> if custom initialization is needed.
	/// </remarks>
	/// <typeparam name="TDependency">The dependency class.</typeparam>
	public abstract class InitializableConfigurationElement<TDependency> : ConfigurationElement
	{
		private readonly Lazy<NameValueCollection> _parameters = new Lazy<NameValueCollection>(() => new NameValueCollection(StringComparer.Ordinal));

		/// <summary>
		/// The attributes of the configuration element.
		/// </summary>
		public NameValueCollection Parameters
		{
			get { return _parameters.Value; }
		}

		/// <summary>
		/// The name of the configuration element.
		/// </summary>
		public abstract string Name { get; set; }

		/// <summary>
		/// The dependency type name.
		/// </summary>
		public abstract string Type { get; set; }

		/// <summary>
		/// The dependency type.
		/// </summary>
		public Type DependencyType
		{
			get { return GetDependencyType(); }
		}

		private Type _dependencyType;

		protected Type GetDependencyType()
		{
			if (_dependencyType == null)
			{
				var type = TypeExtensions.GetType(Type);

				if (type == null || !type.IsA<TDependency>())
				{
					throw new ConfigurationErrorsException("The value '{0}' is not recognized as a valid type or is not of the type '{1}'.".FormatWith(Type, typeof(TDependency)));
				}

				_dependencyType = type;
			}

			return _dependencyType;
		}

		protected TDependency CreateDependency<TDefault>(Func<TDefault> createDefault, params object[] args)
			where TDefault : TDependency
		{
			var type = GetDependencyType();
			var obj = CreateDependency(type, createDefault, args);
			return obj;
		}

		protected TDependency CreateDependencyAndInitialize<TDefault>(Func<TDefault> createDefault, params object[] args)
			where TDefault : TDependency
		{
			var obj = CreateDependency(createDefault, args);
			return Initialize(obj);
		}

		private static TDependency CreateDependency<TDefault>(
			Type type,
			Func<TDefault> createDefault,
			params object[] args)
			where TDefault : TDependency
		{
			if (type == typeof(TDefault)) return createDefault();
			if (type.IsA(typeof(TDefault))) return (TDependency)Activator.CreateInstance(type, args);
			return (TDependency)Activator.CreateInstance(type);
		}

		protected TDependency CreateDependencyAndInitialize<TDefault1, TDefault2>(
			Func<TDefault1> createDefault1,
			object[] args1,
			Func<TDefault2> createDefault2,
			object[] args2)
			where TDefault1 : TDefault2
			where TDefault2 : TDependency
		{
			var obj = CreateDependency(createDefault1, args1, createDefault2, args2);
			return Initialize(obj);
		}

		protected TDependency CreateDependency<TDefault1, TDefault2>(
			Func<TDefault1> createDefault1,
			object[] args1,
			Func<TDefault2> createDefault2,
			object[] args2)
			where TDefault1 : TDefault2
			where TDefault2 : TDependency
		{
			var type = GetDependencyType();
			var obj = CreateDependency(type, createDefault1, args1, createDefault2, args2);
			return obj;
		}

		private static TDependency CreateDependency<TDefault1, TDefault2>(
			Type type,
			Func<TDefault1> createDefault1,
			object[] args1,
			Func<TDefault2> createDefault2,
			object[] args2)
			where TDefault1 : TDefault2
			where TDefault2 : TDependency
		{
			if (type == typeof(TDefault1)) return createDefault1();
			if (type.IsA(typeof(TDefault1))) return (TDependency)Activator.CreateInstance(type, args1);
			if (type == typeof(TDefault2)) return createDefault2();
			if (type.IsA(typeof(TDefault2))) return (TDependency)Activator.CreateInstance(type, args2);
			return (TDependency)Activator.CreateInstance(type);
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		protected override void Reset(ConfigurationElement parentElement)
		{
			base.Reset(parentElement);
			_dependencyType = null;
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
		{
			AddConfig(name, value);
			return true;
		}

		/// <summary>
		/// Adds a setting.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddConfig(string name, string value)
		{
			Parameters[name] = value;
		}

		/// <summary>
		/// Initializes the dependency object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public TDependency Initialize(TDependency obj)
		{
			TryInitialize(obj);
			return obj;
		}

		/// <summary>
		/// Initializes the dependency object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool TryInitialize(object obj)
		{
			var init = obj as IInitializable;

			if (init != null)
			{
				init.Initialize(Name, Parameters);
				return true;
			}

			return false;
		}
	}
}
