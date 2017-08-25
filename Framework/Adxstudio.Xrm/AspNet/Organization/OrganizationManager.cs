/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet.Organization
{
	public class OrganizationManager : BaseManager<IOrganizationStore>
	{
		public OrganizationManager(IOrganizationStore store)
			: base(store)
		{
		}

		public Task<IDictionary<string, object>> InvokeProcessAsync(string name, EntityReference regardingId, IDictionary<string, object> parameters)
		{
			ThrowIfDisposed();

			if (name == null) throw new ArgumentNullException("name");

			return Store.InvokeProcessAsync(name, regardingId, parameters);
		}
	}
}
