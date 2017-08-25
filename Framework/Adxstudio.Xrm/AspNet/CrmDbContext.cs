/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet
{
	public abstract class OrganizationServiceManager
	{
		public abstract IOrganizationService Create();
	}

	public class CrmDbContext : IDisposable
	{
		public IOrganizationService Service { get; private set; }
		public bool DisposeContext { get; set; }

		public CrmDbContext(OrganizationServiceManager serviceManager)
			: this(serviceManager.Create())
		{
			DisposeContext = true;
		}

		public CrmDbContext(IOrganizationService service)
		{
			if (service == null) throw new ArgumentNullException("service");

			Service = service;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (DisposeContext && disposing)
			{
				var service = Service as IDisposable;

				if (service != null)
				{
					service.Dispose();
					Service = null;
				}
			}
		}
	}
}
