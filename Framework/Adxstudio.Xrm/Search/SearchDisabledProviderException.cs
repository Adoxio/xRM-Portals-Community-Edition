/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration.Provider;

namespace Adxstudio.Xrm.Search
{
	internal class SearchDisabledProviderException : ProviderException
	{
		public SearchDisabledProviderException(string message) : base(message)
		{
		}
	}
}
