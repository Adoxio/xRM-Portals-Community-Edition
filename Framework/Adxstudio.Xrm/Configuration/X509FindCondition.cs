/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Configuration
{
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// A certificate store search filter.
	/// </summary>
	public struct X509FindCondition
	{
		/// <summary>
		/// The type of value to search for.
		/// </summary>
		public readonly X509FindType FindType;

		/// <summary>
		/// The value to search for.
		/// </summary>
		public readonly object FindValue;

		/// <summary>
		/// Only returns valid certificates.
		/// </summary>
		public readonly bool ValidOnly;

		/// <summary>
		/// Initializes a new instance of the <see cref="X509FindCondition" /> struct.
		/// </summary>
		/// <param name="findType">The type of value to search for.</param>
		/// <param name="findValue">The value to search for.</param>
		/// <param name="validOnly">Only returns valid certificates.</param>
		public X509FindCondition(X509FindType findType, object findValue, bool validOnly = false)
		{
			this.FindType = findType;
			this.FindValue = findValue;
			this.ValidOnly = validOnly;
		}
	}
}
