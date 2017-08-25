/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Configuration
{
	using System;

	/// <summary>
	/// Indicates that a valid certificate could not be retrieved.
	/// </summary>
	public class CertificateNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateNotFoundException" /> class.
		/// </summary>
		public CertificateNotFoundException()
			: base("Certificate not found.")
		{
		}
	}
}
