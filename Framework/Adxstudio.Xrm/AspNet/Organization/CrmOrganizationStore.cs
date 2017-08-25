/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet.Organization
{
	public interface IOrganizationStore : IDisposable
	{
		Task<IDictionary<string, object>> InvokeProcessAsync(string name, EntityReference regardingId, IDictionary<string, object> parameters);
	}

	public class CrmOrganizationStore : BaseStore, IOrganizationStore
	{
		public CrmOrganizationStore(CrmDbContext context)
			: base(context)
		{
		}

		#region IOrganizationStore

		public async Task<IDictionary<string, object>> InvokeProcessAsync(string name, EntityReference regardingId, IDictionary<string, object> parameters)
		{
			ThrowIfDisposed();

			if (name == null) throw new ArgumentNullException("name");

			var request = new OrganizationRequest(name);

			if (regardingId != null)
			{
				request["Target"] = regardingId;
			}

			if (parameters != null)
			{
				foreach (var parameter in parameters)
				{
					request[parameter.Key] = parameter.Value;
				}
			}

			var response = await ExecuteAsync(request).WithCurrentCulture();

			return new Dictionary<string, object>(response.Results.ToDictionary(pair => pair.Key, pair => pair.Value));
		}

		#endregion
	}
}
