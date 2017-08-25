/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	/// <summary>
	/// Key provider factory
	/// </summary>
	public class CertificateProviderFactory : ICertificateProviderFactory
	{
		/// <summary>
		/// Factory method to initiate proper key provider
		/// </summary>
		/// <param name="isOnline">specifies whether we are in online mode</param>
		/// <returns>key provider</returns>
		public ICertificateProvider GetCertificateProvider(bool isOnline)
		{
			if (isOnline)
			{
				return new PortalCertificateProvider();
			}
			return new OnPremCertificateProvider();
		}
	}
}
