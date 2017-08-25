/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;

	/// <summary>
	/// Null reference exceptions which are transient.
	/// </summary>
	public class TransientNullReferenceException : NullReferenceException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransientNullReferenceException" /> class.
		/// </summary>
		/// <param name="message"> The exception message</param>
		public TransientNullReferenceException(string message)
			: base(message)
		{
		}
	}
}
