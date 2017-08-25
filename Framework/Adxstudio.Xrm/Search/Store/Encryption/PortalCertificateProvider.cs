/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// This key provider takes keys from Portal certificate which is used for CRM authentication
	/// </summary>
	public class PortalCertificateProvider : ICertificateProvider
	{
		/// <summary>
		/// Contains X509 Portal Certificate
		/// </summary>
		private X509Certificate2 certificate;

		/// <summary>
		/// Initializes a new instance of the <see cref="PortalCertificateProvider" /> class.
		/// </summary>
		public PortalCertificateProvider()
		{
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
		public void GetCert()
		{
			X509Store store = null;

			try
			{
				store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);

				var thumbprint = Xrm.Configuration.PortalSettings.Instance.Certificate.ThumbprintPrimary;

				var result = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

				if (result.Count == 0)
				{
					throw new NotImplementedException("Not able to get encryption certificate");
				}

				this.certificate = result[0];


				if (!this.certificate.HasPrivateKey)
				{
					throw new InvalidOperationException("Certificate does not contain private key");
				}
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
