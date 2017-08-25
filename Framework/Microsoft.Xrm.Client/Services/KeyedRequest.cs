/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// A <see cref="OrganizationRequest"/> wrapper that contains a custom key value.
	/// </summary>
	/// <remarks>
	/// The key value is used by the <see cref="OrganizationServiceCache"/> as the cache key when inserting or retrieving cache items.
	/// </remarks>
	[Serializable]
	public sealed class KeyedRequest : OrganizationRequest
	{
		/// <summary>
		/// The underlying request.
		/// </summary>
		public OrganizationRequest Request { get; set; }

		/// <summary>
		/// A custom key value that is unique to the current request.
		/// </summary>
		public string Key { get; set; }

		public KeyedRequest()
		{
		}

		public KeyedRequest(OrganizationRequest request, string key)
		{
			Request = request;
			Key = key;

			ExtensionData = request.ExtensionData;
			Parameters = request.Parameters;
			RequestId = request.RequestId;
			RequestName = request.RequestName;
		}
	}
}
