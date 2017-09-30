/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Store;
using Version=Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Store
{
	public class CompositeDirectoryFactory : IDirectoryFactory
	{
		private readonly IEnumerable<IDirectoryFactory> _factories;

		public CompositeDirectoryFactory(IEnumerable<IDirectoryFactory> factories)
		{
			if (factories == null)
			{
				throw new ArgumentNullException("factories");
			}

			_factories = factories;
		}

		public CompositeDirectoryFactory(params IDirectoryFactory[] factories) : this(factories as IEnumerable<IDirectoryFactory>) { }

		public Directory GetDirectory(Version version)
		{
			return _factories.Select(factory => factory.GetDirectory(version)).FirstOrDefault(directory => directory != null);
		}
	}
}
