/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.IO;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory=Lucene.Net.Store.Directory;

namespace Adxstudio.Xrm.Search.Store
{
	public class AppSettingDirectoryFactory : IDirectoryFactory
	{
		public Directory GetDirectory(Version version)
		{
			var indexPathSetting = ConfigurationManager.AppSettings["Adxstudio.Xrm.Search.Index.DirectoryPath"];

			return string.IsNullOrEmpty(indexPathSetting) ? null : FSDirectory.Open(new DirectoryInfo(indexPathSetting));
		}
	}
}
