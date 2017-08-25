/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Client.Runtime
{
	internal static class ObjectExtensions
	{
		public static void ThrowOnNull(this object obj, string paramName, string message = null)
		{
			if (obj == null) throw new ArgumentNullException(paramName, message);
		}

		public static void ThrowOnNullOrWhitespace(this string obj, string paramName, string message = null)
		{
			if (string.IsNullOrWhiteSpace(obj)) throw new ArgumentNullException(paramName, message);
		}
	}
}
