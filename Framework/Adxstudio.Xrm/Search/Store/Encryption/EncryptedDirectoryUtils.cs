/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.IO;
	using System.Web.Hosting;

	/// <summary>
	/// Encrypted Directory Utils
	/// </summary>
	public static class EncryptedDirectoryUtils
	{
		/// <summary>
		/// Drops indexes different from current
		/// </summary>
		/// <param name="indexPath">path to index location</param>
		/// <param name="isOnline">online deployment mode or not</param>
		public static void CleanupLegacy(string indexPath, bool isOnline)
		{
			var indexDirectory = new DirectoryInfo(
				(indexPath.StartsWith(@"~/", StringComparison.Ordinal) || indexPath.StartsWith(@"~\", StringComparison.Ordinal))
					? HostingEnvironment.MapPath(indexPath) ?? indexPath
					: indexPath);

			var certificateProvider = new CertificateProviderFactory().GetCertificateProvider(isOnline);

			var encryptedIndexPattern = string.Format("{0}.*", indexDirectory.Name);
			var currentIndexFolder = string.Format("{0}.{1}", indexDirectory.Name, certificateProvider.Thumbprint);

			if (indexDirectory.Parent == null || !indexDirectory.Parent.Exists)
			{
				return;
			}

			foreach (var folder in indexDirectory.Parent.EnumerateDirectories(encryptedIndexPattern))
			{
				if (folder.Name.Equals(currentIndexFolder))
				{
					continue;
				}

				try
				{
					folder.Delete(true);
				}
				catch (IOException)
				{
					// it is not critical
				}
			}

		}
	}
}
