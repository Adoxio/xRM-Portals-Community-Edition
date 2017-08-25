/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// Encryption key provider
	/// </summary>
	public interface ICertificateProvider
	{
		/// <summary>
		/// Public Key Thumbprint
		/// </summary>
		string Thumbprint { get; }

		/// <summary>
		/// Sets the cert.
		/// </summary>
		/// <returns></returns>
		X509Certificate2 Certificate { get; }
	}
}
