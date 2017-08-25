/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// Helpers for certificate management.
	/// </summary>
	public static class CertificateExtensions
	{
		/// <summary>
		/// Finds certificates from the certificate store.
		/// </summary>
		/// <param name="conditions">The search conditions.</param>
		/// <param name="storeLocation">The store location.</param>
		/// <param name="storeName">The store name.</param>
		/// <returns>The matching certificates.</returns>
		public static X509Certificate2Collection FindCertificates(
			this IEnumerable<X509FindCondition> conditions,
			StoreLocation storeLocation = StoreLocation.CurrentUser,
			StoreName storeName = StoreName.My)
		{
			var store = new X509Store(storeName, storeLocation);

			try
			{
				store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);

				var matches = conditions.Aggregate(
					store.Certificates,
					(certifiates, condition) => certifiates.Find(condition.FindType, condition.FindValue, condition.ValidOnly));

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Found '{0}' certificate(s).", matches.Count));

				return matches;
			}
			finally
			{
				store.Close();
			}
		}

		/// <summary>
		/// Finds certificates from the certificate store by thumbprint.
		/// </summary>
		/// <param name="thumbprint">The certificate thumbprint.</param>
		/// <param name="findByTimeValid">Only returns non-expired certificates.</param>
		/// <param name="validOnly">Only returns valid certificates.</param>
		/// <returns>The matching certificates.</returns>
		public static X509Certificate2Collection FindCertificatesByThumbprint(this string thumbprint, bool findByTimeValid = true, bool validOnly = false)
		{
			if (string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentNullException("thumbprint");
			}

			var thumbprintCondition = new X509FindCondition(X509FindType.FindByThumbprint, thumbprint, validOnly);
			var timeCondition = new X509FindCondition(X509FindType.FindByTimeValid, DateTime.UtcNow, validOnly);

			var conditions = findByTimeValid
				? new[] { thumbprintCondition, timeCondition }
				: new[] { thumbprintCondition };

			return FindCertificates(conditions);
		}

		/// <summary>
		/// Finds certificates from the certificate store by a collection of thumbprints.
		/// </summary>
		/// <param name="thumbprints">The certificate thumbprint.</param>
		/// <param name="findByTimeValid">Only returns non-expired certificates.</param>
		/// <param name="validOnly">Only returns valid certificates.</param>
		/// <returns>The matching certificates.</returns>
		public static IEnumerable<X509Certificate2> FindCertificatesByThumbprint(this IEnumerable<string> thumbprints, bool findByTimeValid = true, bool validOnly = false)
		{
			var certificates = thumbprints
				.Where(thumbprint => !string.IsNullOrWhiteSpace(thumbprint))
				.SelectMany(thumbprint => thumbprint.FindCertificatesByThumbprint(findByTimeValid, validOnly).OfType<X509Certificate2>());

			return certificates;
		}

		/// <summary>
		/// Finds certificates from the certificate store by a collection of thumbprints.
		/// </summary>
		/// <param name="settings">The portal settings.</param>
		/// <returns>The matching certificates.</returns>
		public static IEnumerable<X509Certificate2> FindCertificates(this ICertificateSettings settings)
		{
			var thumbprints = new[] { settings.ThumbprintPrimary, settings.ThumbprintSecondary };

			var certificates = FindCertificatesByThumbprint(thumbprints, settings.FindByTimeValid);

			return certificates;
		}
	}
}
