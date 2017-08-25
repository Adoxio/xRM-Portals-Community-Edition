/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.Configuration;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// This key provider use Windows CNG storage faciliteis to generate and store key.
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/bb204778(v=vs.85).aspx
	/// </summary>
	public class OnPremCertificateProvider : ICertificateProvider
	{
		/// <summary>
		/// The certificate
		/// </summary>
		private X509Certificate2 certificate;

		/// <summary>
		/// Initializes a new instance of the <see cref="OnPremCertificateProvider" /> class.
		/// </summary>
		public OnPremCertificateProvider()
		{
			// Geting certificate
			this.GetCert();
		}

		/// <summary>
		/// Public Key Thumbprint
		/// </summary>
		public string Thumbprint
		{
			get
			{
				return this.certificate.Thumbprint;
			}
		}

		/// <summary>
		/// Gets the certificate provider.
		/// </summary>
		/// <value>
		/// The certificate provider.
		/// </value>
		public X509Certificate2 Certificate
		{
			get
			{
				return this.certificate;
			}
		}

		/// <summary>
		/// Sets the cert.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException">Not able to get encryption certificate</exception>
		private void GetCert()
		{
			X509Store store = null;

			try
			{
				store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);

				////Temporary using this certificate for development purposes only
				var certificateThumbprint = ConfigurationManager.AppSettings["OnPremCertificateThumbprint"];

				var result = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);

				if (result.Count == 0)
				{
					throw new NotImplementedException("Not able to get encryption certificate");
				}

				this.certificate = result[0];
			}
			finally
			{
				if (store != null)
				{
					store.Close();
				}
			}
		}
	}
}
