/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Lucene.Net.Util;
using Directory=Lucene.Net.Store.Directory;

namespace Adxstudio.Xrm.Search.Store
{
	public interface IDirectoryFactory
	{
		Directory GetDirectory(Version version);
	}
}
