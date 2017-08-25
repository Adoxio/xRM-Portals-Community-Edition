/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class CrmUserLogin
	{
		public virtual Entity Entity { get; private set; }

		public virtual string LoginProvider
		{
			get { return Entity.GetAttributeValue<string>("adx_identityprovidername"); }
		}

		public virtual string ProviderKey
		{
			get { return Entity.GetAttributeValue<string>("adx_username"); }
		}

		public CrmUserLogin(Entity entity)
		{
			Entity = entity;
		}
	}
}
