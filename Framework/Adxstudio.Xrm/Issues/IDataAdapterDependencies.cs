/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides dependencies for the various data adapters within the Adxstudio.Xrm.Issues namespace.
	/// </summary>
	public interface IDataAdapterDependencies : Adxstudio.Xrm.Cms.IDataAdapterDependencies
	{
		/// <summary>
		/// Returns an <see cref="HttpContextBase"/>.
		/// </summary>
		HttpContextBase GetHttpContext();
		
	}
}
