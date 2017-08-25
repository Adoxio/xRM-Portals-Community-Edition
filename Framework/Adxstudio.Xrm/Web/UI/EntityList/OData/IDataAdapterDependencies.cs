/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.EntityList.OData
{
	/// <summary>
	/// Represents dependencies for the data adapter
	/// </summary>
	public interface IDataAdapterDependencies
	{
		/// <summary>
		/// Get the <see cref="OrganizationServiceContext"/> to execute queries.
		/// </summary>
		/// <returns><see cref="OrganizationServiceContext"/></returns>
		OrganizationServiceContext GetServiceContext();

		/// <summary>
		/// Get an <see cref="EntityReference"/> to the current website
		/// </summary>
		/// <returns><see cref="EntityReference"/> to a website record.</returns>
		EntityReference GetWebsite();
	}
}
