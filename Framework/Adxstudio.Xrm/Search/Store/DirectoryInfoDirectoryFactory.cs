/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Adxstudio.Xrm.Search.Store
{
	using Encryption;

	public class DirectoryInfoDirectoryFactory : IDirectoryFactory
	{
		private readonly DirectoryInfo _directory;
		private readonly bool _useEncryptedDirectory;
		private readonly bool _isOnlinePortal;

		public DirectoryInfoDirectoryFactory(DirectoryInfo directoryInfo, bool useEncryptedDirectory = false, bool isOnlinePortal = false)
		{
			if (directoryInfo == null)
			{
				throw new ArgumentNullException("directoryInfo");
			}

			_directory = directoryInfo;
			this._useEncryptedDirectory = useEncryptedDirectory;
			this._isOnlinePortal = isOnlinePortal;
		}

		public Directory GetDirectory(Version version)
		{
			if (!_useEncryptedDirectory)
			{
				return FSDirectory.Open(this._directory);
			}

			var certificateProvider = new CertificateProviderFactory().GetCertificateProvider(this._isOnlinePortal);
			var directoryPath = string.Format("{0}.{1}", this._directory.FullName, certificateProvider.Thumbprint);

			var directory = new EncryptedDirectory(directoryPath, certificateProvider.Certificate);

			return directory;
		}
	}
}
