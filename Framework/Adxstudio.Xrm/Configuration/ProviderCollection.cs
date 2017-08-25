/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration.Provider;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Configuration
{
	public class ProviderCollection<T> : ProviderCollection where T : ProviderBase
	{
		public override void Add(ProviderBase provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			if (!(provider is T))
			{
				throw new ArgumentException("Invalid provider.");
			}

			Add(provider as T);
		}

		public void Add(T provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			base.Add(provider);
		}

		public void AddArray(T[] providerArray)
		{
			if (providerArray == null)
			{
				throw new ArgumentNullException("providerArray");
			}

			for (int i = 0; i < providerArray.Length; ++i)
			{
				T provider = providerArray[i];

				if (this[provider.Name] != null)
				{
					throw new ArgumentException("Duplicate provider name.");
				}

				Add(provider);
			}
		}

		public new T this[string name]
		{
			get
			{
				return base[name] as T;
			}
		}

		public T this[Type type]
		{
			get
			{
				foreach (T provider in this)
				{
					if (provider.GetType() == type)
					{
						return provider;
					}
				}

				return null;
			}
		}
	}
}
