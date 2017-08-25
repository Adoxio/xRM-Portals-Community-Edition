/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Store
{
	public class DefaultDirectoryFactory : IDirectoryFactory
	{
		public Directory GetDirectory(Version version)
		{
			return new RAMDirectory();
		}
	}
}
